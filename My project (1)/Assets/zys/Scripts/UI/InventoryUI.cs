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
        GameObject itemGo = GameObject.Instantiate(itemPrefab);
        itemGo.transform.parent = content.transform;
        ItemUI itemUI = itemGo.GetComponent<ItemUI>();

        itemUI.InitItem(itemSO);
    }
    
    public void OnItemClick(ItemSO itemSO, ItemUI itemUI)
    {
        itemDetailUI.UpdateItemDetailUI(itemSO, itemUI);
    }

    public void OnItemUse(ItemSO itemSO, ItemUI itemUI)
    {
        // 检查物品类型是否为武器
        if (itemSO.itemType == ItemType.Weapon)
        {
            // 如果是武器类型，跳转到装备UI
            if (EquipmentUI.Instance != null)
            {
                EquipmentUI.Instance.EquipWeapon(itemSO, itemUI);
            }
        }
        else
        {
            // 处理消耗品类型的物品
            Destroy(itemUI.gameObject);
            InventoryManager.Instance.RemoveItem(itemSO);

            // 这里可以添加消耗品使用的逻辑
            Debug.Log($"使用了消耗品: {itemSO.name}");

            // 示例：如果消耗品有属性，可以应用这些属性
            if (itemSO.propertyList != null && itemSO.propertyList.Count > 0)
            {
                // 处理消耗品属性
                foreach (var property in itemSO.propertyList)
                {
                    // 这里可以根据属性类型做不同处理
                    Debug.Log($"消耗品属性: {property.propertyType} = {property.value}");
                }
            }
        }


        // GameObject.FindGameObjectWithTag(Tag.PLAYER).GetComponent<Player>().UseItem(itemSO);
    }

    public void OnItemDiscard(ItemSO itemSO, ItemUI itemUI)
    {
        Destroy(itemUI.gameObject);
        InventoryManager.Instance.RemoveItem(itemSO);
    }
}
