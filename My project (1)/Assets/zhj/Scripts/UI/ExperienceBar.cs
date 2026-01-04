// ExperienceGrowthSystem/UI/ExperienceBar.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExperienceBar : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Image fillImage;
    
    [Header("颜色设置")]
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color nearLevelUpColor = Color.yellow;
    [SerializeField] private Color levelUpColor = Color.green;
    
    [Header("动画设置")]
    [SerializeField] private float fillSpeed = 5f;
    [SerializeField] private float levelUpEffectDuration = 1f;
    
    [Header("调试")]
    [SerializeField] private bool debugMode = false;
    
    private float targetFillAmount = 0f;
    private bool isLevelingUp = false;
    private int currentLevel = 1;
    private bool isInitialized = false;
    
    private void Awake()
    {
        // 确保UI组件存在
        ValidateUIComponents();
    }
    
    private void Start()
    {
        // 延迟初始化，确保管理器已就绪
        StartCoroutine(DelayedInitialize());
    }
    
    private IEnumerator DelayedInitialize()
    {
        // 等待一帧，确保所有管理器都初始化完成
        yield return null;
        
        Initialize();
    }
    
    /// <summary>
    /// 初始化经验条
    /// </summary>
    public void Initialize()
    {
        ValidateUIComponents();
        
        // 注册事件监听
        RegisterEvents();
        
        // 初始更新
        UpdateUI();
        
        isInitialized = true;
        
        if (debugMode) Debug.Log("ExperienceBar 初始化完成");
    }
    
    /// <summary>
    /// 验证UI组件
    /// </summary>
    private void ValidateUIComponents()
    {
        if (experienceSlider == null)
        {
            experienceSlider = GetComponentInChildren<Slider>();
            if (experienceSlider == null)
            {
                Debug.LogError("ExperienceBar: 找不到Slider组件！");
            }
        }
        
        if (levelText == null)
        {
            // 尝试查找等级文本
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.Contains("Lv.") || text.text.Contains("Level") || text.name.Contains("Level"))
                {
                    levelText = text;
                    break;
                }
            }
            
            if (levelText == null && texts.Length > 0)
            {
                levelText = texts[0]; // 使用第一个找到的文本
            }
        }
        
        if (expText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text != levelText && (text.text.Contains("/") || text.name.Contains("Exp")))
                {
                    expText = text;
                    break;
                }
            }
        }
        
        if (fillImage == null && experienceSlider != null)
        {
            fillImage = experienceSlider.fillRect?.GetComponent<Image>();
        }
    }
    
    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEvents()
    {
        // 移除之前的事件监听（避免重复）
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.onExperienceChanged.RemoveListener(OnExperienceChanged);
            ExperienceManager.Instance.onExperienceChanged.AddListener(OnExperienceChanged);
        }
        else
        {
            Debug.LogWarning("ExperienceManager 未找到，将延迟注册事件");
            StartCoroutine(DelayedRegisterEvents());
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
            LevelManager.Instance.onLevelUp.AddListener(OnLevelUp);
            
            // 也监听详细升级事件
            LevelManager.Instance.onLevelUpDetailed.RemoveListener(OnLevelUpDetailed);
            LevelManager.Instance.onLevelUpDetailed.AddListener(OnLevelUpDetailed);
        }
        else
        {
            Debug.LogWarning("LevelManager 未找到，将延迟注册事件");
            StartCoroutine(DelayedRegisterEvents());
        }
    }
    
    private IEnumerator DelayedRegisterEvents()
    {
        int maxAttempts = 10;
        int attempts = 0;
        
        while (attempts < maxAttempts)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (ExperienceManager.Instance != null && LevelManager.Instance != null)
            {
                RegisterEvents();
                UpdateUI();
                break;
            }
            
            attempts++;
            
            if (debugMode) Debug.Log($"尝试注册事件 ({attempts}/{maxAttempts})");
        }
        
        if (attempts >= maxAttempts)
        {
            Debug.LogError("无法注册到经验/等级管理器，请确保它们已添加到场景中");
        }
    }
    
    private void Update()
    {
        if (experienceSlider != null)
        {
            // 平滑填充经验条
            experienceSlider.value = Mathf.Lerp(experienceSlider.value, targetFillAmount, Time.deltaTime * fillSpeed);
            
            // 更新颜色
            UpdateFillColor();
        }
    }
    
    /// <summary>
    /// 经验变化时调用
    /// </summary>
    private void OnExperienceChanged(int currentExp, int expToNextLevel)
    {
        if (expToNextLevel <= 0) return;
        
        float progress = (float)currentExp / expToNextLevel;
        targetFillAmount = progress;
        
        UpdateExpText(currentExp, expToNextLevel);
        
        if (debugMode) Debug.Log($"经验更新: {currentExp}/{expToNextLevel} ({progress:P0})");
    }
    
    /// <summary>
    /// 等级提升时调用
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        UpdateLevelText(newLevel);
        StartCoroutine(LevelUpEffect());
        
        if (debugMode) Debug.Log($"等级提升: {newLevel}");
    }
    
    /// <summary>
    /// 详细等级提升事件
    /// </summary>
    private void OnLevelUpDetailed(int newLevel, int skillPoints, int attributePoints)
    {
        // 这里可以添加额外的处理，比如显示获得的点数
        UpdateLevelText(newLevel);
        
        if (debugMode) Debug.Log($"详细等级提升: 等级{newLevel}, 技能点{skillPoints}, 属性点{attributePoints}");
    }
    
    /// <summary>
    /// 更新等级文本
    /// </summary>
    private void UpdateLevelText(int newLevel)
    {
        currentLevel = newLevel;
        
        if (levelText != null)
        {
            levelText.text = $"Lv.{newLevel}";
        }
        else
        {
            Debug.LogWarning("等级文本组件未设置");
        }
    }
    
    /// <summary>
    /// 更新经验文本
    /// </summary>
    private void UpdateExpText(int currentExp, int expToNextLevel)
    {
        if (expText != null)
        {
            expText.text = $"{currentExp} / {expToNextLevel}";
        }
    }
    
    /// <summary>
    /// 更新填充颜色
    /// </summary>
    private void UpdateFillColor()
    {
        if (fillImage == null) return;
        
        float progress = experienceSlider.value;
        
        if (isLevelingUp)
        {
            // 升级时的闪烁效果
            float pingPong = Mathf.PingPong(Time.time * 10f, 1f);
            fillImage.color = Color.Lerp(levelUpColor, Color.white, pingPong);
        }
        else if (progress > 0.8f)
        {
            // 接近升级时的黄色
            fillImage.color = Color.Lerp(normalColor, nearLevelUpColor, (progress - 0.8f) * 5f);
        }
        else
        {
            fillImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// 升级特效
    /// </summary>
    private IEnumerator LevelUpEffect()
    {
        isLevelingUp = true;
        
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (elapsedTime < levelUpEffectDuration)
        {
            float scaleMultiplier = 1f + Mathf.Sin(elapsedTime * Mathf.PI * 2f) * 0.1f;
            transform.localScale = originalScale * scaleMultiplier;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
        isLevelingUp = false;
    }
    
    /// <summary>
    /// 更新UI（手动调用）
    /// </summary>
    public void UpdateUI()
    {
        if (LevelManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance.GetCurrentLevel();
            UpdateLevelText(currentLevel);
            
            if (debugMode) Debug.Log($"更新UI: 等级{currentLevel}");
        }
        
        if (ExperienceManager.Instance != null)
        {
            int currentExp = ExperienceManager.Instance.GetCurrentExperience();
            int expToNext = ExperienceManager.Instance.GetExpToNextLevel();
            OnExperienceChanged(currentExp, expToNext);
        }
    }
    
    /// <summary>
    /// 设置调试模式
    /// </summary>
    public void SetDebugMode(bool debug)
    {
        debugMode = debug;
    }
    
    /// <summary>
    /// 强制更新等级显示
    /// </summary>
    [ContextMenu("强制更新等级显示")]
    public void ForceUpdateLevelDisplay()
    {
        if (LevelManager.Instance != null)
        {
            int level = LevelManager.Instance.GetCurrentLevel();
            UpdateLevelText(level);
            Debug.Log($"强制更新等级显示: Lv.{level}");
        }
        else
        {
            Debug.LogError("无法强制更新：LevelManager 未找到");
        }
    }
    
    /// <summary>
    /// 测试等级更新
    /// </summary>
    [ContextMenu("测试等级更新")]
    public void TestLevelUpdate()
    {
        int testLevel = Random.Range(2, 20);
        UpdateLevelText(testLevel);
        Debug.Log($"测试等级更新: Lv.{testLevel}");
        
        StartCoroutine(LevelUpEffect());
    }
    
    private void OnDestroy()
    {
        // 清理事件监听
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.onExperienceChanged.RemoveListener(OnExperienceChanged);
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
            LevelManager.Instance.onLevelUpDetailed.RemoveListener(OnLevelUpDetailed);
        }
    }
}