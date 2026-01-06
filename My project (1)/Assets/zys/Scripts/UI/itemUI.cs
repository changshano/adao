using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Header("UI引用")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;

    [Header("按钮组件")]
    public Button itemButton;

    [Header("物品数据")]
    public ItemSO itemSO;

    private void Start()
    {
        // 如果按钮没有在Inspector中设置，尝试获取
        if (itemButton == null)
        {
            itemButton = GetComponent<Button>();
        }

        // 添加点击监听
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners(); // 清除旧监听
            itemButton.onClick.AddListener(OnClick);
        }
    }

    // 初始化item
    public void InitItem(ItemSO itemSO)
    {
        Debug.Log($"初始化ItemUI: {itemSO?.name}");

        if (itemSO == null)
        {
            Debug.LogError("ItemUI.InitItem: itemSO为空");
            return;
        }

        this.itemSO = itemSO;

        string type = "";
        switch (itemSO.itemType)
        {
            case ItemType.Weapon:
                type = "武器";
                break;
            case ItemType.Consumable:
                type = "可消耗品";
                break;
        }

        // 更新UI显示
        if (iconImage != null && itemSO.icon != null)
        {
            iconImage.sprite = itemSO.icon;
        }

        if (nameText != null)
        {
            nameText.text = itemSO.name;
        }

        if (typeText != null)
        {
            typeText.text = type;
        }
    }

    public void OnClick()
    {
        Debug.Log($"物品被点击: {itemSO?.name}");

        if (itemSO == null)
        {
            Debug.LogError("点击的物品ItemSO为空");
            return;
        }

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.OnItemClick(itemSO, this);
        }
    }
}