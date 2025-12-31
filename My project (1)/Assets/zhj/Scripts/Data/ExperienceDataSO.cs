// ExperienceGrowthSystem/Data/ExperienceDataSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ExperienceData", menuName = "Game/Level System/Experience Data")]
public class ExperienceDataSO : ScriptableObject
{
    [System.Serializable]
    public class ExperienceSource
    {
        public string sourceType; // "Enemy", "Quest", "Exploration", "Boss"
        public int baseExperience;
        public float levelMultiplier = 0.2f; // 每高一级增加的额外经验百分比
    }
    
    [Header("经验来源配置")]
    public List<ExperienceSource> experienceSources = new List<ExperienceSource>
    {
        new ExperienceSource { sourceType = "Enemy", baseExperience = 50, levelMultiplier = 0.2f },
        new ExperienceSource { sourceType = "Boss", baseExperience = 500, levelMultiplier = 0.3f },
        new ExperienceSource { sourceType = "Quest", baseExperience = 200, levelMultiplier = 0.1f },
        new ExperienceSource { sourceType = "Exploration", baseExperience = 100, levelMultiplier = 0f }
    };
    
    [Header("组队经验加成")]
    [Range(0, 1)] public float perMemberBonus = 0.1f;
    public int maxPartySize = 4;
    
    [Header("等级差经验修正")]
    public AnimationCurve levelDifferenceCurve = AnimationCurve.Linear(0, 1, 10, 0.1f);
    
    /// <summary>
    /// 根据来源类型获取基础经验值
    /// </summary>
    public int GetBaseExperience(string sourceType)
    {
        var source = experienceSources.Find(s => s.sourceType == sourceType);
        return source?.baseExperience ?? 10;
    }
    
    /// <summary>
    /// 计算实际获得的经验值
    /// </summary>
    public int CalculateActualExperience(string sourceType, int sourceLevel, int playerLevel, int partySize = 1)
    {
        int baseExp = GetBaseExperience(sourceType);
        
        // 等级差修正
        int levelDiff = sourceLevel - playerLevel;
        float levelMultiplier = levelDifferenceCurve.Evaluate(Mathf.Abs(levelDiff));
        
        // 计算基础经验
        float experience = baseExp * (1 + (sourceLevel * 0.1f));
        
        // 应用等级差修正
        experience *= levelMultiplier;
        
        // 如果是Boss战，增加固定奖励
        if (sourceType == "Boss")
        {
            experience *= 1.5f;
        }
        
        // 组队经验分配
        if (partySize > 1)
        {
            float partyBonus = 1 + (Mathf.Min(partySize - 1, maxPartySize - 1) * perMemberBonus);
            experience /= partySize; // 平均分配
            experience *= partyBonus; // 组队加成
        }
        
        return Mathf.Max(1, Mathf.RoundToInt(experience));
    }
}