using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PickableObject : InteractableObject
{
    public ItemSO itemSO;
    
    protected override void Interact()
    {
        Debug.Log($"尝试拾取：{itemSO.name}");
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemSO);
            Debug.Log($"成功拾取：{itemSO.name}");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("InventoryManager实例为空！");
        }
    }
}