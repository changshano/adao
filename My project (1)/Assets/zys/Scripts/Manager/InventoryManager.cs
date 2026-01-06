using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    private void Awake()
    {
        Debug.Log("InventoryManager Awake 被调用");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("存在多个InventoryManager实例，销毁当前实例");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("InventoryManager 单例初始化完成");
    }

    public List<ItemSO> itemList = new List<ItemSO>(); // 确保列表被初始化

    public void AddItem(ItemSO item)
    {
        Debug.Log($"尝试添加物品到背包: {item?.name}");

        if (item == null)
        {
            Debug.LogError("添加的物品为null!");
            return;
        }

        itemList.Add(item);
        Debug.Log($"物品已添加到列表，当前物品数量: {itemList.Count}");

        // 直接调用UI添加物品
        if (InventoryUI.Instance != null)
        {
            Debug.Log("InventoryUI实例存在，调用AddItem");
            InventoryUI.Instance.AddItem(item);
        }
        else
        {
            Debug.LogError("InventoryUI实例为null! 请检查是否在场景中创建了InventoryUI");
        }
    }

    public void RemoveItem(ItemSO itemSO)
    {
        itemList.Remove(itemSO);
    }
}
