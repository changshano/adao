using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("血量设置")]
    public float maxHP = 500f; // 最大血量
    public float currentHP;    // 当前血量
    public bool isDead = false; // 是否死亡

    [Header("受伤效果")]
    public Color damageColor = Color.red; // 受伤时颜色
    public float damageFlashDuration = 0.1f; // 受伤闪烁时间
    private SpriteRenderer spriteRenderer; // 精灵渲染器
    private Color originalColor; // 原始颜色

    [Header("死亡设置")]
    public GameObject deathEffect; // 死亡特效
    public float destroyDelay = 1f; // 死亡后销毁延迟

    [Header("移动设置")]
    public Transform target; // 玩家目标
    public float moveSpeed = 3f; // 移动速度
    public float followDistance = 10f; // 跟随距离
    public float stopDistance = 2f; // 停止距离
    public float musicCombatDistance = 15f; // 新增：音乐战斗距离（可以比followDistance大）

    [Header("攻击设置")]
    public float attackRange = 3f; // 攻击范围
    public float attackDamage = 30f; // 攻击伤害
    public float attackCooldown = 2f; // 攻击冷却时间
    private float lastAttackTime; // 上一次攻击的时间
    public float attackDelay = 0.5f; // 攻击延迟时间

    [Header("阶段设置")]
    public float phase1Threshold = 0.7f; // 第一阶段血量阈值（70%）
    public float phase2Threshold = 0.3f; // 第二阶段血量阈值（30%）
    public float phase2MoveSpeedMultiplier = 1.5f; // 第二阶段移动速度倍率
    public float phase2AttackMultiplier = 1.5f; // 第二阶段攻击倍率
    public float phase3AttackMultiplier = 2f; // 第三阶段攻击倍率

    [Header("物品掉落")]
    public GameObject[] dropItems; // 可能掉落的物品
    public float dropChance = 0.8f; // 掉落概率

    // 动画组件
    private Animator animator;
    private Rigidbody2D rb;

    // 状态变量
    private bool isChasing = false;
    private bool isAttacking = false;
    private int currentPhase = 1; // 当前阶段

    // ========== 修改：音乐相关变量 ==========
    private bool wasInCombat = false; // 上次是否在战斗状态（不是追逐状态）
    private static bool bossBattleMusicPlaying = false; // 静态变量，确保只有Boss战斗音乐

    void Start()
    {
        // 初始化血量
        currentHP = maxHP;

        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 获取玩家目标
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        // 记录原始颜色
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 初始化攻击冷却
        lastAttackTime = -attackCooldown;

        // 初始化战斗状态
        wasInCombat = false;
    }

    void Update()
    {
        if (isDead) return;

        CheckCurrentPhase();
        UpdateState();

        // ========== 修改：音乐切换检测 ==========
        // 检测玩家是否在战斗范围内
        bool isInCombat = IsPlayerInCombatRange();

        if (isInCombat != wasInCombat)
        {
            if (isInCombat)
            {
                // 玩家进入战斗范围，播放Boss战斗音乐
                PlayBossBattleMusic();
                Debug.Log("玩家进入Boss战斗范围，播放Boss战斗音乐");
            }
            else
            {
                // 玩家离开战斗范围，切换回正常音乐
                StopBossBattleMusic();
                Debug.Log("玩家离开Boss战斗范围，切换回正常音乐");
            }
            wasInCombat = isInCombat;
        }
        // ======================================

        // 检测死亡
        if (currentHP <= 0 && !isDead)
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        MoveTowardsTarget();
    }

    // ========== 新增：检测玩家是否在战斗范围内 ==========
    /// <summary>
    /// 检测玩家是否在Boss的战斗音乐范围内
    /// 注意：这个范围可以比追逐范围大，确保攻击期间音乐不中断
    /// </summary>
    bool IsPlayerInCombatRange()
    {
        if (target == null) return false;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        return distanceToTarget <= musicCombatDistance;
    }
    // ==================================================

    // ========== 修改：Boss战斗音乐控制方法 ==========
    /// <summary>
    /// 播放Boss战斗音乐
    /// </summary>
    void PlayBossBattleMusic()
    {
        if (bossBattleMusicPlaying) return;

        if (AudioManager.Instance != null)
        {
            // 播放Boss战斗音乐
            AudioManager.Instance.PlayBattleMusic(true);
            bossBattleMusicPlaying = true;
        }
        else
        {
            Debug.LogWarning("AudioManager未找到！");
        }
    }

    /// <summary>
    /// 停止Boss战斗音乐
    /// </summary>
    void StopBossBattleMusic()
    {
        if (!bossBattleMusicPlaying) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalBackgroundMusic(true);
            bossBattleMusicPlaying = false;
        }
    }
    // =============================================

    // 检测当前阶段
    void CheckCurrentPhase()
    {
        float healthPercentage = currentHP / maxHP;

        if (healthPercentage > phase1Threshold)
        {
            currentPhase = 1;
        }
        else if (healthPercentage > phase2Threshold)
        {
            currentPhase = 2;
        }
        else
        {
            currentPhase = 3;
        }
    }

    // 更新状态
    void UpdateState()
    {
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 更新追逐状态（用于移动和动画）
        isChasing = distanceToTarget <= followDistance && distanceToTarget > stopDistance;

        // 更新动画参数
        if (animator != null)
        {
            animator.SetBool("Move", isChasing && !isAttacking);

            // 根据阶段设置速度倍率
            float speedMultiplier = 1f;
            if (currentPhase == 2) speedMultiplier = phase2MoveSpeedMultiplier;

            animator.SetFloat("Speed", isChasing ? moveSpeed * speedMultiplier : 0f);
        }

        // 检测是否应该攻击
        if (distanceToTarget <= attackRange &&
            Time.time >= lastAttackTime + attackCooldown &&
            !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    // 向目标移动
    void MoveTowardsTarget()
    {
        if (!isChasing || target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 movement = direction * moveSpeed * Time.fixedDeltaTime;

        // 根据阶段调整速度
        if (currentPhase == 2)
        {
            movement *= phase2MoveSpeedMultiplier;
        }

        rb.MovePosition(rb.position + movement);

        // 翻转朝向
        if (direction.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (direction.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    // 攻击协程
    IEnumerator Attack()
    {
        isAttacking = true;

        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 等待攻击动画的延迟
        yield return new WaitForSeconds(attackDelay);

        // 造成伤害
        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            float damage = attackDamage;

            // 根据阶段调整伤害
            if (currentPhase == 2) damage *= phase2AttackMultiplier;
            else if (currentPhase == 3) damage *= phase3AttackMultiplier;

            // 这里调用玩家的受伤方法
            PlayerAction playerAction = target.GetComponent<PlayerAction>();
            if (playerAction != null)
            {
                playerAction.TakeDamage(damage);
            }
        }

        // 重置攻击计时器
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(0.5f); // 攻击后摇
        isAttacking = false;
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 扣血
        currentHP -= damage;

        // 受伤反馈
        StartCoroutine(DamageFlash());

        Debug.Log($"{gameObject.name} 受到 {damage} 点伤害，剩余血量: {currentHP}");

        // 检查是否死亡
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    // 受伤闪烁效果
    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    // 死亡处理
    void Die()
    {
        if (isDead) return;

        isDead = true;

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // 停止所有移动和攻击
        rb.velocity = Vector2.zero;
        isAttacking = false;
        isChasing = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNormalBackgroundMusic(true);
            Debug.Log("Boss死亡，切换回正常音乐");
        }

        // 禁用碰撞体
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 播放死亡特效
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 物品掉落
        DropItems();

        Debug.Log($"{gameObject.name} 已死亡");

        // 延迟销毁（现在不在Die方法中调用GameOver）
        Destroy(gameObject, destroyDelay);
        // GameManager.Instance.GameOver(); // 移到动画事件中调用
    }

    // 动画事件调用的方法
    public void OnDeathAnimationComplete()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // 物品掉落
    void DropItems()
    {
        if (dropItems.Length == 0) return;

        float randomValue = Random.Range(0f, 1f);
        if (randomValue <= dropChance)
        {
            int randomIndex = Random.Range(0, dropItems.Length);
            if (dropItems[randomIndex] != null)
            {
                Instantiate(dropItems[randomIndex], transform.position, Quaternion.identity);
            }
        }
    }

    // ========== 修改：确保对象销毁时停止音乐 ==========
    void OnDestroy()
    {
        // 如果Boss被销毁时还在播放战斗音乐，停止它
        StopBossBattleMusic();
    }
    // =============================================
}