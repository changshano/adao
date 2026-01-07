// LevelUpUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    [SerializeField] private TextMeshProUGUI defenseValueText;
    [SerializeField] private TextMeshProUGUI staminaValueText;
    
    [Header("动画设置")]
    [SerializeField] private float showDelay = 1f;
    [SerializeField] private float panelFadeTime = 0.5f;
    
    [Header("音效")]
    [SerializeField] private AudioClip levelUpSound;

    private int pendingLevel = 1;
    private int pendingSkillPoints = 0;
    private int pendingAttributePoints = 0;
    private bool hasPendingLevelUp = false;
    
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private bool isShowing = false;
    private int currentLevel = 1;
    private int skillPointsReward = 1;
    private int attributePointsReward = 3;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        // 获取或添加组件
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
        
        // 初始隐藏
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        
        if (attributeUpgradePanel != null)
        {
            attributeUpgradePanel.SetActive(false);
        }
        
        // 绑定按钮事件
        InitializeButtons();
    }
    
    private void InitializeButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        // 绑定属性升级按钮
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
        // 注册事件监听
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
    // 如果有正在显示的升级，或者有等待处理的升级，则累计奖励
    if (isShowing || hasPendingLevelUp)
    {
        pendingLevel = newLevel;
        pendingSkillPoints += skillPoints;
        pendingAttributePoints += attributePoints;
        hasPendingLevelUp = true;
        
        // 如果正在显示，更新当前显示的UI
        if (isShowing)
        {
            UpdateUITextWithPending();
        }
        return;
    }
    
    // 没有正在显示的升级，正常处理
    currentLevel = newLevel;
    skillPointsReward = skillPoints;
    attributePointsReward = attributePoints;
    
    StartCoroutine(ShowLevelUpCoroutine());
}
// 添加处理待定升级的方法
private void UpdateUITextWithPending()
{
    if (levelText != null)
    {
        levelText.text = $"LEVEL UP!\n<size=72>Lv. {pendingLevel}</size>";
    }
    
    if (rewardsText != null)
    {
        rewardsText.text = $"获得奖励:\n技能点: {pendingSkillPoints}\n属性点: {pendingAttributePoints}";
    }
}
    
    public void ShowLevelUpUI(int newLevel)
    {
        ShowLevelUpUI(newLevel, 1, 3);
    }
    
    public void ShowLevelUpUI(int newLevel, int skillPoints, int attributePoints)
    {
        ShowLevelUp(newLevel, skillPoints, attributePoints);
    }
    
    private IEnumerator ShowLevelUpCoroutine()
{
    isShowing = true;
    
    yield return new WaitForSeconds(showDelay);
    
    // 播放音效
    if (levelUpSound != null && audioSource != null)
    {
        audioSource.PlayOneShot(levelUpSound);
    }
    
    // 更新文本（使用当前值，可能已被待定更新）
    UpdateUIText();
    
    // 显示主面板
    levelUpPanel.SetActive(true);
    canvasGroup.alpha = 0f;
    
    // 淡入动画
    float elapsedTime = 0f;
    while (elapsedTime < panelFadeTime)
    {
        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeTime);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    canvasGroup.alpha = 1f;
    
    // 更新属性值显示
    UpdateAttributeValues();
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
                
                // 播放音效
                PlayUpgradeSound();
                
                // 更新显示
                UpdateAttributePointsDisplay(LevelManager.Instance.GetAvailableAttributePoints());
                
                // 如果属性点用完，自动关闭属性面板
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
        
        // 如果没有属性点，禁用升级按钮
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
            
            if (defenseValueText != null)
                defenseValueText.text = $"{LevelManager.Instance.GetAttribute("defense")}";
            
            if (staminaValueText != null)
                staminaValueText.text = $"{LevelManager.Instance.GetAttribute("stamina")}";
        }
    }
    
    /// <summary>
    /// 隐藏界面
    /// </summary>
    public void Hide()
{
    if (!isShowing) return;
    
    // 恢复游戏时间
    Time.timeScale = 1f;
    
    // 检查是否有待定升级
    if (hasPendingLevelUp)
    {
        // 处理待定升级
        currentLevel = pendingLevel;
        skillPointsReward = pendingSkillPoints;
        attributePointsReward = pendingAttributePoints;
        
        // 重置待定状态
        hasPendingLevelUp = false;
        pendingSkillPoints = 0;
        pendingAttributePoints = 0;
        
        // 立即显示下一次升级
        StartCoroutine(ShowLevelUpCoroutine());
        return;
    }
    
    // 没有待定升级，正常隐藏
    StartCoroutine(FadeOutAndHide());
}
    
    private IEnumerator FadeOutAndHide()
    {
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
    }
}