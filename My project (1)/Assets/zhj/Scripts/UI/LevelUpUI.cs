// ExperienceGrowthSystem/UI/LevelUpUI.cs
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
    
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private bool isShowing = false;
    private int currentLevel = 1;
    private int skillPointsReward = 1;
    private int attributePointsReward = 3;
    
    private void Awake()
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
        levelUpPanel.SetActive(false);
        attributeUpgradePanel.SetActive(false);
        
        // 绑定按钮事件
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
    /// 显示升级界面（三个参数版本）
    /// </summary>
    public void ShowLevelUp(int newLevel, int skillPoints, int attributePoints)
    {
        if (isShowing) return;
        
        currentLevel = newLevel;
        skillPointsReward = skillPoints;
        attributePointsReward = attributePoints;
        
        StartCoroutine(ShowLevelUpCoroutine());
    }
    
    /// <summary>
    /// 显示升级界面（一个参数版本 - 兼容旧代码）
    /// </summary>
    public void ShowLevelUpUI(int newLevel)
    {
        // 使用默认值
        ShowLevelUpUI(newLevel, 1, 3);
    }
    
    /// <summary>
    /// 显示升级界面（三个参数版本 - 兼容性方法）
    /// </summary>
    public void ShowLevelUpUI(int newLevel, int skillPoints, int attributePoints)
    {
        ShowLevelUp(newLevel, skillPoints, attributePoints);
    }
    
    private IEnumerator ShowLevelUpCoroutine()
    {
        isShowing = true;
        
        // 延迟显示
        yield return new WaitForSeconds(showDelay);
        
        // 播放音效
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        
        // 更新文本
        UpdateUIText();
        
        // 显示主面板
        ShowMainPanel();
        
        // 淡入动画
        yield return StartCoroutine(FadeInPanel());
        
        // 更新属性值显示
        UpdateAttributeValues();
        
        // 如果有属性点，显示属性升级面板
        if (attributePointsReward > 0 && attributeUpgradePanel != null)
        {
            yield return new WaitForSeconds(0.5f);
            attributeUpgradePanel.SetActive(true);
            UpdateAttributePointsDisplay(attributePointsReward);
        }
        
        // 暂停游戏
        Time.timeScale = 0f;
        
        Debug.Log($"升级界面显示完成: 等级{currentLevel}");
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
    
    private void ShowMainPanel()
    {
        levelUpPanel.SetActive(true);
        canvasGroup.alpha = 0f;
    }
    
    private IEnumerator FadeInPanel()
    {
        float elapsedTime = 0f;
        while (elapsedTime < panelFadeTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
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
        
        // 如果没有属性点，隐藏升级按钮
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
                healthValueText.text = $"生命: {LevelManager.Instance.GetAttribute("health")}";
            if (attackValueText != null)
                attackValueText.text = $"攻击: {LevelManager.Instance.GetAttribute("attack")}";
            if (defenseValueText != null)
                defenseValueText.text = $"防御: {LevelManager.Instance.GetAttribute("defense")}";
            if (staminaValueText != null)
                staminaValueText.text = $"耐力: {LevelManager.Instance.GetAttribute("stamina")}";
        }
    }
    
    /// <summary>
    /// 属性升级
    /// </summary>
    private void OnAttributeUpgrade(string attribute)
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
                if (LevelManager.Instance.GetAvailableAttributePoints() <= 0)
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
            // 可以添加升级音效
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
    /// 隐藏界面
    /// </summary>
    public void Hide()
    {
        if (!isShowing) return;
        
        // 恢复游戏时间
        Time.timeScale = 1f;
        
        // 淡出动画
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
        
        Debug.Log("升级界面已关闭");
    }
    
    /// <summary>
    /// 强制关闭界面（用于紧急情况）
    /// </summary>
    public void ForceHide()
    {
        Time.timeScale = 1f;
        levelUpPanel.SetActive(false);
        attributeUpgradePanel.SetActive(false);
        isShowing = false;
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// 测试显示升级界面
    /// </summary>
    [ContextMenu("测试升级界面")]
    public void TestShowLevelUp()
    {
        if (!isShowing)
        {
            ShowLevelUpUI(5, 2, 5);
        }
    }
    
    private void OnDestroy()
    {
        // 取消事件监听
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUpDetailed.RemoveListener(ShowLevelUp);
            LevelManager.Instance.onAttributePointsChanged.RemoveListener(UpdateAttributePointsDisplay);
            LevelManager.Instance.onAttributeUpgraded.RemoveListener(UpdateAttributeValues);
        }
    }
}