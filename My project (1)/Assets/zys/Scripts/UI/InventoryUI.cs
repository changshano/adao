// using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    private GameObject uiGameObject;
    private GameObject content;
    public GameObject itemPrefab;
    private bool isShow = false;

    public ItemDetailUI itemDetailUI;

    [Header("回血特效设置")]
    [SerializeField] private int healTextFontSize = 12;  // 调整字体大小为12
    [SerializeField] private float healTextDuration = 2.5f;  // 调整持续时间为2.5秒
    [SerializeField] private float healTextFloatHeight = 1.5f;  // 浮动高度调整为1.5
    [SerializeField] private Color healTextColor = Color.green;  // 文字颜色

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        uiGameObject = transform.Find("UI").gameObject;
        content = transform.Find("UI/ListBg/Scroll View/Viewport/Content").gameObject;
        Hide();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isShow)
            {
                Hide();
                isShow = false;
            }
            else
            {
                Show();
                isShow = true;
            }
        }
    }


    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    public void AddItem(ItemSO itemSO)
    {
        Debug.Log($"InventoryUI.AddItem 被调用，物品: {itemSO?.name}");

        if (itemSO == null)
        {
            Debug.LogError("AddItem: itemSO为空");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogError("AddItem: itemPrefab为空，请检查Inspector中的设置");
            return;
        }

        if (content == null)
        {
            Debug.LogError("AddItem: content为空，请检查UI层级结构");
            return;
        }

        Debug.Log("开始实例化物品UI");

        // 使用正确的实例化方法
        GameObject itemGo = Instantiate(itemPrefab, content.transform);

        // 确保RectTransform正确设置
        RectTransform rectTransform = itemGo.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector2(0, 0);
        }

        // 强制激活游戏对象
        itemGo.SetActive(true);

        ItemUI itemUI = itemGo.GetComponent<ItemUI>();

        if (itemUI != null)
        {
            Debug.Log("找到ItemUI组件，初始化物品");
            itemUI.InitItem(itemSO);
        }
        else
        {
            Debug.LogError("实例化的预制体缺少ItemUI组件");
        }

        Debug.Log($"物品UI创建完成: {itemGo.name}");
    }

    public void OnItemClick(ItemSO itemSO, ItemUI itemUI)
    {
        itemDetailUI.UpdateItemDetailUI(itemSO, itemUI);
    }

    public void OnItemUse(ItemSO itemSO, ItemUI itemUI)
    {
        if (itemSO.itemType == ItemType.Weapon)
        {
            EquipmentUI equipmentUI = EquipmentUI.Instance;

            if (equipmentUI != null)
            {
                // 获取旧的装备武器
                ItemSO oldWeapon = equipmentUI.currentWeapon;

                // 装备新武器
                bool success = equipmentUI.EquipWeapon(itemSO);

                if (success)
                {
                    // 从背包移除新武器
                    Destroy(itemUI.gameObject);
                    InventoryManager.Instance.RemoveItem(itemSO);

                    // 如果之前有装备，放回背包
                    if (oldWeapon != null)
                    {
                        InventoryManager.Instance.AddItem(oldWeapon);
                    }
                }

                Debug.Log($"装备完成");
            }
        }
        else if (itemSO.itemType == ItemType.Consumable)
        {
            // 处理可消耗品
            Debug.Log($"使用可消耗品: {itemSO.name}");

            // 获取玩家引用
            PlayerAction player = FindObjectOfType<PlayerAction>();

            if (player != null)
            {
                // 计算可消耗品的效果
                float totalHPBonus = 0f;

                foreach (ItemProperty property in itemSO.propertyList)
                {
                    if (property.propertyType == ItemPropertyType.HPValue)
                    {
                        totalHPBonus += property.value;
                    }
                }

                if (totalHPBonus > 0)
                {
                    // 恢复血量
                    player.Heal(totalHPBonus);
                    Debug.Log($"恢复 {totalHPBonus} 点生命值，当前血量: {player.GetCurrentHealth()}");

                    // 显示治疗效果提示
                    ShowHealEffect(totalHPBonus);
                }
                else
                {
                    Debug.LogWarning($"可消耗品 {itemSO.name} 没有生命值恢复效果");
                }

                // 从背包移除物品
                Destroy(itemUI.gameObject);
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.RemoveItem(itemSO);
                }

                // 自动关闭背包
                AutoCloseInventory();
            }
            else
            {
                Debug.LogError("找不到玩家对象，无法使用可消耗品");
            }
        }
        else
        {
            // 非武器物品的原有逻辑
            Destroy(itemUI.gameObject);
            InventoryManager.Instance.RemoveItem(itemSO);
        }
    }

    // 自动关闭背包
    private void AutoCloseInventory()
    {
        // 延迟一帧关闭背包，确保其他逻辑先执行完毕
        StartCoroutine(CloseInventoryWithDelay());
    }
    IEnumerator CloseInventoryWithDelay()
    {
        // 等待一帧
        yield return null;

        // 关闭背包
        Hide();
        isShow = false;

        Debug.Log("使用可消耗品后自动关闭背包");

        /*
        // 可选：播放关闭音效
        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        */
    }

    // 显示治疗效果提示
    private void ShowHealEffect(float healAmount)
    {
        // 找到玩家位置
        PlayerAction player = FindObjectOfType<PlayerAction>();
        if (player != null)
        {
            Vector3 playerPosition = player.transform.position;

            // 创建治疗效果文字
            GameObject healText = new GameObject("HealText");
            healText.transform.position = playerPosition + Vector3.up * 1f;  // 初始高度降低

            // 添加 TextMesh 组件
            TextMesh textMesh = healText.AddComponent<TextMesh>();
            textMesh.text = $"+{healAmount} HP";
            textMesh.fontSize = healTextFontSize;  // 使用配置的字体大小
            textMesh.color = healTextColor;  // 使用配置的颜色
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // 添加字符间距和字体样式
            textMesh.characterSize = 0.1f;  // 字符大小
            textMesh.fontStyle = FontStyle.Bold;  // 粗体

            // 添加描边效果（通过创建多个TextMesh层叠）
            AddTextOutline(healText, healAmount);

            // 添加浮动动画
            StartCoroutine(FloatHealText(healText));
        }
    }

    // 添加文字描边效果
