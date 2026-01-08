using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private Button closeButton;
    
    [Header("属性升级UI")]
    [SerializeField] private GameObject attributeUpgradePanel;
    [SerializeField] private Button healthButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private Button staminaButton;
    [SerializeField] private TextMeshProUGUI attributePointsText;
    [SerializeField] private TextMeshProUGUI healthValueText;
    [SerializeField] private TextMeshProUGUI attackValueText;
    [SerializeField] private TextMeshProUGUI staminaValueText;
    
    [Header("等级5特殊面板")]
    [SerializeField] private GameObject level5SpecialPanel;
    [SerializeField] private float specialPanelShowTime = 3f;
    [SerializeField] private AudioClip level5SpecialSound;
    
    [Header("动画设置")]
    [SerializeField] private float showDelay = 1f;
    [SerializeField] private float panelFadeTime = 0.5f;
    [SerializeField] private float autoHideDelay = 2f; 
    
    [Header("音效")]
    [SerializeField] private AudioClip levelUpSound;
    
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private bool isShowing = false;
    private int currentLevel = 1;
    private int skillPointsReward = 1;
    private int attributePointsReward = 3;
    private Coroutine autoHideCoroutine;
    
    // 队列相关变量
    private Queue<LevelUpData> levelUpQueue = new Queue<LevelUpData>();
    private bool isProcessingLevelUp = false;
    
    // 记录上次处理的等级，避免重复
    private int lastProcessedLevel = 0;
    
    // 新增：记录是否已经显示过等级5特殊面板
    private bool hasShownLevel5Special = false;
    
    // 升级数据结构
    private struct LevelUpData
    {
        public int level;
        public int skillPoints;
        public int attributePoints;
        
        public LevelUpData(int lv, int sp, int ap)
        {
            level = lv;
            skillPoints = sp;
            attributePoints = ap;
        }
    }
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        canvasGroup = levelUpPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = levelUpPanel.AddComponent<CanvasGroup>();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        
        if (attributeUpgradePanel != null)
        {
            attributeUpgradePanel.SetActive(false);
        }
        
        // 初始化等级5特殊面板
        if (level5SpecialPanel != null)
        {
            level5SpecialPanel.SetActive(false);
        }
        
        InitializeButtons();
    }
    
    private void InitializeButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        if (healthButton != null)
        {
            healthButton.onClick.AddListener(() => OnAttributeUpgrade("health"));
        }
        
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() => OnAttributeUpgrade("attack"));
        }
        
        if (defenseButton != null)
        {
            defenseButton.onClick.AddListener(() => OnAttributeUpgrade("defense"));
        }
        
        if (staminaButton != null)
        {
            staminaButton.onClick.AddListener(() => OnAttributeUpgrade("stamina"));
        }
    }
    
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUpDetailed.AddListener(ShowLevelUp);
            LevelManager.Instance.onAttributePointsChanged.AddListener(UpdateAttributePointsDisplay);
            LevelManager.Instance.onAttributeUpgraded.AddListener(UpdateAttributeValues);
            LevelManager.Instance.SetLevelUpUI(this);
        }
        
        UpdateAttributeValues();
    }
    
    /// <summary>
    /// 显示升级界面
    /// </summary>
    public void ShowLevelUp(int newLevel, int skillPoints, int attributePoints)
    {
        // 检查是否已经处理过这个等级
        if (newLevel <= lastProcessedLevel)
        {
            Debug.Log($"跳过已处理的等级: {newLevel}, 上次处理的等级: {lastProcessedLevel}");
            return;
        }
        
        // 创建升级数据
        LevelUpData levelUpData = new LevelUpData(newLevel, skillPoints, attributePoints);
        
        // 如果正在显示，加入队列
        if (isShowing || isProcessingLevelUp)
        {
            // 检查队列中是否已有相同或更高等级的数据
            bool shouldEnqueue = true;
            foreach (var data in levelUpQueue)
            {
                if (data.level >= newLevel)
                {
                    shouldEnqueue = false;
                    Debug.Log($"跳过加入队列，已有更高或相同等级: {data.level}");
                    break;
                }
            }
            
            if (shouldEnqueue)
            {
                levelUpQueue.Enqueue(levelUpData);
                Debug.Log($"升级事件加入队列: 等级 {newLevel}, 队列长度: {levelUpQueue.Count}");
            }
            return;
        }
        
        // 直接处理升级
        ProcessLevelUp(levelUpData);
    }
    
    public void ShowLevelUpUI(int newLevel)
    {
        ShowLevelUpUI(newLevel, 1, 3);
    }
    
    public void ShowLevelUpUI(int newLevel, int skillPoints, int attributePoints)
    {
        ShowLevelUp(newLevel, skillPoints, attributePoints);
    }
    
    /// <summary>
    /// 处理升级数据
    /// </summary>
    private void ProcessLevelUp(LevelUpData data)
    {
        // 更新最后处理的等级
        lastProcessedLevel = data.level;
        
        currentLevel = data.level;
        skillPointsReward = data.skillPoints;
        attributePointsReward = data.attributePoints;
        
        isProcessingLevelUp = true;
        StartCoroutine(ShowLevelUpCoroutine());
    }
    
    private IEnumerator ShowLevelUpCoroutine()
    {
        isShowing = true;
        
        yield return new WaitForSeconds(showDelay);
        
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        
        UpdateUIText();
        
        levelUpPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < panelFadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        UpdateAttributeValues();
        
        isProcessingLevelUp = false;
        
        Debug.Log($"显示升级界面: 等级 {currentLevel}");
        
        
        
        // 检查是否需要显示等级5特殊面板
        if (currentLevel >= 5 && !hasShownLevel5Special && level5SpecialPanel != null)
        {
            // 稍等一下再显示特殊面板
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ShowLevel5SpecialPanel());
        }
        // 启动自动隐藏
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
    }
    
    /// <summary>
    /// 显示等级5特殊面板
    /// </summary>
    private IEnumerator ShowLevel5SpecialPanel()
    {
        Debug.Log("显示等级5特殊面板");
        
        // 设置标志，避免重复显示
        hasShownLevel5Special = true;
        
        // 播放特殊音效
        if (level5SpecialSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(level5SpecialSound);
        }
        
        // 显示特殊面板
        level5SpecialPanel.SetActive(true);
        
        // 等待特殊面板显示时间
        yield return new WaitForSeconds(specialPanelShowTime);
        
        // 隐藏特殊面板
        level5SpecialPanel.SetActive(false);
        
        Debug.Log("等级5特殊面板已关闭");
    }
    
    /// <summary>
    /// 自动隐藏协程
    /// </summary>
    private IEnumerator AutoHideCoroutine()
    {
        // 等待指定时间后自动隐藏
        yield return new WaitForSeconds(autoHideDelay);
        
        // 自动隐藏
        if (isShowing)
        {
            Hide();
        }
    }
    
    private void UpdateUIText()
    {
        if (levelText != null)
        {
            levelText.text = $"LEVEL UP!\n<size=72>Lv. {currentLevel}</size>";
        }
        
        if (rewardsText != null)
        {
            rewardsText.text = $"获得奖励:\n技能点: {skillPointsReward}\n属性点: {attributePointsReward}";
        }
    }
    
    /// <summary>
    /// 属性升级按钮点击事件
    /// </summary>
    public void OnAttributeUpgrade(string attribute)
    {
        if (LevelManager.Instance != null)
        {
            bool success = LevelManager.Instance.UpgradeAttribute(attribute, 1);
            if (success)
            {
                Debug.Log($"升级 {attribute} 属性");
                
                PlayUpgradeSound();
                
                UpdateAttributePointsDisplay(LevelManager.Instance.GetAvailableAttributePoints());
                
                if (LevelManager.Instance.GetAvailableAttributePoints() <= 0 && attributeUpgradePanel != null)
                {
                    StartCoroutine(HideAttributePanelWithDelay());
                }
            }
        }
    }
    
    private void PlayUpgradeSound()
    {
        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();
        }
    }
    
    private IEnumerator HideAttributePanelWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        attributeUpgradePanel.SetActive(false);
    }
    
    /// <summary>
    /// 更新属性点显示
    /// </summary>
    private void UpdateAttributePointsDisplay(int points)
    {
        if (attributePointsText != null)
        {
            attributePointsText.text = $"可用属性点: {points}";
        }
        
        bool hasPoints = points > 0;
        if (healthButton != null) healthButton.interactable = hasPoints;
        if (attackButton != null) attackButton.interactable = hasPoints;
        if (defenseButton != null) defenseButton.interactable = hasPoints;
        if (staminaButton != null) staminaButton.interactable = hasPoints;
    }
    
    /// <summary>
    /// 更新属性值显示
    /// </summary>
    private void UpdateAttributeValues()
    {
        UpdateAttributeValues("", 0);
    }
    
    private void UpdateAttributeValues(string attribute, int amount)
    {
        if (LevelManager.Instance != null)
        {
            if (healthValueText != null)
                healthValueText.text = $"{LevelManager.Instance.GetAttribute("health")}";
            
            if (attackValueText != null)
                attackValueText.text = $"{LevelManager.Instance.GetAttribute("attack")}";
            
            
        }
    }
    
    /// <summary>
    /// 隐藏界面
    /// </summary>
    public void Hide()
    {
        if (!isShowing) return;
        
        Time.timeScale = 1f;
        
        // 检查队列中是否有更高等级的升级
        if (levelUpQueue.Count > 0)
        {
            // 找到队列中最高等级的升级
            LevelUpData nextLevelUp = FindHighestLevelInQueue();
            levelUpQueue.Clear(); // 清空队列，只处理最高等级
            
            Debug.Log($"处理队列中的最高等级升级: 等级 {nextLevelUp.level}");
            StartCoroutine(SwitchToNextLevelUp(nextLevelUp));
        }
        else
        {
            StartCoroutine(FadeOutAndHide());
        }
    }
    
    /// <summary>
    /// 查找队列中的最高等级
    /// </summary>
    private LevelUpData FindHighestLevelInQueue()
    {
        LevelUpData highest = new LevelUpData(0, 0, 0);
        foreach (var data in levelUpQueue)
        {
            if (data.level > highest.level)
            {
                highest = data;
            }
        }
        return highest;
    }
    
    /// <summary>
    /// 切换到下一个升级界面
    /// </summary>
    private IEnumerator SwitchToNextLevelUp(LevelUpData nextData)
    {
        // 快速淡出
        float switchFadeTime = 0.2f;
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < switchFadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / switchFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // 更新为下一个升级的数据
        currentLevel = nextData.level;
        skillPointsReward = nextData.skillPoints;
        attributePointsReward = nextData.attributePoints;
        
        // 更新最后处理的等级
        lastProcessedLevel = nextData.level;
        
        // 更新UI文本
        UpdateUIText();
        UpdateAttributeValues();
        
        // 淡入显示下一个升级
        elapsedTime = 0f;
        while (elapsedTime < switchFadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / switchFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // 播放音效
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        
        Debug.Log($"切换到升级界面: 等级 {currentLevel}");
        
        // 检查是否需要显示等级5特殊面板
        if (currentLevel >= 5 && !hasShownLevel5Special && level5SpecialPanel != null)
        {
            // 稍等一下再显示特殊面板
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ShowLevel5SpecialPanel());
        }
        
        // 启动自动隐藏
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
    }
    
    /// <summary>
    /// 完全淡出并隐藏界面
    /// </summary>
    private IEnumerator FadeOutAndHide()
    {
        // 确保特殊面板也关闭
        if (level5SpecialPanel != null && level5SpecialPanel.activeSelf)
        {
            level5SpecialPanel.SetActive(false);
        }
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < panelFadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / panelFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        levelUpPanel.SetActive(false);
        attributeUpgradePanel.SetActive(false);
        isShowing = false;
        
        Debug.Log("升级界面已完全关闭");
    }
    
    /// <summary>
    /// 清空升级队列
    /// </summary>
    public void ClearLevelUpQueue()
    {
        levelUpQueue.Clear();
        lastProcessedLevel = 0;
        Debug.Log("升级队列已清空");
    }
    
    /// <summary>
    /// 重置等级5特殊面板状态
    /// </summary>
    public void ResetLevel5SpecialPanel()
    {
        hasShownLevel5Special = false;
        Debug.Log("等级5特殊面板状态已重置");
    }
}