// ExperienceGrowthSystem/Interfaces/IExperienceSource.cs
public interface IExperienceSource
{
    /// <summary>
    /// 获取经验值
    /// </summary>
    int GetExperienceValue();
    
    /// <summary>
    /// 获取经验来源类型
    /// </summary>
    string GetExperienceSourceType();
    
    /// <summary>
    /// 获取源等级
    /// </summary>
    int GetSourceLevel();
    
    /// <summary>
    /// 获取队伍大小（用于经验分配）
    /// </summary>
    int GetPartySize();
}