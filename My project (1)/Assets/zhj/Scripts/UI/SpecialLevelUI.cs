// SpecialLevelUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpecialLevelUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject specialLevelPanel;
    [SerializeField] private TextMeshProUGUI specialLevelText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image backgroundImage;
    
    [Header("动画设置")]
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float fadeInTime = 0.8f;
    [SerializeField] private float fadeOutTime = 0.5f;
    
    [Header("视觉效果")]
    [SerializeField] private Color[] backgroundColors; // 不同等级的背景颜色
    [SerializeField] private AudioClip specialLevelSound;
    [SerializeField] private ParticleSystem celebrationParticles;
    [ContextMenu("测试特殊等级提示")]
    public void TestSpecialLevel()
    {
    ShowSpecialLevel(10);
    }
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private bool isShowing = false;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        canvasGroup = specialLevelPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = specialLevelPanel.AddComponent<CanvasGroup>();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始隐藏
        specialLevelPanel.SetActive(false);
        
        // 绑定按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }
    
    private void Start()
    {
        // 注册事件监听
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onSpecialLevelReached.AddListener(ShowSpecialLevel);
        }
    }
    
    /// <summary>
    /// 显示特殊等级提示
    /// </summary>
    public void ShowSpecialLevel(int level)
    {
        if (isShowing) 
        {
            // 如果正在显示，延迟执行
            StartCoroutine(DelayedShow(level, 1f));
            return;
        }
        
        StartCoroutine(ShowSpecialLevelCoroutine(level));
    }
    
    private IEnumerator DelayedShow(int level, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(ShowSpecialLevelCoroutine(level));
    }
    
    private IEnumerator ShowSpecialLevelCoroutine(int level)
    {
        isShowing = true;
        
        // 更新UI内容
        UpdateUIForLevel(level);
        
        // 显示面板
        specialLevelPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        
        // 播放音效
        if (specialLevelSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(specialLevelSound);
        }
        
        // 播放粒子效果
        if (celebrationParticles != null)
        {
            celebrationParticles.Play();
        }
        
        // 淡入动画
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // 暂停游戏（可选）
        // Time.timeScale = 0f;
        
        // 显示一段时间
        yield return new WaitForSeconds(showDuration);
        
        // 自动隐藏
        Hide();
    }
    
    /// <summary>
    /// 根据等级更新UI
    /// </summary>
    private void UpdateUIForLevel(int level)
    {
        if (specialLevelText != null)
        {
            specialLevelText.text = $"等级里程碑！\n<size=72>Lv. {level}</size>";
        }
        
        if (messageText != null && LevelManager.Instance != null)
        {
            messageText.text = LevelManager.Instance.GetSpecialLevelMessage(level);
        }
        
        // 设置背景颜色
        if (backgroundImage != null && backgroundColors != null && backgroundColors.Length > 0)
        {
            int colorIndex = Mathf.Clamp(level / 10 - 1, 0, backgroundColors.Length - 1);
            backgroundImage.color = backgroundColors[colorIndex];
        }
    }
    
    /// <summary>
    /// 隐藏UI
    /// </summary>
    public void Hide()
    {
        if (!isShowing) return;
        
        StartCoroutine(FadeOutAndHide());
    }
    
    private IEnumerator FadeOutAndHide()
    {
        // 淡出动画
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < fadeOutTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        specialLevelPanel.SetActive(false);
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        isShowing = false;
    }
    
    /// <summary>
    /// 强制立即显示（用于测试）
    /// </summary>
    [ContextMenu("测试显示10级提示")]
    public void TestShowLevel10()
    {
        ShowSpecialLevel(10);
    }
    
    [ContextMenu("测试显示20级提示")]
    public void TestShowLevel20()
    {
        ShowSpecialLevel(20);
    }
}