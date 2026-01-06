using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public float maxHealth = 100f;  // 最大血量
    public float currentHealth = 85f;  // 当前血量（从你的UI看是85）

    [Header("UI引用（拖拽到这里）")]
    public Image healthBarImage;  // 血条的Image_Mask对象

    [Header("怪物攻击设置")]
    public float enemyDamage = 10f;  // 怪物每次攻击伤害
    public float attackCooldown = 1f;  // 攻击间隔（秒）
    private float lastAttackTime = 0f;

    [Header("测试设置")]
    public KeyCode testDamageKey = KeyCode.Space;  // 按这个键模拟被攻击

    void Start()
    {
        // 初始化血条显示
        UpdateHealthBar();

        // 如果忘记设置UI引用，尝试自动查找
        if (healthBarImage == null)
        {
            // 根据你的图片结构查找：Image_HPbar -> Image_Mask
            GameObject hpMask = GameObject.Find("Image_HPbar/Image_Mask");
            if (hpMask != null)
            {
                healthBarImage = hpMask.GetComponent<Image>();
                Debug.Log("自动找到了血条UI: " + (healthBarImage != null ? "成功" : "失败"));
            }
        }

        // 确保图片类型是Filled
        if (healthBarImage != null)
        {
            healthBarImage.type = Image.Type.Filled;
            healthBarImage.fillMethod = Image.FillMethod.Horizontal;
        }
    }

    void Update()
    {
        // 按空格键测试扣血
        if (Input.GetKeyDown(testDamageKey))
        {
            TakeDamage(enemyDamage);
        }
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        // 扣血
        currentHealth -= damage;

        // 确保血量不会低于0
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Debug.Log("阿岛已经阵亡！");
        }

        // 更新血条显示
        UpdateHealthBar();

        Debug.Log($"阿岛受到 {damage} 点伤害，剩余血量: {currentHealth}");
    }

    // 更新血条UI
    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            // 计算血量的百分比（0到1之间的数）
            float healthPercent = currentHealth / maxHealth;

            // 设置fillAmount，这个就是控制血条长度的关键！
            healthBarImage.fillAmount = healthPercent;

            Debug.Log($"血条更新: {healthPercent * 100}% (fillAmount = {healthPercent})");
        }
        else
        {
            Debug.LogError("没有找到血条Image组件！请拖拽Image_Mask到healthBarImage");
        }
    }

    // 自动被怪物攻击的方法
    public void GetAttackedByEnemy()
    {
        // 冷却时间检查
        if (Time.time - lastAttackTime > attackCooldown)
        {
            TakeDamage(enemyDamage);
            lastAttackTime = Time.time;
        }
    }

    // 恢复血量
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthBar();
        Debug.Log($"阿岛恢复了 {healAmount} 点血量，当前血量: {currentHealth}");
    }
}
