// DiaLogmanager.cs (修复版)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiaLogmanager : MonoBehaviour
{
    /// <summary>
    /// 对话内容文本，csv格式
    /// </summary> 
    public TextAsset dialogDataFile;

    /// <summary>
    /// 左侧角色图像
    /// </summary>
    public SpriteRenderer spriteLeft;
    
    /// <summary>
    /// 右侧角色图像
    /// </summary>
    public SpriteRenderer spriteRight;

    /// <summary>
    /// 角色名字文本
    /// </summary>
    public TMP_Text nameText;

    /// <summary>
    /// 对话内容文本
    /// </summary>
    public TMP_Text dialogText;

    /// <summary>
    /// 角色图片列表
    /// </summary>
    public List<Sprite> sprites = new List<Sprite>();

    /// <summary>
    /// 角色名字对应图片的字典
    /// </summary>
    private Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>();
    
    /// <summary>
    /// 当前对话索引值
    /// </summary>
    public int dialogIndex = 0;
    
    /// <summary>
    /// 对话文本按行分割
    /// </summary>
    public string[] dialogRows;
    
    /// <summary>
    /// 继续按钮
    /// </summary>
    public Button nextButton;

    /// <summary>
    /// 选项按钮预制体
    /// </summary>
    public GameObject optionButton;
    
    /// <summary>
    /// 选项按钮父节点
    /// </summary>
    public Transform buttonGroup;
    
    /// <summary>
    /// 对话面板
    /// </summary>
    public GameObject dialogPanel;
    
    /// <summary>
    /// 调试模式
    /// </summary>
    [SerializeField] private bool debugMode = true;
    
    /// <summary>
    /// 是否正在显示对话
    /// </summary>
    private bool isShowingDialog = false;
    
    /// <summary>
    /// 对话是否结束
    /// </summary>
    private bool isDialogEnded = false;
    
    /// <summary>
    /// 对话结束后传送的目标位置
    /// </summary>
    [SerializeField] private Vector3 teleportTargetPosition = Vector3.zero;
    
    /// <summary>
    /// 是否在对话结束后传送玩家
    /// </summary>
    [SerializeField] private bool enableTeleport = false;
    
    /// <summary>
    /// 玩家对象标签
    /// </summary>
    [SerializeField] private string playerTag = "Player";
    
    /// <summary>
    /// 玩家对象引用
    /// </summary>
    private GameObject playerObject;
    
    /// <summary>
    /// 玩家对象的Transform组件
    /// </summary>
    private Transform playerTransform;
    
    /// <summary>
    /// 是否保持玩家的旋转
    /// </summary>
    [SerializeField] private bool keepRotation = true;
    
    /// <summary>
    /// 是否立即传送（不淡入淡出）
    /// </summary>
    [SerializeField] private bool instantTeleport = true;
    
    /// <summary>
    /// 传送前的延迟时间（秒）
    /// </summary>
    [SerializeField] private float teleportDelay = 0.5f;
    
    /// <summary>
    /// 传送时的淡入淡出面板
    /// </summary>
    [SerializeField] private Image fadePanel;
    
    /// <summary>
    /// 淡入淡出时间
    /// </summary>
    [SerializeField] private float fadeDuration = 0.5f;
    
    /// <summary>
    /// 传送后执行的UnityEvent
    /// </summary>
    [System.Serializable]
    public class TeleportEvent : UnityEngine.Events.UnityEvent { }
    public TeleportEvent onTeleportComplete;

    #region Unity生命周期
    private void Awake()
    {
        // 安全初始化
        SafeInitialize();
    }
    
    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"[对话管理器] Awake完成，开始初始化");
        }
        
        // 确保UI状态正确
        InitializeUI();
        
        // 查找玩家对象
        FindPlayerObject();
        
        // 如果已经有对话数据，开始对话
        if (dialogDataFile != null)
        {
            StartDialog();
        }
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 安全初始化
    /// </summary>
    private void SafeInitialize()
    {
        try
        {
            // 初始化字典
            InitializeImageDictionary();
            
            // 确保组件引用
            EnsureComponentReferences();
            
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 安全初始化完成");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 初始化失败: {e.Message}");
            Debug.LogError($"[对话管理器] 堆栈: {e.StackTrace}");
        }
    }
    
    /// <summary>
    /// 初始化图片字典
    /// </summary>
    private void InitializeImageDictionary()
    {
        try
        {
            // 清空字典
            imageDic.Clear();
            
            // 确保有足够的sprite
            if (sprites.Count >= 2)
            {
                imageDic["人"] = sprites[0];
                imageDic["人鱼"] = sprites[1];
                
                if (debugMode)
                {
                    Debug.Log($"[对话管理器] 图片字典初始化: 人={sprites[0].name}, 人鱼={sprites[1].name}");
                }
            }
            else
            {
                Debug.LogWarning($"[对话管理器] Sprite列表不足，需要至少2个sprite");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 初始化图片字典失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 确保组件引用
    /// </summary>
    private void EnsureComponentReferences()
    {
        // 尝试自动查找组件
        if (spriteLeft == null)
        {
            spriteLeft = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRight == null)
        {
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                if (sprite != spriteLeft)
                {
                    spriteRight = sprite;
                    break;
                }
            }
        }
        
        if (nameText == null)
        {
            nameText = GetComponentInChildren<TMP_Text>();
        }
        
        if (dialogText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in texts)
            {
                if (text != nameText)
                {
                    dialogText = text;
                    break;
                }
            }
        }
        
        if (nextButton == null)
        {
            nextButton = GetComponentInChildren<Button>();
        }
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 组件引用检查:");
            Debug.Log($"  spriteLeft: {spriteLeft != null}");
            Debug.Log($"  spriteRight: {spriteRight != null}");
            Debug.Log($"  nameText: {nameText != null}");
            Debug.Log($"  dialogText: {dialogText != null}");
            Debug.Log($"  nextButton: {nextButton != null}");
        }
    }
    
    /// <summary>
    /// 初始化UI状态
    /// </summary>
    private void InitializeUI()
    {
        // 确保对话面板初始状态
        if (dialogPanel != null && dialogPanel.activeSelf)
        {
            dialogPanel.SetActive(false);
        }
        
        // 初始化文本
        if (nameText != null)
        {
            nameText.text = "";
        }
        
        if (dialogText != null)
        {
            dialogText.text = "";
        }
        
        // 隐藏继续按钮
        if (nextButton != null && nextButton.gameObject.activeSelf)
        {
            nextButton.gameObject.SetActive(false);
        }
        
        // 清空选项按钮
        ClearOptionButtons();
        
        isShowingDialog = false;
        isDialogEnded = false;
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] UI初始化完成");
        }
    }
    
    /// <summary>
    /// 查找玩家对象
    /// </summary>
    private void FindPlayerObject()
    {
        try
        {
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
                if (debugMode)
                {
                    Debug.Log($"[对话管理器] 找到玩家对象: {playerObject.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[对话管理器] 未找到标签为 {playerTag} 的玩家对象");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 查找玩家对象失败: {e.Message}");
        }
    }
    #endregion

    #region 对话控制
    /// <summary>
    /// 开始对话
    /// </summary>
    public void StartDialog()
    {
        if (isShowingDialog) return;
        
        try
        {
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 开始对话");
            }
            
            // 重置对话索引
            dialogIndex = 0;
            
            // 读取对话数据
            if (dialogDataFile != null)
            {
                ReadText(dialogDataFile);
            }
            else
            {
                Debug.LogError($"[对话管理器] 对话数据文件未设置！");
                return;
            }
            
            // 显示对话面板
            if (dialogPanel != null && !dialogPanel.activeSelf)
            {
                dialogPanel.SetActive(true);
            }
            
            isShowingDialog = true;
            isDialogEnded = false;
            
            // 显示第一行对话
            ShowDiaLogRow();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 开始对话失败: {e.Message}");
            EndDialog();
        }
    }
    
    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialog()
{
    if (!isShowingDialog) return;
    
    if (debugMode)
    {
        Debug.Log($"[对话管理器] 结束对话");
    }
    
    isShowingDialog = false;
    isDialogEnded = true;
    
    // 发送对话结束事件
    SendMessage("OnDialogEnded", SendMessageOptions.DontRequireReceiver);
    
    // 传送玩家
    if (enableTeleport)
    {
        // 检查游戏对象是否活动
        if (this != null && this.gameObject != null && this.gameObject.activeInHierarchy)
        {
            StartCoroutine(TeleportPlayerCoroutine());
        }
        else
        {
            // 如果对象不活动，直接传送
            if (debugMode)
            {
                Debug.LogWarning($"[对话管理器] 游戏对象不活动，直接传送玩家");
            }
            
            // 立即传送玩家
            TeleportPlayerImmediate();
        }
    }
    else
    {
        // 立即隐藏面板
        HideDialogPanelImmediate();
    }
}
    /// <summary>
