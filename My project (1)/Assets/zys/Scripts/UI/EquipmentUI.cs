using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance { get; private set; }

    // UI元素引用
    [Header("装备槽位")]
    public GameObject weaponSlot;  // 武器装备槽

    [Header("图标组件")]
    public Image weaponIcon;  // 武器图标显示

    [Header("默认图标")]
    public Sprite defaultWeaponIcon;  // 默认武器图标（空槽时显示）

    private ItemSO currentWeapon;  // 当前装备的武器

    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // 如果没有直接指定weaponIcon，尝试自动查找
        if (weaponIcon == null && weaponSlot != null)
        {
            // 在武器槽中查找Image组件
            weaponIcon = weaponSlot.GetComponent<Image>();

            // 如果还没有找到，尝试查找子对象
            if (weaponIcon == null)
            {
                Transform iconTransform = weaponSlot.transform.Find("Icon");
                if (iconTransform != null)
                {
                    weaponIcon = iconTransform.GetComponent<Image>();
                }
            }
        }

        // 初始化显示默认图标
        if (weaponIcon != null && defaultWeaponIcon != null)
        {
            weaponIcon.sprite = defaultWeaponIcon;
        }
    }

    // 装备武器
    public void EquipWeapon(ItemSO weaponItem, ItemUI originalItemUI = null)
    {
        if (weaponItem == null)
        {
            Debug.LogWarning("尝试装备空武器！");
            return;
        }

        // 检查是否是武器类型
        if (weaponItem.itemType != "武器" && weaponItem.itemType != "Weapon")
        {
            Debug.LogWarning($"物品类型 {weaponItem.itemType} 不是武器，无法装备！");
            return;
        }

        // 存储当前装备的武器
        currentWeapon = weaponItem;

        // 更新图标显示
        if (weaponItem.itemIcon != null && weaponIcon != null)
        {
            weaponIcon.sprite = weaponItem.itemIcon;
            weaponIcon.color = Color.white;  // 确保完全显示
        }
        else
        {
            Debug.LogWarning("武器图标或UI组件为空！");
        }

        // 如果提供了原始物品UI，销毁它
        if (originalItemUI != null)
        {
            Destroy(originalItemUI.gameObject);

            // 从库存管理器中移除物品
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(weaponItem);
            }
        }

        Debug.Log($"已装备武器: {weaponItem.itemName}");
    }

    // 卸下武器
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            // 可以在这里将武器添加回背包
            if (InventoryUI.Instance != null && currentWeapon != null)
            {
                InventoryUI.Instance.AddItem(currentWeapon);
            }

            // 重置当前武器
            currentWeapon = null;
        }

        // 恢复默认图标
        if (weaponIcon != null && defaultWeaponIcon != null)
        {
            weaponIcon.sprite = defaultWeaponIcon;
        }
    }

    // 获取当前装备的武器
    public ItemSO GetCurrentWeapon()
    {
        return currentWeapon;
    }

    // 清空装备槽
    public void ClearEquipment()
    {
        currentWeapon = null;

        if (weaponIcon != null && defaultWeaponIcon != null)
        {
            weaponIcon.sprite = defaultWeaponIcon;
        }
    }
}