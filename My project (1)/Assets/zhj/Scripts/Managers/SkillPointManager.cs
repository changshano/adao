// ExperienceGrowthSystem/Managers/SkillPointManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class SkillPointManager : MonoBehaviour
{
    [Header("技能点分配")]
    [SerializeField] private int totalSkillPointsEarned = 0;
    [SerializeField] private int totalSkillPointsSpent = 0;
    
    [Header("事件")]
    public UnityEvent<string, int> onSkillPointAllocated; // 参数：技能ID，分配点数
    
    [System.Serializable]
    public class SkillAllocation
    {
        public string skillId;
        public int pointsAllocated;
        public int maxPoints = 5;
        public string skillName = "未命名技能";
        public string description = "技能描述";
    }
    
    [System.Serializable]
    public class SkillAllocationSaveData
    {
        public string skillId;
        public int pointsAllocated;
    }
    
    [System.Serializable]
    public class SkillPointSaveData
    {
        public int totalSkillPointsEarned;
        public int totalSkillPointsSpent;
        public SkillAllocationSaveData[] skillAllocations;
    }
    
    private Dictionary<string, SkillAllocation> skillAllocations = new Dictionary<string, SkillAllocation>();
    
    // 单例模式
    public static SkillPointManager Instance { get; private set; }
    
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
    }
    
    /// <summary>
    /// 分配技能点到指定技能
    /// </summary>
    public bool AllocateSkillPoint(string skillId, int points = 1, string skillName = null, string description = null)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager 未初始化");
            return false;
        }
        
        if (!LevelManager.Instance.UseSkillPoint(points))
        {
            Debug.LogWarning("没有足够的技能点");
            return false;
        }
        
        if (!skillAllocations.ContainsKey(skillId))
        {
            skillAllocations[skillId] = new SkillAllocation
            {
                skillId = skillId,
                pointsAllocated = 0,
                maxPoints = 5,
                skillName = skillName ?? skillId,
                description = description ?? "无描述"
            };
        }
        
        var allocation = skillAllocations[skillId];
        
        if (allocation.pointsAllocated + points > allocation.maxPoints)
        {
            Debug.LogWarning($"技能 {skillId} 已达到最大等级 ({allocation.maxPoints})");
            LevelManager.Instance.AddSkillPoints(points); // 返还技能点
            return false;
        }
        
        allocation.pointsAllocated += points;
        totalSkillPointsSpent += points;
        
        Debug.Log($"分配 {points} 技能点到 {skillId} ({allocation.skillName})，当前等级: {allocation.pointsAllocated}/{allocation.maxPoints}");
        
        // 触发事件
        onSkillPointAllocated?.Invoke(skillId, allocation.pointsAllocated);
        
        return true;
    }
    
    /// <summary>
    /// 重置技能点分配
    /// </summary>
    public void ResetSkillAllocations(bool refundPoints = true)
    {
        if (refundPoints && LevelManager.Instance != null)
        {
            LevelManager.Instance.AddSkillPoints(totalSkillPointsSpent);
        }
        
        totalSkillPointsSpent = 0;
        skillAllocations.Clear();
        
        Debug.Log("技能点分配已重置");
    }
    
    /// <summary>
    /// 重置单个技能分配
    /// </summary>
    public bool ResetSkillAllocation(string skillId, bool refundPoints = true)
    {
        if (skillAllocations.TryGetValue(skillId, out var allocation))
        {
            if (refundPoints && LevelManager.Instance != null)
            {
                LevelManager.Instance.AddSkillPoints(allocation.pointsAllocated);
            }
            
            totalSkillPointsSpent -= allocation.pointsAllocated;
            skillAllocations.Remove(skillId);
            
            Debug.Log($"已重置技能 {skillId} 的分配");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取技能分配信息
    /// </summary>
    public SkillAllocation GetSkillAllocation(string skillId)
    {
        return skillAllocations.ContainsKey(skillId) ? skillAllocations[skillId] : null;
    }
    
    /// <summary>
    /// 获取技能当前等级
    /// </summary>
    public int GetSkillLevel(string skillId)
    {
        var allocation = GetSkillAllocation(skillId);
        return allocation?.pointsAllocated ?? 0;
    }
    
    /// <summary>
    /// 获取所有技能分配
    /// </summary>
    public Dictionary<string, SkillAllocation> GetAllAllocations()
    {
        return new Dictionary<string, SkillAllocation>(skillAllocations);
    }
    
    /// <summary>
    /// 获取已分配的总技能点数
    /// </summary>
    public int GetTotalSpentPoints() => totalSkillPointsSpent;
    
    /// <summary>
    /// 获取可用技能点
    /// </summary>
    public int GetAvailableSkillPoints()
    {
        return LevelManager.Instance?.GetAvailableSkillPoints() ?? 0;
    }
    
    /// <summary>
    /// 是否可以升级技能
    /// </summary>
    public bool CanUpgradeSkill(string skillId, int points = 1)
    {
        if (LevelManager.Instance == null) return false;
        
        var allocation = GetSkillAllocation(skillId);
        int availablePoints = LevelManager.Instance.GetAvailableSkillPoints();
        
        if (availablePoints < points) return false;
        
        if (allocation != null)
        {
            return allocation.pointsAllocated + points <= allocation.maxPoints;
        }
        
        return points <= 5; // 新技能最多5点
    }
    
    /// <summary>
    /// 添加技能到可分配列表
    /// </summary>
    public void RegisterSkill(string skillId, string skillName, string description, int maxPoints = 5)
    {
        if (!skillAllocations.ContainsKey(skillId))
        {
            skillAllocations[skillId] = new SkillAllocation
            {
                skillId = skillId,
                pointsAllocated = 0,
                maxPoints = maxPoints,
                skillName = skillName,
                description = description
            };
        }
        else
        {
            // 更新现有技能信息
            var allocation = skillAllocations[skillId];
            allocation.skillName = skillName;
            allocation.description = description;
            allocation.maxPoints = maxPoints;
        }
    }
    
    /// <summary>
    /// 移除技能注册
    /// </summary>
    public void UnregisterSkill(string skillId)
    {
        if (skillAllocations.ContainsKey(skillId))
        {
            // 先重置分配
            ResetSkillAllocation(skillId, true);
        }
    }
    
    /// <summary>
    /// 保存数据
    /// </summary>
    public SkillPointSaveData Save()
    {
        var allocationsList = new List<SkillAllocationSaveData>();
        
        foreach (var kvp in skillAllocations)
        {
            allocationsList.Add(new SkillAllocationSaveData
            {
                skillId = kvp.Key,
                pointsAllocated = kvp.Value.pointsAllocated
            });
        }
        
        return new SkillPointSaveData
        {
            totalSkillPointsEarned = totalSkillPointsEarned,
            totalSkillPointsSpent = totalSkillPointsSpent,
            skillAllocations = allocationsList.ToArray()
        };
    }
    
    /// <summary>
    /// 加载数据
    /// </summary>
    public void Load(SkillPointSaveData data)
    {
        totalSkillPointsEarned = data.totalSkillPointsEarned;
        totalSkillPointsSpent = data.totalSkillPointsSpent;
        
        skillAllocations.Clear();
        
        if (data.skillAllocations != null)
        {
            foreach (var allocationData in data.skillAllocations)
            {
                RegisterSkill(allocationData.skillId, 
                            allocationData.skillId, 
                            "加载的技能", 
                            5);
                
                if (skillAllocations.ContainsKey(allocationData.skillId))
                {
                    skillAllocations[allocationData.skillId].pointsAllocated = allocationData.pointsAllocated;
                }
            }
        }
    }
    
    /// <summary>
    /// 获取技能点使用情况报告
    /// </summary>
    public string GetSkillPointsReport()
    {
        string report = $"技能点报告:\n";
        report += $"已获得总技能点: {totalSkillPointsEarned}\n";
        report += $"已使用技能点: {totalSkillPointsSpent}\n";
        report += $"可用技能点: {GetAvailableSkillPoints()}\n";
        report += $"已分配技能:\n";
        
        foreach (var kvp in skillAllocations)
        {
            var allocation = kvp.Value;
            report += $"  - {allocation.skillName}: {allocation.pointsAllocated}/{allocation.maxPoints}\n";
        }
        
        return report;
    }
}