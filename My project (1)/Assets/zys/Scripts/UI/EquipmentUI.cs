using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance { get; private set; }

    [Header("装备槽引用")]
    public Image weaponIcon;  // 武器的 Image 组件
    public GameObject weaponSlot;  // 武器槽 GameObject

    [Header("当前装备")]
    public ItemSO currentWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // 装备武器的方法
    public bool EquipWeapon(ItemSO weapon)
    {
        if (weapon.itemType != ItemType.Weapon)
        {
            Debug.LogWarning("尝试装备非武器物品");
            return false;
        }

        // 这里可以返回旧装备，但不需要 out 参数
        ItemSO oldWeapon = currentWeapon;

        // 更新当前装备
        currentWeapon = weapon;

        // 更新 UI
        if (weaponIcon != null && weapon.icon != null)
        {
            weaponIcon.sprite = weapon.icon;
            weaponIcon.enabled = true;
            weaponIcon.color = Color.white;
        }

        Debug.Log($"已装备: {weapon.name}");
        return true;
    }

    // 卸下武器的方法
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            // 只添加到库存数据，不创建UI
            if (InventoryManager.Instance != null)
            {
                // 注意：这里只添加到数据列表
                InventoryManager.Instance.AddItem(currentWeapon);
            }

            // 清空装备槽
            if (weaponIcon != null)
            {
                weaponIcon.sprite = null;
                weaponIcon.enabled = false;
            }

            Debug.Log($"已卸下: {currentWeapon.name}");
            currentWeapon = null;
        }
    }

    // 检查是否已装备武器
    public bool IsWeaponEquipped()
    {
        return currentWeapon != null;
    }
}