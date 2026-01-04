// ExperienceGrowthSystem/Managers/LevelManager.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("配置引用")]
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private GrowthConfigSO growthConfig;
    
    [Header("当前状态")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int availableSkillPoints = 0;
    [SerializeField] private int availableAttributePoints = 0;
    [SerializeField] private string playerClass = "Warrior";
    
    [Header("属性")]
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private int baseAttack = 10;
    [SerializeField] private int baseDefense = 5;
    [SerializeField] private int baseStamina = 50;
    
    [Header("当前加成后的属性")]
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private int currentAttack = 10;
    [SerializeField] private int currentDefense = 5;
    [SerializeField] private int currentStamina = 50;
    
    [Header("事件")]
    public UnityEvent<int> onLevelUp; // 参数：新等级
    public UnityEvent<int, int, int> onLevelUpDetailed; // 参数：新等级，技能点，属性点
    public UnityEvent<int> onSkillPointsChanged; // 参数：可用技能点
    public UnityEvent<int> onAttributePointsChanged; // 参数：可用属性点
    public UnityEvent<string, int> onAttributeUpgraded; // 参数：属性名，增加值
    
    // 单例模式
    public static LevelManager Instance { get; private set; }
    
    // 属性升级记录
    private Dictionary<string, int> attributeUpgrades = new Dictionary<string, int>();
    
    // UI引用
    private LevelUpUI levelUpUI;
    
    private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
    
    // 自动加载资源
    LoadResources();
    
    InitializeAttributes();
    
    // 尝试查找LevelUpUI
    FindLevelUpUI();
}

/// <summary>
/// 加载资源
/// </summary>
private void LoadResources()
{
    // 加载LevelData
    if (levelData == null)
    {
        levelData = Resources.Load<LevelDataSO>("LevelSystem/LevelData");
        if (levelData == null)
        {
            Debug.LogError("无法从Resources/LevelSystem/加载LevelData.asset");
            CreateDefaultLevelData();
        }
        else
        {
            Debug.Log("已加载LevelData");
        }
    }
    
    // 加载GrowthConfig
    if (growthConfig == null)
    {
        growthConfig = Resources.Load<GrowthConfigSO>("LevelSystem/GrowthConfig");
        if (growthConfig == null)
        {
            Debug.LogWarning("无法加载GrowthConfig，使用默认值");
        }
    }
}

