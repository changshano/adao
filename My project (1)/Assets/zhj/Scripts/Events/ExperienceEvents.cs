// ExperienceGrowthSystem/Events/ExperienceEvents.cs
using UnityEngine.Events;
using System;

public static class ExperienceEvents
{
    // 经验值事件
    public static event Action<int> OnExperienceGained; // 参数：获得的经验值
    public static event Action<int, int> OnExperienceChanged; // 参数：当前经验，下一级所需经验
    public static event Action<string, int> OnExperienceSource; // 参数：来源类型，经验值
    
    // 等级事件
    public static event Action<int> OnLevelUp; // 参数：新等级
    public static event Action<int, int, int> OnLevelUpDetailed; // 参数：新等级，获得技能点，获得属性点
    
    // 属性事件
    public static event Action<string, int> OnAttributeUpgraded; // 参数：属性名，增加值
    public static event Action<int> OnSkillPointsChanged; // 参数：可用技能点
    public static event Action<int> OnAttributePointsChanged; // 参数：可用属性点
    
    /// <summary>
    /// 触发获得经验事件
    /// </summary>
    public static void TriggerExperienceGained(int amount)
    {
        OnExperienceGained?.Invoke(amount);
    }
    
    /// <summary>
    /// 触发经验变化事件
    /// </summary>
    public static void TriggerExperienceChanged(int currentExp, int expToNext)
    {
        OnExperienceChanged?.Invoke(currentExp, expToNext);
    }
    
    /// <summary>
    /// 触发经验来源事件
    /// </summary>
    public static void TriggerExperienceSource(string sourceType, int amount)
    {
        OnExperienceSource?.Invoke(sourceType, amount);
    }
    
    /// <summary>
    /// 触发升级事件
    /// </summary>
    public static void TriggerLevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
    }
    
    /// <summary>
    /// 触发详细升级事件
    /// </summary>
    public static void TriggerLevelUpDetailed(int newLevel, int skillPoints, int attributePoints)
    {
        OnLevelUpDetailed?.Invoke(newLevel, skillPoints, attributePoints);
    }
    
    /// <summary>
    /// 触发属性升级事件
    /// </summary>
    public static void TriggerAttributeUpgraded(string attribute, int amount)
    {
        OnAttributeUpgraded?.Invoke(attribute, amount);
    }
    
    /// <summary>
    /// 触发技能点变化事件
    /// </summary>
    public static void TriggerSkillPointsChanged(int points)
    {
        OnSkillPointsChanged?.Invoke(points);
    }
    
    /// <summary>
    /// 触发属性点变化事件
    /// </summary>
    public static void TriggerAttributePointsChanged(int points)
    {
        OnAttributePointsChanged?.Invoke(points);
    }
}