private void AddTextOutline(GameObject parentText, float healAmount)
{
    // 创建4个方向偏移的文本作为描边
    Vector3[] offsets = {
        new Vector3(0.01f, 0.01f, 0),  // 右上
        new Vector3(0.01f, -0.01f, 0), // 右下
        new Vector3(-0.01f, 0.01f, 0), // 左上
        new Vector3(-0.01f, -0.01f, 0) // 左下
    };
    
    foreach (Vector3 offset in offsets)
    {
        GameObject outlineText = new GameObject("Outline");
        outlineText.transform.SetParent(parentText.transform);
        outlineText.transform.localPosition = offset;
        
        TextMesh outlineMesh = outlineText.AddComponent<TextMesh>();
        outlineMesh.text = $"+{healAmount} HP";
        outlineMesh.fontSize = healTextFontSize;
        outlineMesh.color = Color.black;  // 描边用黑色
        outlineMesh.anchor = TextAnchor.MiddleCenter;
        outlineMesh.alignment = TextAlignment.Center;
        outlineMesh.characterSize = 0.1f;
    }
}

    IEnumerator FloatHealText(GameObject textObject)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startPosition = textObject.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // 向上浮动
            textObject.transform.position = startPosition + Vector3.up * (2f * progress);

            // 渐变消失
            TextMesh textMesh = textObject.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = 1f - progress;
                textMesh.color = color;
            }

            yield return null;
        }

        Destroy(textObject);
    }

    public void OnItemDiscard(ItemSO itemSO, ItemUI itemUI)
    {
        Destroy(itemUI.gameObject);
        InventoryManager.Instance.RemoveItem(itemSO);
    }
}
