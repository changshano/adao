// ExperienceGrowthSystem/Managers/ExperienceManager.cs
using UnityEngine;
using UnityEngine.Events;
using System;

public class ExperienceManager : MonoBehaviour
{
    [Header("配置引用")]
    [SerializeField] private ExperienceDataSO experienceData;
    [SerializeField] private GrowthConfigSO growthConfig;
    
    [Header("当前状态")]
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private float experienceMultiplier = 1f;
    
    [Header("事件")]
    public UnityEvent<int> onExperienceGained; // 参数：获得的经验值
    public UnityEvent<int, int> onExperienceChanged; // 参数：当前经验，升级所需经验
    public UnityEvent<string, int> onExperienceSource; // 参数：经验来源类型，获得经验
    
    // 单例模式
    public static ExperienceManager Instance { get; private set; }
    
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
}

/// <summary>
/// 自动加载所需资源
/// </summary>
private void LoadResources()
{
    // 尝试从Resources加载ExperienceData
    if (experienceData == null)
    {
        experienceData = Resources.Load<ExperienceDataSO>("LevelSystem/ExperienceData");
        if (experienceData == null)
        {
            Debug.LogWarning("从Resources加载ExperienceData失败，创建默认配置");
            CreateDefaultExperienceData();
        }
        else
        {
            Debug.Log("从Resources加载ExperienceData成功");
        }
    }
    
    // 尝试加载GrowthConfig
    if (growthConfig == null)
    {
        growthConfig = Resources.Load<GrowthConfigSO>("LevelSystem/GrowthConfig");
        if (growthConfig == null)
        {
            Debug.LogWarning("从Resources加载GrowthConfig失败");
        }
    }
}

/// <summary>
/// 创建默认的ExperienceData
/// </summary>
private void CreateDefaultExperienceData()
{
    experienceData = ScriptableObject.CreateInstance<ExperienceDataSO>();
    
    // 设置默认值
    experienceData.perMemberBonus = 0.1f;
    experienceData.maxPartySize = 4;
    
    Debug.Log("已创建默认ExperienceData配置");
}
    
    /// <summary>
    /// 初始化经验管理器
    /// </summary>
    public void Initialize(int startingExp = 0, int expToNextLevel = 100)
    {
        currentExperience = startingExp;
        experienceToNextLevel = expToNextLevel;
        
        onExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
    }
    
    /// <summary>
    /// 添加经验值
    /// </summary>
    public void AddExperience(int amount, string sourceType = "Unknown")
    {
        if (amount <= 0) return;
        
        // 应用经验倍率
        int actualAmount = Mathf.RoundToInt(amount * experienceMultiplier);
        currentExperience += actualAmount;
        
        Debug.Log($"获得经验: {actualAmount} (来源: {sourceType}), 当前经验: {currentExperience}");
        
        // 触发事件
        onExperienceGained?.Invoke(actualAmount);
        onExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        onExperienceSource?.Invoke(sourceType, actualAmount);
        
        // 检查是否升级
        CheckForLevelUp();
    }
    
    /// <summary>
    /// 从敌人或其他来源获得经验
    /// </summary>
    public void GainExperienceFromSource(string sourceType, int sourceLevel, int playerLevel, int partySize = 1)
    {
        if (experienceData == null)
        {
            Debug.LogError("ExperienceDataSO is not assigned!");
            return;
        }
        
        int experience = experienceData.CalculateActualExperience(sourceType, sourceLevel, playerLevel, partySize);
        AddExperience(experience, sourceType);
    }
    
    /// <summary>
    /// 设置经验倍率（用于难度、道具等）
    /// </summary>
    public void SetExperienceMultiplier(float multiplier, float duration = 0f)
    {
        experienceMultiplier = multiplier;
        
        if (duration > 0)
        {
            CancelInvoke(nameof(ResetExperienceMultiplier));
            Invoke(nameof(ResetExperienceMultiplier), duration);
        }
    }
    
    /// <summary>
    /// 重置经验倍率
    /// </summary>
    public void ResetExperienceMultiplier()
    {
        experienceMultiplier = 1f;
    }
    
    /// <summary>
    /// 检查是否达到升级条件
    /// </summary>
    private void CheckForLevelUp()
    {
        if (currentExperience >= experienceToNextLevel)
        {
            // 升级逻辑在LevelManager中处理
            LevelManager.Instance?.LevelUp();
            
            // 计算剩余经验
            int overflowExp = currentExperience - experienceToNextLevel;
            currentExperience = overflowExp;
            
            // 更新下一级所需经验
            experienceToNextLevel = LevelManager.Instance?.GetExpToNextLevel() ?? experienceToNextLevel * 2;
            
            onExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        }
    }
    
    /// <summary>
    /// 获取当前经验进度
    /// </summary>
    public float GetExperienceProgress()
    {
        if (experienceToNextLevel <= 0) return 0f;
        return Mathf.Clamp01((float)currentExperience / experienceToNextLevel);
    }
    
    /// <summary>
    /// 获取当前经验值
    /// </summary>
    public int GetCurrentExperience() => currentExperience;
    
    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetExpToNextLevel() => experienceToNextLevel;
    
    /// <summary>
    /// 保存数据
    /// </summary>
    public ExperienceSaveData Save()
    {
        return new ExperienceSaveData
        {
            currentExperience = currentExperience,
            experienceToNextLevel = experienceToNextLevel,
            experienceMultiplier = experienceMultiplier
        };
    }
    
    /// <summary>
    /// 加载数据
    /// </summary>
    public void Load(ExperienceSaveData data)
    {
        currentExperience = data.currentExperience;
        experienceToNextLevel = data.experienceToNextLevel;
        experienceMultiplier = data.experienceMultiplier;
        
        onExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
    }
}

[System.Serializable]
public class ExperienceSaveData
{
    public int currentExperience;
    public int experienceToNextLevel;
    public float experienceMultiplier = 1f;
}