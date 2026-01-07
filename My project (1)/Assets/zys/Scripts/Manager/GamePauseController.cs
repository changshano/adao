using UnityEngine;
using UnityEngine.UI;

public class GamePauseController : MonoBehaviour
{
    [Header("UI 按钮组件")]
    [SerializeField] private Button pauseButton;      // 暂停按钮
    [SerializeField] private Button resumeButton;     // 继续按钮

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
    private System.Collections.IEnumerator FadeOverlay(float startAlpha, float endAlpha, float duration, bool disableOnComplete = false)
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
}