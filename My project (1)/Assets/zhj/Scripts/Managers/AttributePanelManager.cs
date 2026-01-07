// AttributePanelManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class AttributeData
{
    public string attributeName;
    public float currentValue;
    public float nextValue;
    public Color color = Color.white;
    public Sprite icon;
}

public class AttributePanelManager : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject attributePanel;
    [SerializeField] private Canvas attributeCanvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    
    [Header("面板标题")]
    [SerializeField] private TextMeshProUGUI panelTitleText;
    [SerializeField] private TextMeshProUGUI attributePointsText;
    
    [Header("属性行配置")]
    [SerializeField] private Transform attributesContainer;
    [SerializeField] private GameObject attributeRowPrefab;
    
    [Header("玩家引用")]
    [SerializeField] private PlayerAction playerAction;
    
    [Header("属性数据 - 只保留生命值和攻击力")]
    [SerializeField] private AttributeData healthData = new AttributeData
    {
        attributeName = "最大生命值",
        currentValue = 100f,
        nextValue = 105f,
        color = new Color(1f, 0.2f, 0.2f, 1f) // 红色
    };
    
    [SerializeField] private AttributeData attackData = new AttributeData
    {
        attributeName = "攻击力",
        currentValue = 10f,
        nextValue = 12f,
        color = new Color(1f, 0.5f, 0.2f, 1f) // 橙色
    };
    
    [Header("键盘控制")]
    [SerializeField] private KeyCode toggleKey = KeyCode.V;
    [SerializeField] private float toggleCooldown = 0.3f;
    
    [Header("动画设置")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.2f;
    [SerializeField] private float rowSpacing = 10f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("音效")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip notEnoughPointsSound;
    [SerializeField] private float soundVolume = 1f;
    
    [Header("视觉设置")]
    [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.95f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color upgradeButtonColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color disabledButtonColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
    
    [Header("层级设置")]
    [SerializeField] private int canvasSortOrder = 1000; // 非常高的层级
    [SerializeField] private string canvasSortingLayer = "UI";
    [SerializeField] private bool bringToFrontOnOpen = true;
    
    [Header("调试")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private int debugAttributePoints = 5;
    [SerializeField] private bool showDebugLogs = true;
    
    // 私有变量
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private Image panelBackground;
    private bool isPanelOpen = false;
    private float lastToggleTime = 0f;
    private bool isInitialized = false;
    
    // 属性行引用
    private List<AttributeRowUI> attributeRows = new List<AttributeRowUI>();
    private Dictionary<string, AttributeRowUI> attributeRowDict = new Dictionary<string, AttributeRowUI>();
    
    // 当前属性点
    private int currentAttributePoints = 0;
    
    [System.Serializable]
    public class AttributeRowUI
    {
        public GameObject rowObject;
        public TextMeshProUGUI attributeNameText;
        public TextMeshProUGUI currentValueText;
        public TextMeshProUGUI nextValueText;
        public TextMeshProUGUI upgradeCostText;
        public Button upgradeButton;
        public Image attributeIcon;
        public Image backgroundImage;
        public string attributeId;
        
        public void UpdateValues(float currentVal, float nextVal, int upgradeCost, bool canUpgrade)
        {
            if (currentValueText != null)
                currentValueText.text = currentVal.ToString("F1");
            
            if (nextValueText != null)
                nextValueText.text = $"{nextVal:F1}";
            
            if (upgradeCostText != null)
                upgradeCostText.text = upgradeCost > 0 ? $"{upgradeCost}点" : "MAX";
            
            if (upgradeButton != null)
            {
                upgradeButton.interactable = canUpgrade;
                
                // 更新按钮颜色
                Image buttonImage = upgradeButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = canUpgrade ? 
                        new Color(0.2f, 0.6f, 1f, 1f) : 
                        new Color(0.3f, 0.3f, 0.3f, 0.7f);
                }
                
                // 更新按钮文本
                TextMeshProUGUI buttonText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = canUpgrade ? "升级" : "不可用";
                    buttonText.color = canUpgrade ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                }
            }
            
            // 更新行背景
            if (backgroundImage != null)
            {
                backgroundImage.color = canUpgrade ? 
                    new Color(0.15f, 0.15f, 0.25f, 0.8f) : 
                    new Color(0.1f, 0.1f, 0.1f, 0.6f);
            }
        }
    }
    
    #region Unity生命周期
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        if (!isInitialized)
        {
            InitializePanel();
        }
        
        // 确保初始状态
        if (attributePanel != null)
        {
            attributePanel.SetActive(false);
        }
        
        // 查找PlayerAction引用
        if (playerAction == null)
        {
            playerAction = FindObjectOfType<PlayerAction>();
            if (playerAction != null && debugMode)
            {
                Debug.Log($"[AttributePanelManager] 找到PlayerAction: {playerAction.name}");
            }
        }
    }
    
    private void Update()
    {
        HandleKeyboardInput();
    }
    
    private void OnDestroy()
    {
        UnregisterEventListeners();
    }
    
    #endregion
    
    #region 初始化
    
    private void InitializeComponents()
    {
        if (isInitialized) return;
        
        // 获取或添加AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D音效
            audioSource.volume = soundVolume;
        }
        
        isInitialized = true;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 基础组件初始化完成");
        }
    }
    
    private void InitializePanel()
    {
        if (attributePanel == null)
        {
            Debug.LogError("[AttributePanelManager] 属性面板未设置！");
            return;
        }
        
        // 确保Canvas存在并正确设置
        EnsureCanvasSetup();
        
        // 获取或添加CanvasGroup
        canvasGroup = attributePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = attributePanel.AddComponent<CanvasGroup>();
        }
        
        // 获取或添加背景图片
        panelBackground = attributePanel.GetComponent<Image>();
        if (panelBackground == null)
        {
            panelBackground = attributePanel.AddComponent<Image>();
            panelBackground.color = panelBackgroundColor;
        }
        
        // 初始化属性行
        InitializeAttributeRows();
        
        // 设置初始属性点
        if (debugMode)
        {
            currentAttributePoints = debugAttributePoints;
        }
        else if (LevelManager.Instance != null)
        {
            currentAttributePoints = LevelManager.Instance.GetAvailableAttributePoints();
        }
        
        // 更新UI显示
        UpdateAllDisplays();
        
        // 注册事件监听
        RegisterEventListeners();
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 面板初始化完成");
            Debug.Log($"[AttributePanelManager] 当前属性点: {currentAttributePoints}");
        }
    }
    
    private void EnsureCanvasSetup()
    {
        if (attributePanel == null) return;
        
        // 查找Canvas
        attributeCanvas = attributePanel.GetComponent<Canvas>();
        if (attributeCanvas == null)
        {
            attributeCanvas = attributePanel.GetComponentInParent<Canvas>(true);
        }
        
        // 如果还没有找到Canvas，创建一个
        if (attributeCanvas == null)
        {
            CreateNewCanvas();
        }
        else
        {
            // 确保Canvas设置正确
            SetupCanvas(attributeCanvas);
        }
    }
    
    private void CreateNewCanvas()
    {
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 创建新Canvas...");
        }
        
        GameObject canvasObj = new GameObject("AttributeCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.SetAsLastSibling(); // 确保在层级最后
        
        // 添加Canvas组件
        attributeCanvas = canvasObj.AddComponent<Canvas>();
        canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        // 设置Canvas属性
        SetupCanvas(attributeCanvas);
        
        // 将属性面板移到Canvas下
        attributePanel.transform.SetParent(canvasObj.transform, false);
        
        // 设置RectTransform
        RectTransform rt = attributePanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 500); // 设置合适的大小
        }
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 已创建新Canvas");
        }
    }
    
    private void SetupCanvas(Canvas canvas)
    {
        if (canvas == null) return;
        
        // 设置渲染模式
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // 设置层级
        canvas.overrideSorting = true;
        canvas.sortingOrder = canvasSortOrder;
        canvas.sortingLayerName = canvasSortingLayer;
        
        // 设置CanvasScaler
        if (canvasScaler == null)
        {
            canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler == null) return;
        }
        
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] Canvas设置完成: Sort Order = {canvas.sortingOrder}, Sorting Layer = {canvas.sortingLayerName}");
        }
    }
    
    private void InitializeAttributeRows()
    {
        if (attributesContainer == null || attributeRowPrefab == null)
        {
            Debug.LogWarning("[AttributePanelManager] 属性容器或预制体未设置");
            return;
        }
        
        // 清除现有行
        foreach (Transform child in attributesContainer)
        {
            Destroy(child.gameObject);
        }
        attributeRows.Clear();
        attributeRowDict.Clear();
        
        // 只创建生命值和攻击力行
        CreateAttributeRow("health", healthData);
        CreateAttributeRow("attack", attackData);
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 初始化了 {attributeRows.Count} 个属性行");
        }
    }
    
    private void CreateAttributeRow(string attributeId, AttributeData data)
    {
        if (attributeRowPrefab == null || attributesContainer == null) return;
        
        // 实例化行
        GameObject rowObject = Instantiate(attributeRowPrefab, attributesContainer);
        rowObject.name = $"{attributeId}Row";
        
        // 获取组件引用
        AttributeRowUI rowUI = new AttributeRowUI
        {
            rowObject = rowObject,
            attributeId = attributeId
        };
        
        // 查找子组件
        TextMeshProUGUI[] texts = rowObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text.name.Contains("Name") || text.name.Contains("名称"))
                rowUI.attributeNameText = text;
            else if (text.name.Contains("Current") || text.name.Contains("当前"))
                rowUI.currentValueText = text;
            else if (text.name.Contains("Next") || text.name.Contains("下一级"))
                rowUI.nextValueText = text;
            else if (text.name.Contains("Cost") || text.name.Contains("消耗"))
                rowUI.upgradeCostText = text;
        }
        
        // 查找按钮
        rowUI.upgradeButton = rowObject.GetComponentInChildren<Button>();
        
        // 查找图标
        Image[] images = rowObject.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            if (image.name.Contains("Icon") || image.name.Contains("图标"))
                rowUI.attributeIcon = image;
            else if (image.gameObject == rowObject)
                rowUI.backgroundImage = image;
        }
        
        // 设置初始值
        if (rowUI.attributeNameText != null)
        {
            rowUI.attributeNameText.text = data.attributeName;
            rowUI.attributeNameText.color = data.color;
        }
        
        // 设置图标颜色
        if (rowUI.attributeIcon != null && data.icon != null)
        {
            rowUI.attributeIcon.sprite = data.icon;
            rowUI.attributeIcon.color = data.color;
        }
        
        // 设置按钮事件
        if (rowUI.upgradeButton != null)
        {
            string id = attributeId; // 局部变量用于闭包
            rowUI.upgradeButton.onClick.AddListener(() => OnUpgradeAttribute(id));
        }
        
        // 添加到列表
        attributeRows.Add(rowUI);
        attributeRowDict[attributeId] = rowUI;
        
        // 初始更新显示
        int upgradeCost = 1; // 默认升级消耗1点
        bool canUpgrade = currentAttributePoints >= upgradeCost;
        rowUI.UpdateValues(data.currentValue, data.nextValue, upgradeCost, canUpgrade);
    }
    
    #endregion
    
    #region 事件处理
    
    private void RegisterEventListeners()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onAttributePointsChanged.AddListener(OnAttributePointsChanged);
            LevelManager.Instance.onAttributeUpgraded.AddListener(OnAttributeUpgraded);
        }
    }
    
    private void UnregisterEventListeners()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.onAttributePointsChanged.RemoveListener(OnAttributePointsChanged);
            LevelManager.Instance.onAttributeUpgraded.RemoveListener(OnAttributeUpgraded);
        }
    }
    
    private void OnAttributePointsChanged(int points)
    {
        currentAttributePoints = points;
        UpdateAttributePointsDisplay();
        UpdateAllRowsInteractable();
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 属性点更新: {points}");
        }
    }
    
    private void OnAttributeUpgraded(string attribute, int amount)
{
    // 立即刷新属性数据
    RefreshAttributesFromPlayer();
    
    // 更新对应的属性行
    if (attributeRowDict.TryGetValue(attribute, out AttributeRowUI row))
    {
        int upgradeCost = 1;
        bool canUpgrade = currentAttributePoints >= upgradeCost;
        
        if (attribute == "health")
        {
            row.UpdateValues(healthData.currentValue, healthData.nextValue, upgradeCost, canUpgrade);
            
            // 更新PlayerAction的最大生命值
            if (playerAction != null)
            {
                playerAction.maxHealth = healthData.currentValue;
                playerAction.currentHealth = healthData.currentValue; // 同时恢复满血
                // playerAction.UpdateHealthBar();
                Debug.Log($"已更新PlayerAction最大生命值: {healthData.currentValue}");
            }
        }
        else if (attribute == "attack")
        {
            row.UpdateValues(attackData.currentValue, attackData.nextValue, upgradeCost, canUpgrade);
            
            // 注意：攻击力升级已经通过LevelManager处理了基础值
            // 装备加成保持不变，所以总攻击力会自动更新
            if (playerAction != null)
            {
                // Debug.Log($"攻击力升级完成 - 基础攻击力: {playerAction.GetBaseAttackDamage()}, 装备加成: {playerAction.GetEquipmentAttackBonus()}, 总攻击力: {playerAction.GetTotalAttackDamage()}");
            }
        }
    }
    
    PlaySound(upgradeSound);
    
    if (debugMode && showDebugLogs)
    {
        Debug.Log($"[AttributePanelManager] 属性升级完成: {attribute}, 增加值: {amount}");
    }
}

    
    #endregion
    
    #region UI控制
    
    private void UpdateAllDisplays()
    {
        UpdateAttributePointsDisplay();
        UpdateAllRowsDisplay();
    }
    
    private void UpdateAttributePointsDisplay()
    {
        if (attributePointsText != null)
        {
            attributePointsText.text = $"可用属性点: {currentAttributePoints}";
            attributePointsText.color = currentAttributePoints > 0 ? 
                new Color(0.2f, 0.8f, 0.2f, 1f) : // 绿色
                new Color(0.8f, 0.2f, 0.2f, 1f);  // 红色
        }
    }
    
    private void UpdateAllRowsDisplay()
    {
        foreach (var row in attributeRows)
        {
            int upgradeCost = 1;
            bool canUpgrade = currentAttributePoints >= upgradeCost;
            
            if (row.attributeId == "health")
            {
                row.UpdateValues(healthData.currentValue, healthData.nextValue, upgradeCost, canUpgrade);
            }
            else if (row.attributeId == "attack")
            {
                row.UpdateValues(attackData.currentValue, attackData.nextValue, upgradeCost, canUpgrade);
            }
        }
    }
    
    private void UpdateAllRowsInteractable()
    {
        foreach (var row in attributeRows)
        {
            bool canUpgrade = currentAttributePoints >= 1;
            if (row.upgradeButton != null)
            {
                row.upgradeButton.interactable = canUpgrade;
            }
        }
    }
    
    #endregion
    
    #region 键盘输入
    
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(toggleKey) && Time.time > lastToggleTime + toggleCooldown)
        {
            lastToggleTime = Time.time;
            TogglePanel();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isPanelOpen)
        {
            ClosePanel();
        }
    }
    
    #endregion
    
    #region 面板控制
    
    /// <summary>
    /// 切换属性面板
    /// </summary>
    public void TogglePanel()
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }
    
    /// <summary>
    /// 打开属性面板
    /// </summary>
    public void OpenPanel()
    {
        if (isPanelOpen) return;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 正在打开属性面板...");
        }
        
        // 每次打开面板时从PlayerAction重新读取属性
        RefreshAttributesFromPlayer();
        
        // 确保Canvas设置正确
        EnsureCanvasSetup();
        
        // 确保层级在最前
        if (bringToFrontOnOpen && attributeCanvas != null)
        {
            BringPanelToFront();
        }
        
        // 更新UI显示
        UpdateAllDisplays();
        
        // 显示面板
        if (attributePanel != null)
        {
            attributePanel.SetActive(true);
            StartCoroutine(FadeInPanel());
        }
        else
        {
            Debug.LogError("[AttributePanelManager] attributePanel 未设置");
            return;
        }
        
        // 暂停游戏
        Time.timeScale = 0f;
        
        isPanelOpen = true;
        PlaySound(openSound);
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 属性面板已打开");
        }
    }
    
    /// <summary>
    /// 关闭属性面板
    /// </summary>
    public void ClosePanel()
    {
        if (!isPanelOpen) return;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 正在关闭属性面板...");
        }
        
        StartCoroutine(FadeOutAndClosePanel());
        
        PlaySound(closeSound);
    }
    
    /// <summary>
    /// 将面板置于最前
    /// </summary>
    private void BringPanelToFront()
    {
        if (attributeCanvas != null)
        {
            // 设置高Sort Order
            attributeCanvas.sortingOrder = canvasSortOrder;
            attributeCanvas.sortingLayerName = canvasSortingLayer;
            
            // 将Canvas移到Hierarchy最后（在Unity中，后渲染的在上面）
            if (attributeCanvas.transform.parent != null)
            {
                attributeCanvas.transform.SetAsLastSibling();
            }
            
            if (debugMode && showDebugLogs)
            {
                Debug.Log($"[AttributePanelManager] 已将面板置于最前 (Sort Order: {attributeCanvas.sortingOrder}, Layer: {attributeCanvas.sortingLayerName})");
            }
        }
    }
    
    #endregion
    
    #region 动画控制
    
    private IEnumerator FadeInPanel()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime / fadeInTime;
            t = fadeCurve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    private IEnumerator FadeOutAndClosePanel()
    {
        if (canvasGroup == null)
        {
            attributePanel.SetActive(false);
            OnPanelClosed();
            yield break;
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;
            t = fadeCurve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        attributePanel.SetActive(false);
        
        OnPanelClosed();
    }
    
    private void OnPanelClosed()
    {
        // 恢复游戏
        Time.timeScale = 1f;
        
        isPanelOpen = false;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 属性面板已关闭");
        }
    }
    
    #endregion
    
    #region 属性升级
    
    private void OnUpgradeAttribute(string attributeId)
    {
        if (LevelManager.Instance != null)
        {
            // 通过LevelManager升级属性
            bool success = LevelManager.Instance.UpgradeAttribute(attributeId, 1);
            
            if (success)
            {
                if (debugMode && showDebugLogs)
                {
                    Debug.Log($"[AttributePanelManager] 升级属性: {attributeId}");
                }
            }
            else
            {
                PlaySound(notEnoughPointsSound);
                
                if (debugMode && showDebugLogs)
                {
                    Debug.Log($"[AttributePanelManager] 无法升级属性: {attributeId}");
                }
            }
        }
        else
        {
            // 调试模式下的模拟升级
            SimulateUpgrade(attributeId);
        }
    }
    
    private void SimulateUpgrade(string attributeId)
    {
        if (currentAttributePoints <= 0)
        {
            PlaySound(notEnoughPointsSound);
            return;
        }
        
        currentAttributePoints--;
        
        // 更新属性数据
        if (attributeId == "health")
        {
            healthData.currentValue = healthData.nextValue;
            healthData.nextValue += 5f;
            
            // 更新PlayerAction中的最大生命值
            if (playerAction != null)
            {
                playerAction.maxHealth = healthData.currentValue;
                // 同时更新当前生命值到新的最大值
                playerAction.currentHealth = healthData.currentValue;
                // playerAction.UpdateHealthBar();
            }
        }
        else if (attributeId == "attack")
        {
            attackData.currentValue = attackData.nextValue;
            attackData.nextValue += 2f;
            
            // 更新PlayerAction中的基础攻击力
            if (playerAction != null)
            {
                SetPlayerBaseAttackDamage(attackData.currentValue);
            }
        }
        
        // 更新UI
        UpdateAllDisplays();
        PlaySound(upgradeSound);
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 模拟升级: {attributeId}");
        }
    }
    
    #endregion
    
    #region PlayerAction数据交互