/// 立即隐藏面板
/// </summary>
private void HideDialogPanelImmediate()
{
    if (dialogPanel != null && dialogPanel.activeSelf)
    {
        dialogPanel.SetActive(false);
    }
    
    // 清理UI
    ClearUI();
}
/// <summary>
/// 清理UI
/// </summary>
private void ClearUI()
{
    // 清空文本
    if (nameText != null)
    {
        nameText.text = "";
    }
    
    if (dialogText != null)
    {
        dialogText.text = "";
    }
    
    // 隐藏继续按钮
    if (nextButton != null && nextButton.gameObject.activeSelf)
    {
        nextButton.gameObject.SetActive(false);
    }
    
    // 清空选项按钮
    ClearOptionButtons();
}
/// <summary>
/// 立即传送玩家
/// </summary>
private void TeleportPlayerImmediate()
{
    if (!enableTeleport || playerTransform == null) return;
    
    try
    {
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 立即传送玩家到位置: {teleportTargetPosition}");
        }
        
        // 保存当前旋转
        Quaternion originalRotation = playerTransform.rotation;
        
        // 更新玩家位置
        playerTransform.position = teleportTargetPosition;
        
        // 如果不需要保持旋转，重置旋转
        if (!keepRotation)
        {
            playerTransform.rotation = Quaternion.identity;
        }
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 玩家传送完成: {playerTransform.position}");
        }
        
        // 触发传送完成事件
        onTeleportComplete?.Invoke();
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[对话管理器] 传送玩家失败: {e.Message}");
    }
    finally
    {
        // 确保面板被隐藏
        HideDialogPanelImmediate();
    }
}
    /// <summary>
    /// 继续下一句对话
    /// </summary>
    public void OnClickNext()
    {
        if (!isShowingDialog || isDialogEnded) return;
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 点击继续按钮，当前索引: {dialogIndex}");
        }
        
        ShowDiaLogRow();
    }
    #endregion

    #region 玩家传送
    /// <summary>
    /// 传送玩家到目标位置
    /// </summary>
    public void TeleportPlayer()
    {
        if (!enableTeleport || playerTransform == null)
        {
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 传送功能已禁用或玩家对象为空");
            }
            return;
        }
        
        try
        {
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 开始传送玩家到位置: {teleportTargetPosition}");
            }
            
            // 保存当前旋转
            Quaternion originalRotation = playerTransform.rotation;
            
            // 更新玩家位置
            playerTransform.position = teleportTargetPosition;
            
            // 如果不需要保持旋转，重置旋转
            if (!keepRotation)
            {
                playerTransform.rotation = Quaternion.identity;
            }
            
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 玩家传送完成: {playerTransform.position}");
            }
            
            // 触发传送完成事件
            onTeleportComplete?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 传送玩家失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 带有延迟和淡入淡出的传送协程
    /// </summary>
    private IEnumerator TeleportPlayerCoroutine()
    {
        if (!enableTeleport || playerTransform == null)
        {
            yield break;
        }
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 开始传送协程，延迟: {teleportDelay}秒");
        }
        
        // 等待延迟
        if (teleportDelay > 0)
        {
            yield return new WaitForSeconds(teleportDelay);
        }
        
        // 淡出效果
        if (!instantTeleport && fadePanel != null)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // 传送玩家
        TeleportPlayer();
        
        // 淡入效果
        if (!instantTeleport && fadePanel != null)
        {
            yield return StartCoroutine(FadeIn());
        }
    }
    
    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;
        
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, 0);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        fadePanel.color = new Color(0, 0, 0, 1);
    }
    
    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;
        
        fadePanel.color = new Color(0, 0, 0, 1);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 设置传送目标位置
    /// </summary>
    public void SetTeleportTargetPosition(Vector3 newPosition)
    {
        teleportTargetPosition = newPosition;
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 设置传送目标位置: {teleportTargetPosition}");
        }
    }
    
    /// <summary>
    /// 设置传送功能开关
    /// </summary>
    public void SetTeleportEnabled(bool enabled)
    {
        enableTeleport = enabled;
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 传送功能: {enableTeleport}");
        }
    }
    
    /// <summary>
    /// 设置传送玩家对象
    /// </summary>
    public void SetPlayerObject(GameObject player)
    {
        playerObject = player;
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 设置玩家对象: {playerObject?.name ?? "null"}");
        }
    }
    #endregion

    #region 对话数据处理
    /// <summary>
    /// 读取对话文本
    /// </summary>
    public void ReadText(TextAsset _textAsset)
    {
        if (_textAsset == null)
        {
            Debug.LogError($"[对话管理器] 对话文本为空！");
            return;
        }
        
        try
        {
            // 按行分割文本
            dialogRows = _textAsset.text.Split('\n');
            
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 读取成功，行数: {dialogRows.Length}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 读取对话文本失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 显示对话行
    /// </summary>
    public void ShowDiaLogRow()
    {
        if (dialogRows == null || dialogRows.Length == 0)
        {
            Debug.LogError($"[对话管理器] 对话行为空！");
            EndDialog();
            return;
        }
        
        bool rowFound = false;
        
        for (int i = 0; i < dialogRows.Length; i++)
        {
            // 跳过空行
            if (string.IsNullOrWhiteSpace(dialogRows[i]))
                continue;
                
            string[] cells = dialogRows[i].Split(',');
            
            // 确保有足够的单元格
            if (cells.Length < 6)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[对话管理器] 第{i}行单元格不足: {cells.Length}");
                }
                continue;
            }
            
            // 对话行
            if (cells[0] == "#" && int.TryParse(cells[1], out int rowIndex) && rowIndex == dialogIndex)
            {
                // 更新文本
                UpdateText(cells[2], cells[4]);
                
                // 更新图片
                UpdateImage(cells[2], cells[3]);
                
                // 更新对话索引
                if (int.TryParse(cells[5], out int nextIndex))
                {
                    dialogIndex = nextIndex;
                }
                
                // 显示继续按钮
                if (nextButton != null && !nextButton.gameObject.activeSelf)
                {
                    nextButton.gameObject.SetActive(true);
                }
                
                rowFound = true;
                break;
            }
            // 选项行
            else if (cells[0] == "&" && int.TryParse(cells[1], out int optionIndex) && optionIndex == dialogIndex)
            {
                // 隐藏继续按钮
                if (nextButton != null && nextButton.gameObject.activeSelf)
                {
                    nextButton.gameObject.SetActive(false);
                }
                
                // 生成选项按钮
                GenerateOption(i);
                rowFound = true;
                break;
            }
            // 结束行
            else if (cells[0] == "end" && int.TryParse(cells[1], out int endIndex) && endIndex == dialogIndex)
            {
                if (debugMode)
                {
                    Debug.Log("[对话管理器] 剧情结束");
                }
                EndDialog();
                rowFound = true;
                break;
            }
        }
        
        if (!rowFound)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[对话管理器] 未找到索引为 {dialogIndex} 的对话行");
            }
            EndDialog();
        }
    }
    #endregion

    #region UI更新
    /// <summary>
    /// 更新文本信息
    /// </summary>
    public void UpdateText(string _name, string _text)
    {
        try
        {
            if (nameText != null)
            {
                nameText.text = _name;
            }
            
            if (dialogText != null)
            {
                dialogText.text = _text;
            }
            
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 更新文本: {_name}: {_text}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 更新文本失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 更新图片信息
    /// </summary>
    public void UpdateImage(string _name, string _position)
    {
        try
        {
            if (imageDic.TryGetValue(_name, out Sprite targetSprite))
            {
                if (_position == "左" && spriteLeft != null)
                {
                    spriteLeft.sprite = targetSprite;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[对话管理器] 更新左侧图片: {_name}");
                    }
                }
                else if (_position == "右" && spriteRight != null)
                {
                    spriteRight.sprite = targetSprite;
                    
                    if (debugMode)
                    {
                        Debug.Log($"[对话管理器] 更新右侧图片: {_name}");
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"[对话管理器] 未知位置或SpriteRenderer为空: {_position}");
                    }
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[对话管理器] 未找到角色图片: {_name}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 更新图片失败: {e.Message}");
        }
    }
    #endregion

    #region 选项系统
    /// <summary>
    /// 生成选项按钮
    /// </summary>
    public void GenerateOption(int _index)
    {
        if (_index >= dialogRows.Length)
        {
            Debug.LogError($"[对话管理器] 选项索引超出范围: {_index}");
            return;
        }
        
        try
        {
            string[] cells = dialogRows[_index].Split(',');
            
            if (cells[0] != "&" || cells.Length < 6)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[对话管理器] 无效的选项行格式");
                }
                return;
            }
            
            // 创建选项按钮
            GameObject button = Instantiate(optionButton, buttonGroup);
            
            // 设置按钮文本
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = cells[4];
            }
            
            // 绑定点击事件
            Button buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                if (int.TryParse(cells[5], out int optionId))
                {
                    buttonComponent.onClick.AddListener(() => OnOptionClick(optionId));
                }
                else
                {
                    Debug.LogError($"[对话管理器] 无法解析选项ID: {cells[5]}");
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"[对话管理器] 生成选项: {cells[4]} -> {cells[5]}");
            }
            
            // 递归生成下一个选项
            GenerateOption(_index + 1);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[对话管理器] 生成选项失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 选项点击事件
    /// </summary>
    public void OnOptionClick(int _id)
    {
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 选项点击: {_id}");
        }
        
        // 更新对话索引
        dialogIndex = _id;
        
        // 清空选项按钮
        ClearOptionButtons();
        
        // 显示下一行对话
        ShowDiaLogRow();
    }
    
    /// <summary>
    /// 清空选项按钮
    /// </summary>
    private void ClearOptionButtons()
    {
        if (buttonGroup == null) return;
        
        for (int i = buttonGroup.childCount - 1; i >= 0; i--)
        {
            Transform child = buttonGroup.GetChild(i);
            if (child != null && child.gameObject != null)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"[对话管理器] 清空选项按钮");
        }
    }
    #endregion

    #region 调试方法
    [ContextMenu("测试: 开始对话")]
    public void TestStartDialog()
    {
        StartDialog();
    }
    
    [ContextMenu("测试: 结束对话")]
    public void TestEndDialog()
    {
        EndDialog();
    }
    
    [ContextMenu("测试: 强制激活面板")]
    public void TestForceActivatePanel()
    {
        if (dialogPanel != null)
        {
            bool before = dialogPanel.activeSelf;
            dialogPanel.SetActive(true);
            bool after = dialogPanel.activeSelf;
            
            Debug.Log($"[对话管理器] 强制激活面板: {before} -> {after}");
        }
    }
    
    [ContextMenu("测试: 立即传送玩家")]
    public void TestTeleportPlayer()
    {
        TeleportPlayer();
    }
    
    [ContextMenu("设置传送位置为当前位置")]
    public void SetTeleportToCurrentPosition()
    {
        if (playerTransform != null)
        {
            teleportTargetPosition = playerTransform.position;
            Debug.Log($"[对话管理器] 设置传送位置为玩家当前位置: {teleportTargetPosition}");
        }
    }
    
    [ContextMenu("打印状态")]
    public void PrintStatus()
    {
        Debug.Log($"=== 对话管理器状态 ===");
        Debug.Log($"显示对话中: {isShowingDialog}");
        Debug.Log($"对话已结束: {isDialogEnded}");
        Debug.Log($"当前索引: {dialogIndex}");
        Debug.Log($"对话行数: {(dialogRows != null ? dialogRows.Length : 0)}");
        Debug.Log($"面板激活: {dialogPanel != null && dialogPanel.activeSelf}");
        Debug.Log($"图片字典数量: {imageDic.Count}");
        Debug.Log($"传送功能: {enableTeleport}");
        Debug.Log($"传送目标位置: {teleportTargetPosition}");
        Debug.Log($"玩家对象: {playerObject?.name ?? "null"}");
    }
    
    [ContextMenu("验证UI引用")]
    public void VerifyUIRefs()
    {
        Debug.Log($"=== UI引用验证 ===");
        Debug.Log($"对话面板: {dialogPanel != null}");
        Debug.Log($"左侧Sprite: {spriteLeft != null}");
        Debug.Log($"右侧Sprite: {spriteRight != null}");
        Debug.Log($"名字文本: {nameText != null}");
        Debug.Log($"对话文本: {dialogText != null}");
        Debug.Log($"继续按钮: {nextButton != null}");
        Debug.Log($"选项按钮预制体: {optionButton != null}");
        Debug.Log($"按钮组: {buttonGroup != null}");
        Debug.Log($"淡入淡出面板: {fadePanel != null}");
    }
    #endregion
}