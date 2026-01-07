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
                equipmentUI.EquipWeapon(itemSO);

                // 从背包移除新武器
                Destroy(itemUI.gameObject);
                InventoryManager.Instance.RemoveItem(itemSO);

                // 如果之前有装备武器，将其放回背包
                if (oldWeapon != null)
                {
                    // 注意：这里使用 AddItem 会同时更新数据和UI
                    InventoryManager.Instance.AddItem(oldWeapon);
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
            healText.transform.position = playerPosition + Vector3.up * 2f;

            TextMesh textMesh = healText.AddComponent<TextMesh>();
            textMesh.text = $"+{healAmount} HP";
            textMesh.fontSize = 20;
            textMesh.color = Color.green;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // 添加浮动动画
            StartCoroutine(FloatHealText(healText));
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
