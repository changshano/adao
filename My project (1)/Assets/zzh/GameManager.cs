using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button quitButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 移除 DontDestroyOnLoad
            // DontDestroyOnLoad(gameObject); // ← 删除这行
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始隐藏Game Over界面
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 绑定按钮事件
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    // 游戏结束方法（在其他脚本中调用）
    public void GameOver()
    {
        // 暂停游戏时间
        Time.timeScale = 0f;

        // 显示Game Over界面
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // 播放音效（可选）
        // AudioManager.Instance.PlaySound("gameOver");
    }

    // 重新开始游戏
    public void RestartGame()
    {
        // 恢复游戏时间
        Time.timeScale = 1f;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // 或者加载指定场景
        // SceneManager.LoadScene("YourSceneName");
    }

    // 退出游戏
    public void QuitGame()
    {
        // 在编辑器中停止播放
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在构建版本中退出游戏
        Application.Quit();
#endif
    }
}