/// <summary>
/// 从PlayerAction中刷新属性数据
/// </summary>
private void RefreshAttributesFromPlayer()
{
    if (LevelManager.Instance != null)
    {
        // 从LevelManager获取基础属性值
        var healthValues = LevelManager.Instance.GetAttributeDisplayValues("health");
        healthData.currentValue = healthValues.current;
        healthData.nextValue = healthValues.next;
        
        // 攻击力需要读取总攻击力（基础+装备加成）
        var attackValues = LevelManager.Instance.GetAttributeDisplayValues("attack");
        float totalAttack = GetPlayerTotalAttack(); // 获取包含装备的总攻击力
        float baseAttack = attackValues.current; // 基础攻击力
        float equipmentBonus = totalAttack - baseAttack; // 装备加成
        
        attackData.currentValue = totalAttack; // 显示总攻击力
        attackData.nextValue = attackValues.next + equipmentBonus; // 下一级也要包含装备加成
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 从LevelManager刷新属性: 生命={healthData.currentValue}, 攻击={attackData.currentValue} (基础={baseAttack}, 装备加成={equipmentBonus})");
        }
    }
    else if (playerAction != null)
    {
        // 备用方案：从PlayerAction获取总攻击力
        playerAction = FindObjectOfType<PlayerAction>();
        if (playerAction == null) return;
        
        healthData.currentValue = playerAction.maxHealth;
        healthData.nextValue = healthData.currentValue + 5f;
        
        // 获取总攻击力（包含装备）
        float totalAttack = GetPlayerTotalAttack();
        attackData.currentValue = totalAttack;
        attackData.nextValue = totalAttack + 2f;
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 从PlayerAction刷新属性: 生命={healthData.currentValue}, 攻击={attackData.currentValue}");
        }
    }
    else
    {
        Debug.LogWarning("[AttributePanelManager] 无法刷新属性，LevelManager和PlayerAction都未找到");
    }
}
/// <summary>
/// 获取玩家总攻击力（基础攻击力 + 装备加成）
/// </summary>
private float GetPlayerTotalAttack()
{
    if (playerAction == null) 
    {
        playerAction = FindObjectOfType<PlayerAction>();
        if (playerAction == null) return attackData.currentValue;
    }
    
    try
    {
        // 直接使用PlayerAction的AttackDamage属性，它已经包含了装备加成
        return playerAction.AttackDamage;
    }
    catch (System.Exception e)
    {
        Debug.LogWarning($"获取玩家总攻击力时出错: {e.Message}");
        return attackData.currentValue;
    }
}
private void SetPlayerBaseAttackDamage(float newDamage)
{
    // 现在通过LevelManager统一管理，这个方法可以保留为空或删除
    if (debugMode)
    {
        Debug.Log($"[AttributePanelManager] 攻击力更新已通过LevelManager处理: {newDamage}");
    }
}
private float GetPlayerCurrentAttack()
{
    if (playerAction == null) return attackData.currentValue;
    
    try
    {
        // 尝试多种方式获取攻击力
        var method = typeof(PlayerAction).GetMethod("GetTotalAttackDamage");
        if (method != null)
        {
            return (float)method.Invoke(playerAction, null);
        }
        
        var property = typeof(PlayerAction).GetProperty("baseAttackDamage");
        if (property != null && property.CanRead)
        {
            return (float)property.GetValue(playerAction);
        }
        
        var field = typeof(PlayerAction).GetField("baseAttackDamage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (float)field.GetValue(playerAction);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogWarning($"获取PlayerAction攻击力时出错: {e.Message}");
    }
    
    return attackData.currentValue;
}


#endregion

#region 音效

private void PlaySound(AudioClip clip)
{
    if (clip != null && audioSource != null)
    {
        audioSource.PlayOneShot(clip, soundVolume);
    }
}

#endregion

#region 公共方法

/// <summary>
/// 添加属性点
/// </summary>
public void AddAttributePoints(int points)
{
    currentAttributePoints += points;
    UpdateAttributePointsDisplay();
    UpdateAllRowsInteractable();
    
    if (debugMode && showDebugLogs)
    {
        Debug.Log($"[AttributePanelManager] 添加属性点: {points}, 当前: {currentAttributePoints}");
    }
}

/// <summary>
/// 获取当前属性点
/// </summary>
public int GetAttributePoints()
{
    return currentAttributePoints;
}

/// <summary>
/// 设置PlayerAction引用
/// </summary>
public void SetPlayerAction(PlayerAction player)
{
    playerAction = player;
    if (playerAction != null && debugMode)
    {
        Debug.Log($"[AttributePanelManager] 已设置PlayerAction: {playerAction.name}");
    }
}

/// <summary>
/// 强制打开面板（用于测试）
/// </summary>
[ContextMenu("强制打开面板")]
public void ForceOpenPanel()
{
    OpenPanel();
}

/// <summary>
/// 强制关闭面板（用于测试）
/// </summary>
[ContextMenu("强制关闭面板")]
public void ForceClosePanel()
{
    ClosePanel();
}

/// <summary>
/// 重置面板位置和层级
/// </summary>
[ContextMenu("重置面板层级")]
public void ResetPanelHierarchy()
{
    if (attributeCanvas != null)
    {
        // 重置层级
        attributeCanvas.sortingOrder = canvasSortOrder;
        attributeCanvas.sortingLayerName = canvasSortingLayer;
        
        // 确保在Hierarchy最后
        attributeCanvas.transform.SetAsLastSibling();
        
        Debug.Log($"[AttributePanelManager] 已重置面板层级: Sort Order = {canvasSortOrder}");
    }
}

/// <summary>
/// 测试添加属性点
/// </summary>
[ContextMenu("测试: 添加5点属性点")]
public void TestAddPoints()
{
    AddAttributePoints(5);
}

/// <summary>
/// 测试升级生命值
/// </summary>
[ContextMenu("测试: 升级生命值")]
public void TestUpgradeHealth()
{
    OnUpgradeAttribute("health");
}

/// <summary>
/// 测试升级攻击力
/// </summary>
[ContextMenu("测试: 升级攻击力")]
public void TestUpgradeAttack()
{
    OnUpgradeAttribute("attack");
}

/// <summary>
/// 打印面板状态
/// </summary>
[ContextMenu("打印面板状态")]
public void PrintPanelStatus()
{
    Debug.Log($"=== 属性面板状态 ===");
    Debug.Log($"面板打开: {isPanelOpen}");
    Debug.Log($"当前属性点: {currentAttributePoints}");
    Debug.Log($"Canvas排序: {canvasSortOrder}");
    Debug.Log($"Canvas层级: {canvasSortingLayer}");
    
    if (attributeCanvas != null)
    {
        Debug.Log($"实际Canvas排序: {attributeCanvas.sortingOrder}");
        Debug.Log($"实际Canvas层级: {attributeCanvas.sortingLayerName}");
    }
    
    Debug.Log($"面板激活: {attributePanel != null && attributePanel.activeSelf}");
    Debug.Log($"CanvasGroup Alpha: {(canvasGroup != null ? canvasGroup.alpha : 0)}");
    
    if (playerAction != null)
    {
        Debug.Log($"PlayerAction引用: {playerAction.name}");
        Debug.Log($"玩家最大生命值: {playerAction.maxHealth}");
        Debug.Log($"玩家当前生命值: {playerAction.GetCurrentHealth()}");
        Debug.Log($"玩家基础攻击力: {playerAction.GetBaseAttackDamage()}");
        Debug.Log($"玩家总攻击力: {playerAction.GetTotalAttackDamage()}");
    }
}

/// <summary>
/// 检查所有UI引用
/// </summary>
[ContextMenu("检查UI引用")]
public void CheckUIRefs()
{
    Debug.Log($"=== UI引用检查 ===");
    Debug.Log($"属性面板: {attributePanel != null}");
    Debug.Log($"属性Canvas: {attributeCanvas != null}");
    Debug.Log($"属性点文本: {attributePointsText != null}");
    Debug.Log($"属性容器: {attributesContainer != null}");
    Debug.Log($"属性行预制体: {attributeRowPrefab != null}");
    Debug.Log($"CanvasGroup: {canvasGroup != null}");
    Debug.Log($"PlayerAction引用: {playerAction != null}");
    
    if (attributeCanvas != null)
    {
        Debug.Log($"Canvas Sort Order: {attributeCanvas.sortingOrder}");
        Debug.Log($"Canvas Sorting Layer: {attributeCanvas.sortingLayerName}");
    }
}

/// <summary>
/// 刷新属性显示（外部调用）
/// </summary>
public void RefreshAttributeDisplay()
{
    RefreshAttributesFromPlayer();
    UpdateAllDisplays();
    
    if (debugMode && showDebugLogs)
    {
        Debug.Log($"[AttributePanelManager] 已手动刷新属性显示");
    }
}
// 添加一个专门的方法用于装备更新后刷新面板
public void RefreshOnEquipmentChange()
{
    if (isPanelOpen)
    {
        RefreshAttributesFromPlayer();
        UpdateAllDisplays();
        
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[AttributePanelManager] 装备变更后刷新属性面板");
        }
    }
}
}


#endregion