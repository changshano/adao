using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public GameObject propertyGrid;
    public GameObject propertyTemplate;

    private ItemSO itemSO;
    private ItemUI itemUI;

    private void Start()
    {
        propertyTemplate.SetActive(false);
        this.gameObject.SetActive(false);

        // 添加预制体结构检查
        if (propertyTemplate != null)
        {
            Transform propertyChild = propertyTemplate.transform.Find("Property");
            if (propertyChild == null)
            {
                Debug.LogError($"[ItemDetailUI] propertyTemplate '{propertyTemplate.name}' 没有名为 'Property' 的子对象！");
            }
            else
            {
                TextMeshProUGUI text = propertyChild.GetComponent<TextMeshProUGUI>();
                if (text == null)
                {
                    Debug.LogError($"[ItemDetailUI] 'Property' 子对象没有 TextMeshProUGUI 组件！");
                }
            }
        }
    }

    public void UpdateItemDetailUI(ItemSO itemSO, ItemUI itemUI)
    {
        Debug.Log($"正在更新详情面板，物品名称: {itemSO?.name}");

        if (nameText == null)
            Debug.LogError("nameText 未赋值！");
        if (itemSO == null)
            Debug.LogError("itemSO 为空！");

        this.itemSO = itemSO;
        this.itemUI = itemUI;
        this.gameObject.SetActive(true);

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

        nameText.text = itemSO.name;

        foreach (Transform child in propertyGrid.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
        
        foreach (ItemProperty property in itemSO.propertyList)
        {
            string propertyStr = "";
            string propertyName = "";
            switch (property.propertyType)
            {
                case ItemPropertyType.HPValue:
                    propertyName = "生命值 +";
                    break;
                case ItemPropertyType.DefenseValue:
                    propertyName = "防御力 +";
                    break;
                case ItemPropertyType.AttackValue:
                    propertyName = "攻击力 +";
                    break;
                default:
                    break;
            }
            propertyStr += propertyName;
            propertyStr += property.value;

            // 调试1：检查模板
            if (propertyTemplate == null)
            {
                Debug.LogError("propertyTemplate 是空的！请检查 Inspector 设置");
                continue;
            }

            // 调试2：检查实例化
            GameObject go = GameObject.Instantiate(propertyTemplate);
            if (go == null)
            {
                Debug.LogError("实例化模板失败！");
                continue;
            }

            // GameObject go = GameObject.Instantiate(propertyTemplate);
            go.SetActive(true);
            go.transform.SetParent(propertyGrid.transform);

            // 调试3：检查 Find 结果
            Transform propertyTransform = go.transform.Find("Property");
            if (propertyTransform == null)
            {
                Debug.LogError("找不到名为 'Property' 的子对象！请检查预制体结构");
                // 打印所有子对象名称
                foreach (Transform child in go.transform)
                {
                    Debug.Log($"找到子对象: {child.name}");
                }
                continue;
            }

            // 调试4：检查组件
            TextMeshProUGUI textComp = propertyTransform.GetComponent<TextMeshProUGUI>();
            if (textComp == null)
            {
                Debug.LogError("'Property' 对象没有 TextMeshProUGUI 组件！");
                continue;
            }

            textComp.text = propertyStr;

            // go.transform.Find("Property").GetComponent<TextMeshProUGUI>().text = propertyStr;
        }
    }
    
    public void OnUseButtonClick()
    {
        InventoryUI.Instance.OnItemUse(itemSO, itemUI);
        this.gameObject.SetActive(false);
    }

    public void OnDiscardButtonClick()
    {
        InventoryUI.Instance.OnItemDiscard(itemSO, itemUI);
        this.gameObject.SetActive(false);
    }
}
