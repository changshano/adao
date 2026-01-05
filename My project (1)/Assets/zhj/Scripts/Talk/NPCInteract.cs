// NPCInteract.cs (修复版本)
using UnityEngine;

/// <summary>
/// 简易NPC交互脚本（范围检测 + 按键触发对话）
/// </summary>
public class NPCInteract : MonoBehaviour
{
    [Header("UI 引用（拖拽赋值）")]
    [Tooltip("交互按键提示面板（如\"按F交互\"）")]
    [SerializeField] private GameObject interactTip; // 交互提示UI
    
    [Tooltip("对话面板（触发后显示）")]
    [SerializeField] private GameObject dialogUI;    // 对话UI

    [Header("交互配置")]
    [Tooltip("交互有效范围（单位：米）")]
    [SerializeField] private float interactRange = 3f; // 交互范围
    
    [Tooltip("触发交互的按键")]
    [SerializeField] private KeyCode interactKey = KeyCode.F; // 交互按键

    [Header("调试设置")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);
    
    // 私有变量
    private Transform _playerTransform; // 玩家Transform引用
    private bool _isPlayerInRange;      // 玩家是否在交互范围内
    private bool _isDialogShowing;      // 对话UI是否正在显示
    
    // 调试属性
    private float _lastDistance = 0f;
    private int _checkCount = 0;

    #region 生命周期方法
    private void Start()
    {
        // 1. 查找玩家（通过"Player"标签，确保玩家对象添加该标签）
        FindPlayer();

        // 2. 初始化UI状态（默认隐藏）
        InitUIState();
        
        if (showDebugLogs)
        {
            Debug.Log($"[NPC交互] 初始化完成: {gameObject.name}");
            Debug.Log($"[NPC交互] 交互范围: {interactRange}");
            Debug.Log($"[NPC交互] 交互按键: {interactKey}");
        }
    }

    private void Update()
    {
        // 玩家未找到时，直接跳过后续逻辑
        if (_playerTransform == null) 
        {
            if (showDebugLogs && _checkCount % 60 == 0) // 每秒打印一次
            {
                Debug.LogWarning($"[NPC交互] 玩家未找到，请确保场景中有\"Player\"标签的对象");
            }
            _checkCount++;
            return;
        }

        // 3. 核心逻辑：帧同步检测玩家范围 + 按键交互
        CheckPlayerRange();
        CheckInteractInput();
        
        _checkCount++;
    }

    // 编辑器可视化交互范围（方便调试，仅在编辑器生效）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, interactRange); // 绘制球形交互范围
        