/// <summary>
/// 创建默认的LevelData
/// </summary>
private void CreateDefaultLevelData()
{
    levelData = ScriptableObject.CreateInstance<LevelDataSO>();
    
    // 设置默认值
    levelData.maxLevel = 100;
    levelData.baseExperience = 100;
    levelData.experienceMultiplier = 1.5f;
    
    // 生成等级数据
    var method = levelData.GetType().GetMethod("GenerateLevelData");
    if (method != null)
    {
        method.Invoke(levelData, null);
    }
    
    Debug.Log("已创建默认LevelData配置");
}
    
    /// <summary>
    /// 查找LevelUpUI
    /// </summary>
    private void FindLevelUpUI()
    {
        levelUpUI = FindObjectOfType<LevelUpUI>(true);
        if (levelUpUI == null)
        {
            Debug.Log("未找到LevelUpUI，升级时将只显示日志");
        }
    }
    
    /// <summary>
    /// 设置LevelUpUI引用
    /// </summary>
    public void SetLevelUpUI(LevelUpUI ui)
    {
        levelUpUI = ui;
    }
    
    /// <summary>
    /// 初始化属性
    /// </summary>
    private void InitializeAttributes()
    {
        if (growthConfig == null)
        {
            Debug.LogWarning("GrowthConfig 未设置，使用默认属性");
            return;
        }
        
        var classGrowth = growthConfig.GetClassGrowth(playerClass);
        if (classGrowth != null)
        {
            baseHealth = classGrowth.baseHealth;
            baseAttack = classGrowth.baseAttack;
            baseDefense = classGrowth.baseDefense;
            baseStamina = classGrowth.baseStamina;
        }
        
        UpdateCurrentAttributes();
    }
    
    /// <summary>
    /// 升级
    /// </summary>
    public void LevelUp()
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData 未设置");
            return;
        }
        
        if (currentLevel >= levelData.maxLevel)
        {
            Debug.Log("已达到最大等级");
            return;
        }
        
        int previousLevel = currentLevel;
        currentLevel++;
        
        // 获取等级信息
        var levelInfo = levelData.GetLevelInfo(currentLevel);
        
        // 奖励技能点和属性点
        int skillPointsReward = levelInfo.skillPointsReward;
        int attributePointsReward = levelInfo.attributePointsReward;
        
        availableSkillPoints += skillPointsReward;
        availableAttributePoints += attributePointsReward;
        
        // 自动属性增长
        AutoGrowAttributes();
        
        // 触发事件
        onLevelUp?.Invoke(currentLevel);
        onLevelUpDetailed?.Invoke(currentLevel, skillPointsReward, attributePointsReward);
        onSkillPointsChanged?.Invoke(availableSkillPoints);
        onAttributePointsChanged?.Invoke(availableAttributePoints);
        
        Debug.Log($"升级到 {currentLevel} 级！获得 {skillPointsReward} 技能点，{attributePointsReward} 属性点");
        
        // 显示升级UI
        ShowLevelUpNotification(currentLevel, skillPointsReward, attributePointsReward);
    }
    
    /// <summary>
    /// 显示升级通知
    /// </summary>
    private void ShowLevelUpNotification(int newLevel, int skillPoints, int attributePoints)
    {
        if (levelUpUI != null)
        {
            levelUpUI.ShowLevelUpUI(newLevel, skillPoints, attributePoints);
        }
        else
        {
            // 如果没有UI，至少显示日志
            Debug.Log($"<color=yellow>等级提升！</color> 当前等级: {newLevel}");
            Debug.Log($"<color=yellow>获得奖励：</color> 技能点: {skillPoints}, 属性点: {attributePoints}");
        }
    }
    
    /// <summary>
    /// 自动增长属性
    /// </summary>
    private void AutoGrowAttributes()
    {
        if (levelData == null) return;
        
        // 随机增长属性
        int healthGain = Random.Range(levelData.minHealthGain, levelData.maxHealthGain + 1);
        int attackGain = Random.Range(levelData.minAttackGain, levelData.maxAttackGain + 1);
        int defenseGain = Random.Range(levelData.minDefenseGain, levelData.maxDefenseGain + 1);
        
        baseHealth += healthGain;
        baseAttack += attackGain;
        baseDefense += defenseGain;
        
        UpdateCurrentAttributes();
        
        Debug.Log($"自动属性增长: 生命+{healthGain}, 攻击+{attackGain}, 防御+{defenseGain}");
    }
    
    /// <summary>
    /// 手动升级属性
    /// </summary>
    public bool UpgradeAttribute(string attributeName, int amount = 1)
    {
        if (availableAttributePoints < amount) 
        {
            Debug.LogWarning($"属性点不足，需要{amount}点，当前{availableAttributePoints}点");
            return false;
        }
        
        int increaseAmount = 0;
        
        switch (attributeName.ToLower())
        {
            case "health":
                increaseAmount = amount * 5;
                baseHealth += increaseAmount;
                currentHealth += increaseAmount;
                break;
            case "attack":
                increaseAmount = amount;
                baseAttack += increaseAmount;
                currentAttack += increaseAmount;
                break;
            case "defense":
                increaseAmount = amount;
                baseDefense += increaseAmount;
                currentDefense += increaseAmount;
                break;
            case "stamina":
                increaseAmount = amount * 3;
                baseStamina += increaseAmount;
                currentStamina += increaseAmount;
                break;
            default:
                Debug.LogWarning($"未知属性: {attributeName}");
                return false;
        }
        
        // 记录升级
        if (!attributeUpgrades.ContainsKey(attributeName))
            attributeUpgrades[attributeName] = 0;
        attributeUpgrades[attributeName] += amount;
        
        availableAttributePoints -= amount;
        
        // 触发事件
        onAttributeUpgraded?.Invoke(attributeName, increaseAmount);
        onAttributePointsChanged?.Invoke(availableAttributePoints);
        
        Debug.Log($"属性升级: {attributeName} +{increaseAmount}");
        
        return true;
    }
    
    /// <summary>
    /// 使用技能点
    /// </summary>
    public bool UseSkillPoint(int amount = 1)
    {
        if (availableSkillPoints < amount) 
        {
            Debug.LogWarning($"技能点不足，需要{amount}点，当前{availableSkillPoints}点");
            return false;
        }
        
        availableSkillPoints -= amount;
        onSkillPointsChanged?.Invoke(availableSkillPoints);
        
        Debug.Log($"使用技能点: {amount}，剩余: {availableSkillPoints}");
        
        return true;
    }
    
    /// <summary>
    /// 添加技能点
    /// </summary>
    public void AddSkillPoints(int amount)
    {
        if (amount <= 0) return;
        
        availableSkillPoints += amount;
        onSkillPointsChanged?.Invoke(availableSkillPoints);
        
        Debug.Log($"获得技能点: {amount}，当前: {availableSkillPoints}");
    }
    
    /// <summary>
    /// 添加属性点
    /// </summary>
    public void AddAttributePoints(int amount)
    {
        if (amount <= 0) return;
        
        availableAttributePoints += amount;
        onAttributePointsChanged?.Invoke(availableAttributePoints);
        
        Debug.Log($"获得属性点: {amount}，当前: {availableAttributePoints}");
    }
    
    /// <summary>
    /// 更新当前属性（考虑装备、buff等加成）
    /// </summary>
    public void UpdateCurrentAttributes()
    {
        // 这里可以添加装备、buff等加成计算
        currentHealth = baseHealth;
        currentAttack = baseAttack;
        currentDefense = baseDefense;
        currentStamina = baseStamina;
    }
    
    /// <summary>
    /// 获取下一级所需经验
    /// </summary>
    public int GetExpToNextLevel()
    {
        if (levelData == null) 
        {
            Debug.LogWarning("LevelData 未设置，使用默认经验值");
            return 100;
        }
        
        if (currentLevel < levelData.maxLevel)
        {
            return levelData.GetLevelInfo(currentLevel + 1).experienceRequired;
        }
        return int.MaxValue;
    }
    
    /// <summary>
    /// 获取当前等级
    /// </summary>
    public int GetCurrentLevel() => currentLevel;
    
    /// <summary>
    /// 获取可用技能点
    /// </summary>
    public int GetAvailableSkillPoints() => availableSkillPoints;
    
    /// <summary>
    /// 获取可用属性点
    /// </summary>
    public int GetAvailableAttributePoints() => availableAttributePoints;
    
    /// <summary>
    /// 获取属性
    /// </summary>
    public int GetAttribute(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "health" or "hp" => currentHealth,
            "attack" or "atk" => currentAttack,
            "defense" or "def" => currentDefense,
            "stamina" or "sta" => currentStamina,
            _ => 0
        };
    }
    
    /// <summary>
    /// 获取所有当前属性
    /// </summary>
    public Dictionary<string, int> GetAllAttributes()
    {
        return new Dictionary<string, int>
        {
            { "Health", currentHealth },
            { "Attack", currentAttack },
            { "Defense", currentDefense },
            { "Stamina", currentStamina }
        };
    }
    
    /// <summary>
    /// 获取基础属性
    /// </summary>
    public Dictionary<string, int> GetBaseAttributes()
    {
        return new Dictionary<string, int>
        {
            { "Health", baseHealth },
            { "Attack", baseAttack },
            { "Defense", baseDefense },
            { "Stamina", baseStamina }
        };
    }
    
    /// <summary>
    /// 设置玩家职业
    /// </summary>
    public void SetPlayerClass(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogWarning("职业名不能为空");
            return;
        }
        
        playerClass = className;
        InitializeAttributes();
        
        Debug.Log($"职业已设置为: {className}");
    }
    
    /// <summary>
    /// 获取玩家职业
    /// </summary>
    public string GetPlayerClass() => playerClass;
    
    /// <summary>
    /// 设置等级
    /// </summary>
    public void SetLevel(int level, bool updateAttributes = true)
    {
        if (level < 1) level = 1;
        if (levelData != null && level > levelData.maxLevel) 
            level = levelData.maxLevel;
            
        currentLevel = level;
        
        if (updateAttributes)
        {
            InitializeAttributes();
        }
    }
    
    /// <summary>
    /// 添加经验并检查升级
    /// </summary>
    public void AddExperience(int experience)
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.AddExperience(experience, "LevelManager");
        }
    }
    
    /// <summary>
    /// 重置所有属性
    /// </summary>
    [ContextMenu("重置属性")]
    public void ResetAttributes()
    {
        availableSkillPoints = 0;
        availableAttributePoints = 0;
        attributeUpgrades.Clear();
        
        InitializeAttributes();
        
        onSkillPointsChanged?.Invoke(availableSkillPoints);
        onAttributePointsChanged?.Invoke(availableAttributePoints);
        
        Debug.Log("属性已重置");
    }
    
    /// <summary>
    /// 获取属性升级次数
    /// </summary>
    public int GetAttributeUpgradeCount(string attributeName)
    {
        if (attributeUpgrades.TryGetValue(attributeName, out int count))
        {
            return count;
        }
        return 0;
    }
    
    /// <summary>
    /// 获取总属性升级次数
    /// </summary>
    public int GetTotalAttributeUpgrades()
    {
        int total = 0;
        foreach (var kvp in attributeUpgrades)
        {
            total += kvp.Value;
        }
        return total;
    }
    
    /// <summary>
    /// 保存数据
    /// </summary>
    public LevelSaveData Save()
    {
        return new LevelSaveData
        {
            currentLevel = currentLevel,
            availableSkillPoints = availableSkillPoints,
            availableAttributePoints = availableAttributePoints,
            playerClass = playerClass,
            baseHealth = baseHealth,
            baseAttack = baseAttack,
            baseDefense = baseDefense,
            baseStamina = baseStamina
        };
    }
    
    /// <summary>
    /// 加载数据
    /// </summary>
    public void Load(LevelSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("加载的数据为空");
            return;
        }
        
        currentLevel = Mathf.Max(1, data.currentLevel);
        availableSkillPoints = Mathf.Max(0, data.availableSkillPoints);
        availableAttributePoints = Mathf.Max(0, data.availableAttributePoints);
        playerClass = string.IsNullOrEmpty(data.playerClass) ? "Warrior" : data.playerClass;
        baseHealth = Mathf.Max(1, data.baseHealth);
        baseAttack = Mathf.Max(1, data.baseAttack);
        baseDefense = Mathf.Max(0, data.baseDefense);
        baseStamina = Mathf.Max(1, data.baseStamina);
        
        UpdateCurrentAttributes();
        
        onSkillPointsChanged?.Invoke(availableSkillPoints);
        onAttributePointsChanged?.Invoke(availableAttributePoints);
        
        Debug.Log($"等级数据已加载: 等级{currentLevel}, 职业{playerClass}");
    }
}

[System.Serializable]
public class LevelSaveData
{
    public int currentLevel = 1;
    public int availableSkillPoints = 0;
    public int availableAttributePoints = 0;
    public string playerClass = "Warrior";
    public int baseHealth = 100;
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int baseStamina = 50;
}