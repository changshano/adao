// SimpleEventMonitor.cs
using UnityEngine;
using System.Collections.Generic;

public class SimpleEventMonitor : MonoBehaviour
{
    [System.Serializable]
    public class EventLog
    {
        public string eventName;
        public string parameters;
        public float time;
    }
    
    [Header("监控设置")]
    [SerializeField] private bool monitorExperienceEvents = true;
    [SerializeField] private bool monitorLevelEvents = true;
    [SerializeField] private int maxLogs = 20;
    
    [Header("事件日志")]
    [SerializeField] private List<EventLog> eventLogs = new List<EventLog>();
    
    private void OnEnable()
    {
        RegisterEventListeners();
    }
    
    private void OnDisable()
    {
        UnregisterEventListeners();
    }
    
    private void RegisterEventListeners()
    {
        if (monitorExperienceEvents)
        {
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.onExperienceChanged.AddListener(OnExperienceChanged);
                ExperienceManager.Instance.onExperienceGained.AddListener(OnExperienceGained);
            }
        }
        
        if (monitorLevelEvents)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.onLevelUp.AddListener(OnLevelUp);
                LevelManager.Instance.onLevelUpDetailed.AddListener(OnLevelUpDetailed);
            }
        }
    }
    
    private void UnregisterEventListeners()
    {
        if (monitorExperienceEvents && ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.onExperienceChanged.RemoveListener(OnExperienceChanged);
            ExperienceManager.Instance.onExperienceGained.RemoveListener(OnExperienceGained);
        }
        
        if (monitorLevelEvents && LevelManager.Instance != null)
        {
            LevelManager.Instance.onLevelUp.RemoveListener(OnLevelUp);
            LevelManager.Instance.onLevelUpDetailed.RemoveListener(OnLevelUpDetailed);
        }
    }
    
    private void OnExperienceChanged(int currentExp, int expToNext)
    {
        AddLog("onExperienceChanged", $"经验: {currentExp}/{expToNext}");
    }
    
    private void OnExperienceGained(int amount)
    {
        AddLog("onExperienceGained", $"获得经验: {amount}");
    }
    
    private void OnLevelUp(int newLevel)
    {
        AddLog("onLevelUp", $"新等级: {newLevel}");
        
        // 立即检查ExperienceBar
        CheckExperienceBarAfterLevelUp();
    }
    
    private void OnLevelUpDetailed(int newLevel, int skillPoints, int attributePoints)
    {
        AddLog("onLevelUpDetailed", $"等级:{newLevel}, 技能点:{skillPoints}, 属性点:{attributePoints}");
    }
    
    private void AddLog(string eventName, string parameters)
    {
        EventLog log = new EventLog
        {
            eventName = eventName,
            parameters = parameters,
            time = Time.time
        };
        
        eventLogs.Insert(0, log);
        
        if (eventLogs.Count > maxLogs)
        {
            eventLogs.RemoveAt(eventLogs.Count - 1);
        }
        
        Debug.Log($"<color=cyan>[事件] {eventName}: {parameters}</color>");
    }
    
    private void CheckExperienceBarAfterLevelUp()
    {
        ExperienceBar experienceBar = FindObjectOfType<ExperienceBar>();
        if (experienceBar != null)
        {
            // 获取当前等级文本
            TMPro.TextMeshProUGUI[] texts = experienceBar.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.text.Contains("Lv."))
                {
                    Debug.Log($"<color=yellow>ExperienceBar等级文本: '{text.text}'</color>");
                }
            }
        }
    }
    
    [ContextMenu("清除日志")]
    public void ClearLogs()
    {
        eventLogs.Clear();
        Debug.Log("事件日志已清除");
    }
    
    [ContextMenu("检查事件系统")]
    public void CheckEventSystem()
    {
        Debug.Log("=== 事件系统检查 ===");
        
        // 简单检查，不获取监听器数量
        if (ExperienceManager.Instance != null)
        {
            Debug.Log("✓ ExperienceManager 事件系统正常");
        }
        
        if (LevelManager.Instance != null)
        {
            Debug.Log("✓ LevelManager 事件系统正常");
        }
    }
    
    [ContextMenu("打印事件日志")]
    public void PrintEventLogs()
    {
        Debug.Log("=== 事件日志 ===");
        foreach (var log in eventLogs)
        {
            Debug.Log($"[{log.time:F1}] {log.eventName}: {log.parameters}");
        }
    }
}