        // 绘制连接线
        if (_playerTransform != null && Application.isPlaying)
        {
            Gizmos.color = _isPlayerInRange ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _playerTransform.position);
        }
    }
    #endregion

    #region 核心功能方法
    /// <summary>
    /// 查找玩家对象（通过Player标签）
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 找到玩家: {playerObj.name}");
            }
        }
        else
        {
            Debug.LogError("[NPC交互] 未找到玩家！请给玩家对象添加\"Player\"标签并确保场景中存在玩家。");
            
            // 尝试通过其他方式查找
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length > 0)
            {
                _playerTransform = players[0].transform;
                Debug.Log($"[NPC交互] 通过备用方法找到玩家: {players[0].name}");
            }
        }
    }

    /// <summary>
    /// 初始化UI状态（隐藏提示和对话面板）
    /// </summary>
    private void InitUIState()
    {
        if (interactTip != null)
        {
            interactTip.SetActive(false);
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 交互提示UI: {interactTip.name} (已隐藏)");
            }
        }
        else
        {
            Debug.LogWarning("[NPC交互] 交互提示UI未设置！请在Inspector中拖拽赋值");
        }

        if (dialogUI != null)
        {
            dialogUI.SetActive(false);
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 对话UI: {dialogUI.name} (已隐藏)");
            }
        }
        else
        {
            Debug.LogWarning("[NPC交互] 对话UI未设置！请在Inspector中拖拽赋值");
        }

        _isDialogShowing = false;
    }

    /// <summary>
    /// 检测玩家是否在交互范围内，更新状态并切换提示UI
    /// </summary>
    private void CheckPlayerRange()
    {
        // 计算NPC与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        _lastDistance = distanceToPlayer;
        
        if (showDebugLogs && _checkCount % 60 == 0) // 每秒打印一次距离
        {
            Debug.Log($"[NPC交互] 距离玩家: {distanceToPlayer:F2}米, 在范围内: {distanceToPlayer <= interactRange}");
        }

        // 玩家进入范围
        if (distanceToPlayer <= interactRange && !_isPlayerInRange && !_isDialogShowing)
        {
            _isPlayerInRange = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 玩家进入范围! 距离: {distanceToPlayer:F2}米");
            }
            
            ShowInteractTip();
        }
        // 玩家离开范围
        else if (distanceToPlayer > interactRange && _isPlayerInRange)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 玩家离开范围! 距离: {distanceToPlayer:F2}米");
            }
            
            _isPlayerInRange = false;
            HideInteractTip();

            // 玩家离开范围时，自动关闭对话UI
            if (_isDialogShowing)
            {
                HideDialogUI();
            }
        }
    }

    /// <summary>
    /// 检测交互按键输入，触发对话UI
    /// </summary>
    private void CheckInteractInput()
    {
        // 满足条件：玩家在范围内 + 未显示对话 + 按下交互键
        if (_isPlayerInRange && !_isDialogShowing && Input.GetKeyDown(interactKey))
        {
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 检测到交互按键: {interactKey}");
            }
            
            HideInteractTip(); // 隐藏交互提示
            ShowDialogUI();    // 显示对话UI
        }
        
        // 如果对话正在显示，按下ESC关闭
        if (_isDialogShowing && Input.GetKeyDown(KeyCode.Escape))
        {
            HideDialogUI();
        }
    }
    #endregion

    #region UI 控制方法
    /// <summary>
    /// 显示交互提示
    /// </summary>
    private void ShowInteractTip()
    {
        if (interactTip != null && !interactTip.activeSelf)
        {
            interactTip.SetActive(true);
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 显示交互提示");
            }
        }
        else if (interactTip == null)
        {
            Debug.LogError("[NPC交互] 尝试显示交互提示，但interactTip为null！");
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    private void HideInteractTip()
    {
        if (interactTip != null && interactTip.activeSelf)
        {
            interactTip.SetActive(false);
            
            if (showDebugLogs)
            {
                Debug.Log($"[NPC交互] 隐藏交互提示");
            }
        }
    }

    /// <summary>
    /// 显示对话UI
    /// </summary>
    private void ShowDialogUI()
    {
        if (dialogUI != null && !dialogUI.activeSelf)
        {
            dialogUI.SetActive(true);
            _isDialogShowing = true;
            
            if (showDebugLogs)
            {
                Debug.Log("[NPC交互] 显示对话UI");
            }
        }
        else if (dialogUI == null)
        {
            Debug.LogError("[NPC交互] 尝试显示对话，但dialogUI为null！");
        }
    }

    /// <summary>
    /// 隐藏对话UI（可外部调用，如对话面板的关闭按钮）
    /// </summary>
    public void HideDialogUI()
    {
        if (dialogUI != null && dialogUI.activeSelf)
        {
            dialogUI.SetActive(false);
            _isDialogShowing = false;
            
            if (showDebugLogs)
            {
                Debug.Log("[NPC交互] 关闭对话UI");
            }

            // 玩家仍在范围内时，重新显示交互提示
            if (_isPlayerInRange)
            {
                ShowInteractTip();
            }
        }
    }
    #endregion
    
    #region 调试方法
    [ContextMenu("测试: 强制显示交互提示")]
    public void DebugForceShowTip()
    {
        if (interactTip != null)
        {
            interactTip.SetActive(true);
            Debug.Log($"强制显示交互提示: {interactTip.name}");
        }
    }
    
    [ContextMenu("测试: 强制隐藏交互提示")]
    public void DebugForceHideTip()
    {
        if (interactTip != null)
        {
            interactTip.SetActive(false);
            Debug.Log($"强制隐藏交互提示: {interactTip.name}");
        }
    }
    
    [ContextMenu("打印当前状态")]
    public void DebugPrintStatus()
    {
        Debug.Log($"=== NPC交互状态 ===");
        Debug.Log($"NPC名称: {gameObject.name}");
        Debug.Log($"玩家Transform: {_playerTransform != null}");
        Debug.Log($"距离玩家: {_lastDistance:F2}米");
        Debug.Log($"交互范围: {interactRange}米");
        Debug.Log($"玩家在范围内: {_isPlayerInRange}");
        Debug.Log($"对话显示中: {_isDialogShowing}");
        Debug.Log($"交互提示UI引用: {interactTip != null}");
        Debug.Log($"对话UI引用: {dialogUI != null}");
        Debug.Log($"交互提示UI激活: {interactTip != null && interactTip.activeSelf}");
        Debug.Log($"对话UI激活: {dialogUI != null && dialogUI.activeSelf}");
    }
    
    [ContextMenu("模拟玩家进入范围")]
    public void DebugSimulatePlayerEnter()
    {
        _isPlayerInRange = true;
        ShowInteractTip();
        Debug.Log("已模拟玩家进入范围");
    }
    
    [ContextMenu("模拟玩家离开范围")]
    public void DebugSimulatePlayerLeave()
    {
        _isPlayerInRange = false;
        HideInteractTip();
        Debug.Log("已模拟玩家离开范围");
    }
    #endregion
}