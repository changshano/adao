using UnityEngine;

/// <summary>
/// 简易NPC交互脚本（范围检测 + 按键触发对话）
/// </summary>
public class SimpleNPCInteract : MonoBehaviour
{
    [Header("UI 引用（拖拽赋值）")]
    [Tooltip("交互按键提示面板（如“按F交互”）")]
    [SerializeField] private GameObject interactTip; // 交互提示UI
    [Tooltip("对话面板（触发后显示）")]
    [SerializeField] private GameObject dialogUI;    // 对话UI

    [Header("交互配置")]
    [Tooltip("交互有效范围（单位：米）")]
    [SerializeField] private float interactRange = 3f; // 交互范围
    [Tooltip("触发交互的按键")]
    [SerializeField] private KeyCode interactKey = KeyCode.F; // 交互按键

    // 私有变量
    private Transform _playerTransform; // 玩家Transform引用
    private bool _isPlayerInRange;      // 玩家是否在交互范围内
    private bool _isDialogShowing;      // 对话UI是否正在显示

    #region 生命周期方法
    private void Start()
    {
        // 1. 查找玩家（通过"Player"标签，确保玩家对象添加该标签）
        FindPlayer();

        // 2. 初始化UI状态（默认隐藏）
        InitUIState();
    }

    private void Update()
    {
        // 玩家未找到时，直接跳过后续逻辑
        if (_playerTransform == null) return;

        // 3. 核心逻辑：帧同步检测玩家范围 + 按键交互
        CheckPlayerRange();
        CheckInteractInput();
    }

    // 编辑器可视化交互范围（方便调试，仅在编辑器生效）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 255, 0, 0.3f); // 半透明绿色
        Gizmos.DrawSphere(transform.position, interactRange); // 绘制球形交互范围
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
        }
        else
        {
            Debug.LogError("未找到玩家！请给玩家对象添加“Player”标签并确保场景中存在玩家。");
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
        }

        if (dialogUI != null)
        {
            dialogUI.SetActive(false);
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

        // 玩家进入范围
        if (distanceToPlayer <= interactRange && !_isPlayerInRange && !_isDialogShowing)
        {
            _isPlayerInRange = true;
            ShowInteractTip();
        }
        // 玩家离开范围
        else if (distanceToPlayer > interactRange && _isPlayerInRange)
        {
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
            HideInteractTip(); // 隐藏交互提示
            ShowDialogUI();    // 显示对话UI
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
            Debug.Log("触发NPC对话，显示对话UI");
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
            Debug.Log("关闭NPC对话，隐藏对话UI");

            // 玩家仍在范围内时，重新显示交互提示
            if (_isPlayerInRange)
            {
                ShowInteractTip();
            }
        }
    }
    #endregion
}