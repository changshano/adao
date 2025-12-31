// ExperienceGrowthSystem/Data/LevelDataSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level System/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public int level;
        public int experienceRequired;
        public float healthMultiplier = 1.2f;
        public float attackMultiplier = 1.1f;
        public float defenseMultiplier = 1.05f;
        public int skillPointsReward = 1;
        public int attributePointsReward = 3;
    }
    
    [Header("等级配置")]
    public int maxLevel = 100;
    public int baseExperience = 100;
    public float experienceMultiplier = 1.5f;
    
    [Header("详细等级信息")]
    public List<LevelInfo> levelInfoList = new List<LevelInfo>();
    
    [Header("属性增长范围")]
    [Range(0, 100)] public int minHealthGain = 10;
    [Range(0, 100)] public int maxHealthGain = 20;
    [Range(0, 50)] public int minAttackGain = 2;
    [Range(0, 50)] public int maxAttackGain = 5;
    [Range(0, 20)] public int minDefenseGain = 1;
    [Range(0, 20)] public int maxDefenseGain = 3;
    
    /// <summary>
    /// 计算升级所需经验值
    /// </summary>
    public int CalculateRequiredExp(int currentLevel)
    {
        if (currentLevel <= 0) return 0;
        if (currentLevel > maxLevel) return int.MaxValue;
        
        // 使用公式：基础经验 * (等级^经验系数)
        return Mathf.RoundToInt(baseExperience * Mathf.Pow(currentLevel, experienceMultiplier));
    }
    
    /// <summary>
    /// 获取等级信息
    /// </summary>
    public LevelInfo GetLevelInfo(int level)
    {
        if (levelInfoList.Count > level && level >= 0)
        {
            return levelInfoList[level];
        }
        
        // 如果列表中没有，创建一个
        var info = new LevelInfo
        {
            level = level,
            experienceRequired = CalculateRequiredExp(level)
        };
        
        if (levelInfoList.Count <= level)
        {
            levelInfoList.Add(info);
        }
        else
        {
            levelInfoList[level] = info;
        }
        
        return info;
    }
    
    /// <summary>
    /// 生成所有等级信息
    /// </summary>
    [ContextMenu("Generate Level Data")]
    public void GenerateLevelData()
    {
        levelInfoList.Clear();
        
        for (int i = 1; i <= maxLevel; i++)
        {
            LevelInfo info = new LevelInfo
            {
                level = i,
                experienceRequired = CalculateRequiredExp(i),
                healthMultiplier = 1.0f + (i * 0.02f),
                attackMultiplier = 1.0f + (i * 0.01f),
                defenseMultiplier = 1.0f + (i * 0.005f),
                skillPointsReward = i % 5 == 0 ? 2 : 1, // 每5级额外给1点技能点
                attributePointsReward = 3
            };
            levelInfoList.Add(info);
        }
    }
}