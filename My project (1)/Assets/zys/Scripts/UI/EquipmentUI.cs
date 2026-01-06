using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI Instance { get; private set; }

    [Header("装备UI引用")]
    [SerializeField] private Canvas canvas;  // 场景中的Canvas

    [Header("装备槽位路径")]
    [Tooltip("装备武器图标的完整路径")]
    [SerializeField] private string weaponIconPath = "Canvas/EquipmentPanel/WeaponSlot/Icon";

    [Header("组件引用")]
    private Image weaponIconImage;  // 武器图标组件
    private ItemSO currentWeapon;   // 当前装备的武器

    [Header("默认图标")]
    [SerializeField] private Sprite defaultWeaponIcon;  // 默认武器图标

    // 获取武器图标
    public Sprite CurrentWeaponIcon
    {
        get { return weaponIconImage != null ? weaponIconImage.sprite : null; }
    }

    // 获取当前装备的武器
    public ItemSO CurrentWeapon
    {
        get { return currentWeapon; }
    }

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

        // 如果Canvas未指定，尝试查找
        if (canvas == null)
        {
            canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("场景中没有找到Canvas!");
                return;
            }
        }

        // 查找武器图标
        InitializeWeaponIcon();
    }

    // 初始化武器图标组件
    private void InitializeWeaponIcon()
    {
        if (canvas == null) return;

        // 方法1: 通过路径查找
        Transform weaponIconTransform = canvas.transform.Find(weaponIconPath);
        if (weaponIconTransform != null)
        {
            weaponIconImage = weaponIconTransform.GetComponent<Image>();
        }

        // 方法2: 如果路径查找失败，尝试通过标签查找
        if (weaponIconImage == null)
        {
            GameObject weaponIconObj = GameObject.FindGameObjectWithTag("WeaponIcon");
            if (weaponIconObj != null)
            {
                weaponIconImage = weaponIconObj.GetComponent<Image>();
            }
        }

        // 方法3: 如果都失败，尝试在Canvas下搜索
        if (weaponIconImage == null)
        {
            Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.gameObject.name.Contains("Weapon") ||
                    img.gameObject.name.Contains("Icon"))
                {
                    weaponIconImage = img;
                    break;
                }
            }
        }

        if (weaponIconImage == null)
        {
            Debug.LogWarning($"未找到武器图标组件! 路径: {weaponIconPath}");
        }
        else
        {
            // 设置默认图标
            if (defaultWeaponIcon != null)
            {
                weaponIconImage.sprite = defaultWeaponIcon;
            }
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
        if (weaponItem.itemType != ItemType.Weapon)
        {
            Debug.LogWarning($"物品类型 {weaponItem.itemType} 不是武器，无法装备！");
            return;
        }

        // 确保图标组件已初始化
        if (weaponIconImage == null)
        {
            InitializeWeaponIcon();
            if (weaponIconImage == null)
            {
                Debug.LogError("无法初始化武器图标组件，装备失败！");
                return;
            }
        }

        // 更新当前装备的武器
        currentWeapon = weaponItem;

        // 更新图标显示
        if (weaponItem.icon != null)
        {
            weaponIconImage.sprite = weaponItem.icon;
            weaponIconImage.color = Color.white;  // 确保完全显示

            // 可选：记录日志
            Debug.Log($"装备武器成功: {weaponItem.name} (ID: {weaponItem.id})");
        }
        else
        {
            Debug.LogWarning("武器图标为空，使用默认图标！");
            if (defaultWeaponIcon != null)
            {
                weaponIconImage.sprite = defaultWeaponIcon;
            }
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

        // 触发装备事件（如果需要）
        OnWeaponEquipped(weaponItem);
    }

    // 装备武器时触发的事件
    private void OnWeaponEquipped(ItemSO weapon)
    {
        Debug.Log($"已装备武器: {weapon.name}");

        // 这里可以添加装备武器后的逻辑，比如：
        // 1. 更新角色属性
        // 2. 播放音效
        // 3. 触发UI更新
        // 4. 保存游戏状态

        // 示例：如果武器有属性，可以应用这些属性
        if (weapon.propertyList != null && weapon.propertyList.Count > 0)
        {
            foreach (var property in weapon.propertyList)
            {
                // 这里可以根据属性类型做不同处理
                Debug.Log($"武器属性: {property.propertyType} = {property.value}");

                // 示例：更新角色属性
                // ApplyWeaponProperty(property);
            }
        }
    }

    // 卸下武器
    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;

        // 记录之前装备的武器
        ItemSO previousWeapon = currentWeapon;

        // 可以在这里将武器添加回背包
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.AddItem(previousWeapon);
        }

        // 重置当前武器
        currentWeapon = null;

        // 恢复默认图标
        if (weaponIconImage != null && defaultWeaponIcon != null)
        {
            weaponIconImage.sprite = defaultWeaponIcon;
        }

        Debug.Log($"卸下武器: {previousWeapon.name}");

        // 触发卸下事件
        OnWeaponUnequipped(previousWeapon);
    }

    // 卸下武器时触发的事件
    private void OnWeaponUnequipped(ItemSO weapon)
    {
        // 这里可以添加卸下武器后的逻辑
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

        if (weaponIconImage != null && defaultWeaponIcon != null)
        {
            weaponIconImage.sprite = defaultWeaponIcon;
        }
    }

    // 检查是否已装备武器
    public bool HasWeaponEquipped()
    {
        return currentWeapon != null;
    }

    // 手动设置图标路径（如果需要动态修改）
    public void SetWeaponIconPath(string newPath)
    {
        weaponIconPath = newPath;
        InitializeWeaponIcon();  // 重新初始化
    }

    // 手动刷新UI（例如场景切换后）
    public void RefreshUI()
    {
        InitializeWeaponIcon();

        // 如果当前有装备武器，刷新图标
        if (currentWeapon != null && weaponIconImage != null && currentWeapon.icon != null)
        {
            weaponIconImage.sprite = currentWeapon.icon;
        }
    }

    // 在编辑器中快速设置的方法
    [ContextMenu("自动查找武器图标")]
    private void AutoFindWeaponIcon()
    {
        if (canvas == null)
        {
            canvas = GameObject.FindObjectOfType<Canvas>();
        }

        if (canvas != null)
        {
            // 尝试查找常见的武器图标命名
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.gameObject.name.ToLower().Contains("weapon") &&
                    img.gameObject.name.ToLower().Contains("icon"))
                {
                    weaponIconImage = img;
                    Debug.Log($"找到武器图标: {img.gameObject.name}");
                    return;
                }
            }
        }

        Debug.LogWarning("未找到武器图标，请手动设置。");
    }
}