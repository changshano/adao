// Interfaces/ISkillTree.cs
public interface ISkillTree
{
    void OnSkillPointsAllocated(string skillId, int allocatedPoints);
    bool CanAllocateSkillPoint(string skillId, int points = 1);
    int GetSkillLevel(string skillId);
    string GetSkillName(string skillId);
    string GetSkillDescription(string skillId);
}