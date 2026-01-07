using UnityEngine;
using UnityEngine.UI;

public class SkillPanelController : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Canvas skillCanvas;      // Canvas组件
    [SerializeField] private GameObject skillPanel;    // 技能面板

    [Header("快捷键设置")]
    [SerializeField] private KeyCode toggleKey = KeyCode.T;  // 默认为T键

    private bool isPanelActive = false;  // 面板当前状态

    private void Start()
    {
        // 初始化时确保面板处于关闭状态
        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
            isPanelActive = false;
        }

        // 如果Canvas被禁用，启用它
        if (skillCanvas != null && !skillCanvas.enabled)
        {
            skillCanvas.enabled = true;
        }
    }

    private void Update()
    {
        // 检测是否按下了T键
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleSkillPanel();
        }

        // 可选：添加其他按键检测
        // 例如按下ESC键关闭面板
        if (isPanelActive && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSkillPanel();
        }
    }

    /// <summary>
    /// 切换技能面板的显示/隐藏状态
    /// </summary>
    public void ToggleSkillPanel()
    {
        if (skillPanel == null) return;

        isPanelActive = !isPanelActive;  // 反转状态
        skillPanel.SetActive(isPanelActive);

        Debug.Log($"技能面板状态: {(isPanelActive ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 打开技能面板
    /// </summary>
    public void OpenSkillPanel()
    {
        if (skillPanel == null) return;

        skillPanel.SetActive(true);
        isPanelActive = true;
    }

    /// <summary>
    /// 关闭技能面板
    /// </summary>
    public void CloseSkillPanel()
    {
        if (skillPanel == null) return;

        skillPanel.SetActive(false);
        isPanelActive = false;
    }
}