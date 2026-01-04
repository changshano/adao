// LevelSystemDebugger.cs
using UnityEngine;
using System.Collections.Generic;

public class LevelSystemDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool autoDebugOnStart = true;
    [SerializeField] private float debugDelay = 2f;
    
    [Header("UI引用")]
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private LevelUpUI levelUpUI;
    
    private void Start()
    {
        if (autoDebugOnStart)
        {
            Invoke("RunDebug", debugDelay);
        }
    }
    
    [ContextMenu("运行等级系统调试")]
    public void RunDebug()
    {
        Debug.Log("=== 等级系统调试开始 ===");
        
        CheckManagers();
        CheckEvents();
        CheckUI();
        TestLevelUp();
        
        Debug.Log("=== 等级系统调试结束 ===");
    }
    
    private void CheckManagers()
    {
        Debug.Log("检查管理器...");
        
        // 检查ExperienceManager
        if (ExperienceManager.Instance != null)
        {
            Debug.Log($"✓ ExperienceManager 存在 (单例)");
            Debug.Log($"  当前经验: {ExperienceManager.Instance.GetCurrentExperience()}");
            Debug.Log($"  下一级需要: {ExperienceManager.Instance.GetExpToNextLevel()}");
            Debug.Log($"  经验进度: {ExperienceManager.Instance.GetExperienceProgress():P0}");
            
            // 不调用GetExperienceMultiplier，改为获取其他可用信息
            // 如果GetExperienceMultiplier不存在，注释掉这行
            // Debug.Log($"  经验倍率: {ExperienceManager.Instance.GetExperienceMultiplier()}x");
            
            // 添加替代的调试信息
            Debug.Log($"  ExperienceManager 组件状态: 正常");
        }
        else
        {
            Debug.LogError("✗ ExperienceManager 不存在");
        }
        
        // 检查LevelManager
        if (LevelManager.Instance != null)
        {
            Debug.Log($"✓ LevelManager 存在 (单例)");
            Debug.Log($"  当前等级: {LevelManager.Instance.GetCurrentLevel()}");
            Debug.Log($"  技能点: {LevelManager.Instance.GetAvailableSkillPoints()}");
            Debug.Log($"  属性点: {LevelManager.Instance.GetAvailableAttributePoints()}");
        }
        else
        {
            Debug.LogError("✗ LevelManager 不存在");
        }
    }
    
    private void CheckEvents()
    {
        Debug.Log("检查事件监听...");
        
        if (experienceBar != null)
        {
            Debug.Log($"✓ ExperienceBar 存在: {experienceBar.name}");
            experienceBar.ForceUpdateLevelDisplay();
        }
        else
        {
            Debug.LogWarning("! ExperienceBar 未设置，尝试查找...");
            experienceBar = FindObjectOfType<ExperienceBar>();
            if (experienceBar != null)
            {
                Debug.Log($"  ✓ 找到ExperienceBar: {experienceBar.name}");
            }
        }
        
        if (levelUpUI != null)
        {
            Debug.Log($"✓ LevelUpUI 存在");
        }
    }
    
    private void CheckUI()
    {
        Debug.Log("检查UI组件...");
        
        ExperienceBar[] bars = FindObjectsOfType<ExperienceBar>();
        Debug.Log($"找到 {bars.Length} 个ExperienceBar");
        
        foreach (var bar in bars)
        {
            Debug.Log($"  - {bar.name} (启用: {bar.isActiveAndEnabled})");
            
            // 检查等级文本
            TMPro.TextMeshProUGUI[] texts = bar.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.Contains("Lv."))
                {
                    Debug.Log($"    等级文本: '{text.text}'");
                }
            }
        }
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"✓ 找到Canvas: {canvas.name}");
        }
    }
    
    private void TestLevelUp()
    {
        Debug.Log("测试等级提升...");
        
        if (LevelManager.Instance != null && ExperienceManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance.GetCurrentLevel();
            int expNeeded = ExperienceManager.Instance.GetExpToNextLevel();
            int expToAdd = expNeeded + 100;
            
            Debug.Log($"当前等级: {currentLevel}");
            Debug.Log($"需要经验升级: {expNeeded}");
            Debug.Log($"添加经验: {expToAdd}");
            
            ExperienceManager.Instance.AddExperience(expToAdd, "Debug");
            
            // 检查等级是否提升
            StartCoroutine(CheckLevelAfterDelay(currentLevel));
        }
    }
    
    private System.Collections.IEnumerator CheckLevelAfterDelay(int beforeLevel)
    {
        yield return new WaitForSeconds(1f);
        
        if (LevelManager.Instance != null)
        {
            int newLevel = LevelManager.Instance.GetCurrentLevel();
            Debug.Log($"升级后等级: {newLevel}");
            
            if (newLevel > beforeLevel)
            {
                Debug.Log("✓ 等级提升测试成功");
            }
            else
            {
                Debug.LogError("✗ 等级提升测试失败");
                
                // 提供更多调试信息
                Debug.Log($"当前经验: {ExperienceManager.Instance?.GetCurrentExperience()}");
                Debug.Log($"下一级需要: {ExperienceManager.Instance?.GetExpToNextLevel()}");
            }
        }
    }
    
    [ContextMenu("手动添加经验测试")]
    public void ManualAddExpTest()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.AddExperience(500, "ManualTest");
            Debug.Log("手动添加500经验");
        }
    }
    
    [ContextMenu("手动升级测试")]
    public void ManualLevelUpTest()
    {
        if (LevelManager.Instance != null)
        {
            int before = LevelManager.Instance.GetCurrentLevel();
            LevelManager.Instance.LevelUp();
            int after = LevelManager.Instance.GetCurrentLevel();
            Debug.Log($"手动触发升级: {before} -> {after}");
        }
    }
    
    [ContextMenu("检查ExperienceBar等级显示")]
    public void CheckExperienceBarDisplay()
    {
        ExperienceBar bar = FindObjectOfType<ExperienceBar>();
        if (bar != null)
        {
            bar.ForceUpdateLevelDisplay();
            
            // 打印当前显示的等级
            TMPro.TextMeshProUGUI[] texts = bar.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.Contains("Lv."))
                {
                    Debug.Log($"当前显示的等级: {text.text}");
                }
            }
        }
    }
}