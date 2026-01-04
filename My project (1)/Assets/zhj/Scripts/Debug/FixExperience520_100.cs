// FixExperience520_100.cs
using UnityEngine;
using System.Reflection;
using System.Collections;

public class FixExperience520_100 : MonoBehaviour
{
    [Header("自动修复")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private float delay = 3f;
    
    [Header("调试")]
    [SerializeField] private bool verboseLogging = true;
    
    private void Start()
    {
        if (autoFixOnStart)
        {
            StartCoroutine(DelayedFix());
        }
    }
    
    private IEnumerator DelayedFix()
    {
        yield return new WaitForSeconds(delay);
        RunComprehensiveFix();
    }
    
    [ContextMenu("运行综合修复")]
    public void RunComprehensiveFix()
    {
        Log("<color=cyan>=== 经验条520/100问题修复 ===</color>");
        
        // 1. 诊断问题
        DiagnoseProblem();
        
        // 2. 修复问题
        FixProblem();
        
        // 3. 验证修复
        StartCoroutine(VerifyFix());
    }
    
    private void DiagnoseProblem()
    {
        Log("1. 诊断问题...");
        
        if (ExperienceManager.Instance == null)
        {
            LogError("ExperienceManager 未找到");
            return;
        }
        
        if (LevelManager.Instance == null)
        {
            LogError("LevelManager 未找到");
            return;
        }
        
        // 获取当前状态
        int currentLevel = LevelManager.Instance.GetCurrentLevel();
        int currentExp = GetCurrentExperience();
        int expToNext = GetExpToNextLevel();
        
        Log($"当前状态: 等级{currentLevel}, 经验{currentExp}/{expToNext}");
        
        // 诊断问题
        if (currentExp >= expToNext && expToNext > 0)
        {
            LogError($"<color=red>诊断结果: 发现520/100问题！</color>");
            LogError($"当前经验({currentExp}) >= 升级所需({expToNext})，但未触发升级");
            LogError($"原因: 升级后经验未正确重置，溢出经验未处理");
        }
        else
        {
            Log($"诊断结果: 经验值正常");
        }
        
        // 检查是否应该升级
        if (ShouldLevelUp(currentExp, expToNext))
        {
            LogWarning("应该触发升级但未触发");
        }
    }
    
    private void FixProblem()
    {
        Log("2. 修复问题...");
        
        int currentExp = GetCurrentExperience();
        int expToNext = GetExpToNextLevel();
        
        if (currentExp < expToNext || expToNext <= 0)
        {
            Log("无需修复: 经验值正常");
            return;
        }
        
        Log($"<color=yellow>开始修复: 当前经验{currentExp}/{expToNext}</color>");
        
        // 方法1: 触发升级检查
        Log("尝试方法1: 触发升级检查...");
        TriggerLevelUpCheck();
        
        // 稍等一会儿
        StartCoroutine(DelayedCheck());
    }
    
    private IEnumerator DelayedCheck()
    {
        yield return new WaitForSeconds(0.5f);
        
        // 检查修复是否生效
        int currentExp = GetCurrentExperience();
        int expToNext = GetExpToNextLevel();
        
        if (currentExp >= expToNext && expToNext > 0)
        {
            LogWarning("方法1未完全解决问题，尝试方法2...");
            
            // 方法2: 手动计算并设置正确的经验值
            ManualFixExperience();
        }
    }
    
    private void ManualFixExperience()
    {
        int currentExp = GetCurrentExperience();
        int expToNext = GetExpToNextLevel();
        int currentLevel = LevelManager.Instance.GetCurrentLevel();
        
        Log($"手动修复: 等级{currentLevel}, 经验{currentExp}/{expToNext}");
        
        // 计算溢出经验
        int overflowExp = currentExp - expToNext;
        
        if (overflowExp >= 0)
        {
            // 应该升级
            Log($"计算溢出经验: {currentExp} - {expToNext} = {overflowExp}");
            
            // 触发升级
            LevelManager.Instance.LevelUp();
            
            // 设置新的经验值
            int newLevel = LevelManager.Instance.GetCurrentLevel();
            int newExpToNext = LevelManager.Instance.GetExpToNextLevel();
            
            // 更新经验
            SetExperience(overflowExp, newExpToNext);
            
            Log($"<color=green>手动修复完成:</color>");
            Log($"  等级: {currentLevel} → {newLevel}");
            Log($"  经验: {currentExp}/{expToNext} → {overflowExp}/{newExpToNext}");
        }
    }
    
    private IEnumerator VerifyFix()
    {
        yield return new WaitForSeconds(1f);
        
        Log("3. 验证修复...");
        
        int currentExp = GetCurrentExperience();
        int expToNext = GetExpToNextLevel();
        
        if (currentExp < expToNext && expToNext > 0)
        {
            LogSuccess($"<color=green>修复成功！</color> 当前经验: {currentExp}/{expToNext}");
            
            // 更新UI
            UpdateExperienceBarUI();
        }
        else
        {
            LogError($"<color=red>修复失败！</color> 经验: {currentExp}/{expToNext}");
        }
    }
    
    private void TriggerLevelUpCheck()
    {
        // 尝试调用ManualCheckForLevelUp
        var method = typeof(ExperienceManager).GetMethod("ManualCheckForLevelUp");
        if (method != null)
        {
            method.Invoke(ExperienceManager.Instance, null);
            Log("已调用ManualCheckForLevelUp");
        }
        else
        {
            // 如果方法不存在，直接调用AddExperience触发检查
            ExperienceManager.Instance.AddExperience(0, "FixTrigger");
            Log("通过AddExperience(0)触发升级检查");
        }
    }
    
    private void SetExperience(int currentExp, int expToNext)
    {
        var method = typeof(ExperienceManager).GetMethod("SetExperience");
        if (method != null)
        {
            method.Invoke(ExperienceManager.Instance, new object[] { currentExp, expToNext });
        }
        else
        {
            // 通过反射设置字段
            SetField("currentExperience", currentExp);
            SetField("experienceToNextLevel", expToNext);
            
            // 触发事件
            ExperienceManager.Instance.onExperienceChanged?.Invoke(currentExp, expToNext);
        }
    }
    
    private int GetCurrentExperience()
    {
        var method = typeof(ExperienceManager).GetMethod("GetCurrentExperience");
        if (method != null)
        {
            return (int)method.Invoke(ExperienceManager.Instance, null);
        }
        
        // 通过反射获取字段
        return GetField<int>("currentExperience");
    }
    
    private int GetExpToNextLevel()
    {
        var method = typeof(ExperienceManager).GetMethod("GetExpToNextLevel");
        if (method != null)
        {
            return (int)method.Invoke(ExperienceManager.Instance, null);
        }
        
        // 通过反射获取字段
        return GetField<int>("experienceToNextLevel");
    }
    
    private T GetField<T>(string fieldName)
    {
        try
        {
            var field = typeof(ExperienceManager).GetField(fieldName, 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                return (T)field.GetValue(ExperienceManager.Instance);
            }
        }
        catch
        {
            // 忽略错误
        }
        return default(T);
    }
    
    private void SetField(string fieldName, object value)
    {
        try
        {
            var field = typeof(ExperienceManager).GetField(fieldName, 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(ExperienceManager.Instance, value);
            }
        }
        catch
        {
            // 忽略错误
        }
    }
    
    private bool ShouldLevelUp(int currentExp, int expToNext)
    {
        return currentExp >= expToNext && expToNext > 0;
    }
    
    private void UpdateExperienceBarUI()
    {
        ExperienceBar expBar = FindObjectOfType<ExperienceBar>();
        if (expBar != null)
        {
            var method = expBar.GetType().GetMethod("ForceUpdateExperienceBar");
            if (method != null)
            {
                method.Invoke(expBar, null);
            }
            else
            {
                // 尝试其他方法
                method = expBar.GetType().GetMethod("UpdateUI");
                if (method != null)
                {
                    method.Invoke(expBar, null);
                }
            }
        }
    }
    
    [ContextMenu("模拟520/100问题")]
    public void SimulateProblem()
    {
        Log("模拟520/100问题...");
        SetExperience(520, 100);
        RunComprehensiveFix();
    }
    
    [ContextMenu("重置到正常状态")]
    public void ResetToNormal()
    {
        Log("重置到正常状态...");
        SetExperience(50, 100);
        UpdateExperienceBarUI();
    }
    
    private void Log(string message)
    {
        if (verboseLogging) Debug.Log($"[经验修复] {message}");
    }
    
    private void LogWarning(string message)
    {
        if (verboseLogging) Debug.LogWarning($"[经验修复] {message}");
    }
    
    private void LogError(string message)
    {
        if (verboseLogging) Debug.LogError($"[经验修复] {message}");
    }
    
    private void LogSuccess(string message)
    {
        if (verboseLogging) Debug.Log($"<color=green>[经验修复] {message}</color>");
    }
}