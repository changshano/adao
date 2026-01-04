// ExperienceBarDebugger.cs
using UnityEngine;
using System.Collections;

public class ExperienceBarDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private bool autoDebug = true;
    
    [Header("测试值")]
    [SerializeField] private int testCurrentExp = 520;
    [SerializeField] private int testExpToNext = 100;
    
    private float timer = 0f;
    
    private void Start()
    {
        if (experienceBar == null)
        {
            experienceBar = FindObjectOfType<ExperienceBar>();
        }
        
        if (autoDebug)
        {
            StartCoroutine(DelayedDebug());
        }
    }
    
    private void Update()
    {
        if (autoDebug)
        {
            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;
                CheckExperienceBarState();
            }
        }
    }
    
    private IEnumerator DelayedDebug()
    {
        yield return new WaitForSeconds(2f);
        
        Debug.Log("<color=cyan>=== 经验条调试器启动 ===</color>");
        CheckExperienceBarState();
    }
    
    [ContextMenu("检查经验条状态")]
    public void CheckExperienceBarState()
    {
        Debug.Log("<color=yellow>--- 经验条状态检查 ---</color>");
        
        if (experienceBar == null)
        {
            Debug.LogError("❌ ExperienceBar 未找到");
            return;
        }
        
        Debug.Log($"✓ ExperienceBar: {experienceBar.name}");
        
        // 检查管理器状态
        if (ExperienceManager.Instance == null)
        {
            Debug.LogError("❌ ExperienceManager 未找到");
            return;
        }
        
        if (LevelManager.Instance == null)
        {
            Debug.LogError("❌ LevelManager 未找到");
            return;
        }
        
        // 获取当前值
        int currentExp = ExperienceManager.Instance.GetCurrentExperience();
        int expToNext = ExperienceManager.Instance.GetExpToNextLevel();
        int currentLevel = LevelManager.Instance.GetCurrentLevel();
        
        Debug.Log($"当前等级: {currentLevel}");
        Debug.Log($"当前经验: {currentExp}/{expToNext}");
        Debug.Log($"经验进度: {ExperienceManager.Instance.GetExperienceProgress():P2}");
        
        // 检查经验值是否合理
        if (currentExp >= expToNext)
        {
            Debug.LogError($"❌ 经验异常: 当前经验({currentExp}) >= 升级所需({expToNext})，应该触发升级！");
        }
        
        if (expToNext <= 0)
        {
            Debug.LogError($"❌ 经验异常: 升级所需经验为{expToNext}，应该大于0");
        }
        
        // 检查UI显示
        CheckExperienceBarDisplay();
    }
    
    [ContextMenu("检查经验条显示")]
    public void CheckExperienceBarDisplay()
    {
        if (experienceBar == null) return;
        
        // 检查Slider
        UnityEngine.UI.Slider slider = experienceBar.GetComponentInChildren<UnityEngine.UI.Slider>();
        if (slider != null)
        {
            Debug.Log($"Slider值: {slider.value} (范围: {slider.minValue}-{slider.maxValue})");
        }
        
        // 检查文本显示
        TMPro.TextMeshProUGUI[] texts = experienceBar.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (var text in texts)
        {
            if (text.text.Contains("/"))
            {
                Debug.Log($"经验文本: '{text.text}'");
                
                // 解析文本
                string[] parts = text.text.Split('/');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0].Trim(), out int displayedCurrent) && 
                        int.TryParse(parts[1].Trim(), out int displayedNext))
                    {
                        int actualCurrent = ExperienceManager.Instance.GetCurrentExperience();
                        int actualNext = ExperienceManager.Instance.GetExpToNextLevel();
                        
                        if (displayedCurrent == actualCurrent && displayedNext == actualNext)
                        {
                            Debug.Log($"✓ 经验文本显示正确");
                        }
                        else
                        {
                            Debug.LogError($"❌ 经验文本显示错误！显示: {displayedCurrent}/{displayedNext}, 实际: {actualCurrent}/{actualNext}");
                        }
                    }
                }
            }
        }
    }
    
    [ContextMenu("模拟升级后状态")]
    public void SimulatePostLevelUpState()
    {
        Debug.Log("模拟升级后状态...");
        
        if (ExperienceManager.Instance != null && LevelManager.Instance != null)
        {
            // 模拟升级
            int beforeLevel = LevelManager.Instance.GetCurrentLevel();
            int beforeExp = ExperienceManager.Instance.GetCurrentExperience();
            int beforeExpToNext = ExperienceManager.Instance.GetExpToNextLevel();
            
            Debug.Log($"升级前: 等级{beforeLevel}, 经验{beforeExp}/{beforeExpToNext}");
            
            // 触发升级
            LevelManager.Instance.LevelUp();
            
            // 检查升级后状态
            StartCoroutine(CheckAfterLevelUp(beforeLevel, beforeExp, beforeExpToNext));
        }
    }
    
    private IEnumerator CheckAfterLevelUp(int beforeLevel, int beforeExp, int beforeExpToNext)
    {
        yield return new WaitForSeconds(0.5f);
        
        int afterLevel = LevelManager.Instance.GetCurrentLevel();
        int afterExp = ExperienceManager.Instance.GetCurrentExperience();
        int afterExpToNext = ExperienceManager.Instance.GetExpToNextLevel();
        
        Debug.Log($"升级后: 等级{afterLevel}, 经验{afterExp}/{afterExpToNext}");
        
        if (afterLevel > beforeLevel)
        {
            Debug.Log($"✓ 等级提升成功: {beforeLevel} -> {afterLevel}");
            
            // 检查经验是否重置
            if (afterExp < beforeExpToNext)
            {
                Debug.Log($"✓ 经验已重置: {beforeExp} -> {afterExp}");
            }
            else
            {
                Debug.LogError($"❌ 经验未重置！升级后经验({afterExp})不应该大于等于升级前所需({beforeExpToNext})");
            }
            
            // 检查经验条UI
            CheckExperienceBarDisplay();
        }
        else
        {
            Debug.LogError($"❌ 等级未提升！");
        }
    }
    
    [ContextMenu("强制修复经验条")]
    public void ForceFixExperienceBar()
    {
        Debug.Log("强制修复经验条...");
        
        if (ExperienceManager.Instance != null)
        {
            // 获取当前值
            int currentExp = ExperienceManager.Instance.GetCurrentExperience();
            int expToNext = ExperienceManager.Instance.GetExpToNextLevel();
            
            // 如果经验异常，修正
            if (currentExp >= expToNext && expToNext > 0)
            {
                Debug.LogWarning($"检测到经验异常: {currentExp}/{expToNext}，尝试修复...");
                
                // 手动触发升级检查
                ExperienceManager.Instance.ManualCheckForLevelUp();
                
                // 再次检查
                StartCoroutine(CheckAfterFix());
            }
            else
            {
                Debug.Log("经验值正常，无需修复");
            }
        }
    }
    
    private IEnumerator CheckAfterFix()
    {
        yield return new WaitForSeconds(0.5f);
        
        int currentExp = ExperienceManager.Instance.GetCurrentExperience();
        int expToNext = ExperienceManager.Instance.GetExpToNextLevel();
        
        Debug.Log($"修复后: 经验{currentExp}/{expToNext}");
        
        if (currentExp < expToNext)
        {
            Debug.Log("✓ 经验条修复成功");
        }
        else
        {
            Debug.LogError("❌ 经验条修复失败");
        }
    }
    
    [ContextMenu("测试异常经验值 520/100")]
    public void TestAbnormalExperience()
    {
        Debug.Log("测试异常经验值 520/100...");
        
        if (ExperienceManager.Instance != null)
        {
            // 设置异常经验值
            ExperienceManager.Instance.SetExperience(testCurrentExp, testExpToNext);
            
            Debug.Log($"已设置异常经验值: {testCurrentExp}/{testExpToNext}");
            
            // 检查状态
            CheckExperienceBarState();
            
            // 尝试修复
            ForceFixExperienceBar();
        }
    }
    
    [ContextMenu("重置测试")]
    public void ResetTest()
    {
        Debug.Log("重置经验系统测试...");
        
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.ResetExperienceBar();
        }
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetLevel(1, true);
        }
        
        Debug.Log("已重置到初始状态");
    }
}