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

    [Header("玩家引用")]
    public PlayerAction playerController;  // 玩家控制器引用

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestCurrentEquipment();
        }
    }

    void TestCurrentEquipment()
    {
        Debug.Log("=== 当前装备测试 ===");
        Debug.Log($"当前装备武器: {(currentWeapon != null ? currentWeapon.name : "无")}");
        Debug.Log($"玩家引用: {(playerController != null ? "已连接" : "未连接")}");

        if (playerController != null)
        {
            Debug.Log($"玩家基础攻击力: {playerController.GetBaseAttackDamage()}");
            Debug.Log($"玩家当前总攻击力: {playerController.GetTotalAttackDamage()}");
        }

        Debug.Log($"装备攻击力加成: {GetTotalAttackBonus()}");
        Debug.Log("=== 测试结束 ===");
    }

    public bool EquipWeapon(ItemSO weapon)
    {
        if (weapon.itemType != ItemType.Weapon)
        {
            Debug.LogWarning("尝试装备非武器物品");
            return false;
        }

        Debug.Log($"=== 开始装备流程: {weapon.name} ===");

        // 获取旧的装备武器
        ItemSO oldWeapon = currentWeapon;

        // 第一步：移除旧装备的属性
        if (currentWeapon != null)
        {
            Debug.Log($"卸下旧装备: {currentWeapon.name}");
            // 只移除当前装备的加成，不全部清空
            RemoveCurrentEquipmentBonus();
        }

        // 第二步：更新当前装备
        currentWeapon = weapon;
        Debug.Log($"当前装备更新为: {weapon.name}");

        // 第三步：应用新装备属性
        ApplyEquipmentAttributes();

        // 第四步：更新 UI
        if (weaponIcon != null && weapon.icon != null)
        {
            weaponIcon.sprite = weapon.icon;
            weaponIcon.enabled = true;
            weaponIcon.color = Color.white;
            Debug.Log($"UI图标已更新");
        }

        Debug.Log($"=== 装备完成 ===");
        return true;
    }

    // 新增方法：移除当前装备的加成
    private void RemoveCurrentEquipmentBonus()
    {
        if (currentWeapon == null || playerController == null) return;

        float attackBonus = GetWeaponAttackBonus(currentWeapon);
        if (attackBonus > 0)
        {
            playerController.RemoveAttackBonus(attackBonus);
        }
    }

    // 新增方法：获取武器的攻击力加成
    private float GetWeaponAttackBonus(ItemSO weapon)
    {
        if (weapon == null) return 0f;

        float total = 0f;
        foreach (ItemProperty property in weapon.propertyList)
        {
            if (property.propertyType == ItemPropertyType.AttackValue)
            {
                total += property.value;
            }
        }
        return total;
    }

    // 卸下武器的方法
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Debug.Log($"=== 开始卸下装备: {currentWeapon.name} ===");

            // 卸下前移除属性
            RemoveEquipmentAttributes();

            // 只添加到库存数据，不创建UI
            if (InventoryManager.Instance != null)
            {
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

    // 应用装备属性
    private void ApplyEquipmentAttributes()
    {
        Debug.Log($"=== 开始应用装备属性 ===");
        Debug.Log($"当前装备: {currentWeapon?.name}");
        Debug.Log($"玩家控制器: {(playerController != null ? "已连接" : "为空")}");

        if (currentWeapon != null && playerController != null)
        {
            float totalAttackBonus = 0f;

            // 计算武器的所有属性加成
            Debug.Log($"装备属性数量: {currentWeapon.propertyList.Count}");
            foreach (ItemProperty property in currentWeapon.propertyList)
            {
                Debug.Log($"属性类型: {property.propertyType}, 值: {property.value}");
                if (property.propertyType == ItemPropertyType.AttackValue)
                {
                    totalAttackBonus += property.value;
                }
            }

            // 应用攻击力加成
            Debug.Log($"总攻击力加成: {totalAttackBonus}");
            playerController.ApplyAttackBonus(totalAttackBonus);

            // 确认玩家当前攻击力
            Debug.Log($"玩家当前总攻击力: {playerController.GetTotalAttackDamage()}");
        }
        else
        {
            Debug.LogError("无法应用装备属性：currentWeapon 或 playerController 为空");
        }
        Debug.Log($"=== 结束应用装备属性 ===");
    }

    // 移除装备属性 - 修复：添加可选参数
    private void RemoveEquipmentAttributes(ItemSO specificWeapon = null)
    {
        // 如果没有指定装备，使用当前装备
        ItemSO weaponToRemove = specificWeapon ?? currentWeapon;

        if (weaponToRemove != null && playerController != null)
        {
            Debug.Log($"=== 开始移除装备属性: {weaponToRemove.name} ===");

            float totalAttackReduction = 0f;

            // 计算要移除的属性
            foreach (ItemProperty property in weaponToRemove.propertyList)
            {
                if (property.propertyType == ItemPropertyType.AttackValue)
                {
                    totalAttackReduction += property.value;
                }
            }

            if (totalAttackReduction > 0)
            {
                // 移除属性加成
                playerController.RemoveAttackBonus(totalAttackReduction);
                Debug.Log($"移除攻击力加成: {totalAttackReduction}");
            }
            else
            {
                Debug.Log($"该装备没有攻击力属性");
            }

            Debug.Log($"=== 属性移除完成 ===");
        }
        else
        {
            if (weaponToRemove == null) Debug.LogWarning("要移除属性的装备为空");
            if (playerController == null) Debug.LogWarning("玩家控制器为空");
        }
    }

    // 获取总攻击力加成
    public float GetTotalAttackBonus()
    {
        if (currentWeapon == null) return 0f;

        float total = 0f;
        foreach (ItemProperty property in currentWeapon.propertyList)
        {
            if (property.propertyType == ItemPropertyType.AttackValue)
            {
                total += property.value;
            }
        }
        return total;
    }

    // 测试武器属性
    public void TestWeaponAttributes(ItemSO weapon)
    {
        Debug.Log($"=== 测试武器属性 ===");
        Debug.Log($"武器名称: {weapon.name}");
        Debug.Log($"武器类型: {weapon.itemType}");
        Debug.Log($"属性数量: {weapon.propertyList.Count}");

        float totalAttackBonus = 0f;
        foreach (ItemProperty property in weapon.propertyList)
        {
            Debug.Log($"属性: {property.propertyType} = {property.value}");
            if (property.propertyType == ItemPropertyType.AttackValue)
            {
                totalAttackBonus += property.value;
            }
        }
        Debug.Log($"总攻击力加成: {totalAttackBonus}");
        Debug.Log($"=== 测试结束 ===");
    }
}