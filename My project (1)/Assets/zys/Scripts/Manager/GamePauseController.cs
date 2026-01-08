using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // 添加场景管理命名空间
using System.Collections;           // 添加协程命名空间

public class GamePauseController : MonoBehaviour
{
    [Header("UI 按钮组件")]
    [SerializeField] private Button pauseButton;      // 暂停按钮
    [SerializeField] private Button resumeButton;    // 继续按钮
    [SerializeField] private Button restartButton;   // 重新开始按钮
    [SerializeField] private Button exitButton;      // 退出游戏按钮

    [Header("暂停相关设置")]
    [SerializeField] private float timeScaleWhenPaused = 0f;  // 暂停时的时间缩放

    [Header("屏幕变暗效果")]
    [SerializeField] private Image darkOverlay;       // 黑色遮罩图片
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.7f);  // 遮罩颜色和透明度

    [Header("渐变效果设置")]
    [SerializeField] private float fadeDuration = 0.3f;  // 渐变时间
    [SerializeField] private bool useFadeEffect = true;  // 是否使用渐变效果

    [Header("玩家控制")]
    [SerializeField] private MonoBehaviour playerController;  // 玩家控制器脚本

    [Header("游戏控制")]
    [SerializeField] private string mainMenuScene = "MainMenu";  // 主菜单场景名称
    [SerializeField] private float sceneChangeDelay = 0.2f;      // 场景切换延迟

    private bool isPaused = false;  // 游戏当前是否暂停
    private Coroutine fadeCoroutine;  // 渐变协程引用

    private void Start()
    {
        // 确保游戏开始时处于运行状态
        ResumeGame();

        // 为按钮添加点击事件监听
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        // 初始化遮罩状态
        if (darkOverlay != null)
        {
            // 确保遮罩初始不可见
            darkOverlay.gameObject.SetActive(false);
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        }

        // 初始化按钮状态
        UpdateButtonVisibility();
    }

    private void OnDestroy()
    {
        // 清理事件监听，防止内存泄漏
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitButtonClicked);
        }
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;  // 已经是暂停状态则不重复操作

        isPaused = true;
        Time.timeScale = timeScaleWhenPaused;  // 设置为0时完全暂停

        // 显示/隐藏按钮
        UpdateButtonVisibility();

        // 显示屏幕变暗效果
        ShowDarkOverlay();

        // 禁用玩家控制
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Debug.Log("游戏已暂停，屏幕变暗效果已启用");
    }

    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;  // 已经是运行状态则不重复操作

        isPaused = false;
        Time.timeScale = 1f;  // 恢复正常时间流速

        // 显示/隐藏按钮
        UpdateButtonVisibility();

        // 隐藏屏幕变暗效果
        HideDarkOverlay();

        // 启用玩家控制
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        Debug.Log("游戏已继续，屏幕变暗效果已禁用");
    }

    /// <summary>
    /// 点击重新开始按钮
    /// </summary>
    public void OnRestartButtonClicked()
    {
        Debug.Log("重新开始游戏");
        RestartGame();
    }

    /// <summary>
    /// 点击退出游戏按钮
    /// </summary>
    public void OnExitButtonClicked()
    {
        Debug.Log("退出游戏");
        ExitGame();
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 先恢复游戏时间
        Time.timeScale = 1f;

        // 获取当前场景索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 重新加载当前场景
        StartCoroutine(LoadSceneWithDelay(currentSceneIndex, sceneChangeDelay));
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        // 如果有指定主菜单场景，则加载主菜单
        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            StartCoroutine(LoadSceneWithDelay(mainMenuScene, sceneChangeDelay));
        }
        else
        {
            // 否则加载第一个场景
            StartCoroutine(LoadSceneWithDelay(0, sceneChangeDelay));
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        Debug.Log("退出游戏...");

#if UNITY_EDITOR
        // 在Unity编辑器中
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建的游戏/应用程序中
        Application.Quit();
#endif
    }

    /// <summary>
    /// 带延迟加载场景
    /// </summary>
    private IEnumerator LoadSceneWithDelay(int sceneIndex, float delay)
    {
        // 等待一小段时间，让过渡更自然
        yield return new WaitForSecondsRealtime(delay);

        // 加载场景
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// 带延迟加载场景（通过名称）
    /// </summary>
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 显示屏幕变暗遮罩
    /// </summary>
    private void ShowDarkOverlay()
    {
        if (darkOverlay == null) return;

        if (useFadeEffect && fadeDuration > 0)
        {
            // 使用渐变效果
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            darkOverlay.gameObject.SetActive(true);
            fadeCoroutine = StartCoroutine(FadeOverlay(0, overlayColor.a, fadeDuration));
        }
        else
        {
            // 直接显示
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.color = overlayColor;
        }
    }

    /// <summary>
    /// 隐藏屏幕变暗遮罩
    /// </summary>
    private void HideDarkOverlay()
    {
        if (darkOverlay == null) return;

        if (useFadeEffect && fadeDuration > 0)
        {
            // 使用渐变效果
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOverlay(overlayColor.a, 0, fadeDuration, true));
        }
        else
        {
            // 直接隐藏
            darkOverlay.gameObject.SetActive(false);
            darkOverlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0);
        }
    }

    /// <summary>
    /// 遮罩渐变协程
    /// </summary>
    private IEnumerator FadeOverlay(float startAlpha, float endAlpha, float duration, bool disableOnComplete = false)
    {
        float elapsedTime = 0f;
        Color startColor = darkOverlay.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;  // 使用不受时间缩放影响的时间
            float t = elapsedTime / duration;
            darkOverlay.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        darkOverlay.color = endColor;

        if (disableOnComplete && darkOverlay.gameObject.activeSelf)
        {
            darkOverlay.gameObject.SetActive(false);
        }

        fadeCoroutine = null;
    }

    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// 更新按钮的显示/隐藏状态
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(!isPaused);  // 游戏运行时显示暂停按钮
        }

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(isPaused);  // 游戏暂停时显示继续按钮
        }

        // 重新开始和退出按钮只在暂停时显示
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(isPaused);
        }

        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(isPaused);
        }
    }

    /// <summary>
    /// 在编辑器中测试暂停功能
    /// </summary>
    [ContextMenu("测试暂停")]
    private void TestPause()
    {
        PauseGame();
    }

    [ContextMenu("测试继续")]
    private void TestResume()
    {
        ResumeGame();
    }

    /// <summary>
    /// 在编辑器中测试重新开始功能
    /// </summary>
    [ContextMenu("测试重新开始")]
    private void TestRestart()
    {
        RestartGame();
    }

    /// <summary>
    /// 在编辑器中测试退出功能
    /// </summary>
    [ContextMenu("测试退出游戏")]
    private void TestExit()
    {
        ExitGame();
    }
}