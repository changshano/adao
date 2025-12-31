// 最简单的修正版本
using UnityEngine;

[CreateAssetMenu(fileName = "GrowthConfig", menuName = "Game/Level System/Growth Config")]
public class GrowthConfigSO : ScriptableObject
{
    [System.Serializable]
    public class AttributeGrowth
    {
        public int baseHealth = 100;
        public int healthPerLevel = 20;
        public int baseAttack = 10;
        public int attackPerLevel = 2;
        public int baseDefense = 5;
        public int defensePerLevel = 1;
        public int baseStamina = 50;
        public int staminaPerLevel = 5;
    }
    
    [System.Serializable]
    public class ClassGrowth
    {
        public string className = "Warrior";
        public AttributeGrowth attributes = new AttributeGrowth();
        [Range(1, 3)] public int healthGrowthTier = 2;
        [Range(1, 3)] public int attackGrowthTier = 2;
        [Range(1, 3)] public int defenseGrowthTier = 2;
    }
    
    [Header("默认成长配置")]
    public AttributeGrowth defaultGrowth = new AttributeGrowth();
    
    [Header("职业成长配置")]
    public ClassGrowth[] classGrowths = new ClassGrowth[3];
    
    [Header("难度经验调整")]
    [Range(0.5f, 2f)] public float easyMultiplier = 0.8f;
    [Range(0.5f, 2f)] public float normalMultiplier = 1f;
    [Range(0.5f, 2f)] public float hardMultiplier = 1.2f;
    [Range(0.5f, 2f)] public float nightmareMultiplier = 1.5f;
    
    [Header("其他配置")]
    public bool enableLevelCap = true;
    public int softLevelCap = 50;
    public int hardLevelCap = 100;
    [Range(1f, 5f)] public float postSoftCapMultiplier = 3f;
    
    /// <summary>
    /// 获取难度经验倍率
    /// </summary>
    public float GetDifficultyMultiplier(string difficulty)
    {
        return difficulty switch
        {
            "Easy" => easyMultiplier,
            "Normal" => normalMultiplier,
            "Hard" => hardMultiplier,
            "Nightmare" => nightmareMultiplier,
            _ => 1f
        };
    }
    
    /// <summary>
    /// 根据职业获取成长配置
    /// </summary>
    public AttributeGrowth GetClassGrowth(string className)
    {
        if (classGrowths == null) return defaultGrowth;
        
        foreach (var classGrowth in classGrowths)
        {
            if (classGrowth != null && classGrowth.className == className)
            {
                return classGrowth.attributes;
            }
        }
        return defaultGrowth;
    }
}