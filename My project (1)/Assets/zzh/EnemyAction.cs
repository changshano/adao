using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // 添加全局静态变量来追踪正在追逐玩家的敌人数量
    private static int chasingEnemiesCount = 0;
    private static bool isBattleMusicPlaying = false;


    // 在现有Header下方添加经验值设置
    [Header("经验值设置")]
    public int expValue = 10; // 击败后获得的经验值（可调整）
    public string expSourceType = "Enemy"; // 经验来源类型
    [Header("血量设置")]
    public float maxHP = 100f; // 最大血量
    public float currentHP;    // 当前血量
    public bool isDead = false; // 是否死亡

    [Header("受伤效果")]
    public Color damageColor = Color.red; // 受伤时颜色
    public float damageFlashDuration = 0.1f; // 受伤闪烁时间
    private SpriteRenderer spriteRenderer; // 精灵渲染器
    private Color originalColor; // 原始颜色

    [Header("死亡设置")]
    public GameObject deathEffect; // 死亡特效
    public float destroyDelay = 2f; // 死亡后销毁延迟
    public string deathTriggerName = "Die"; // 死亡动画触发器名称

    [Header("移动设置")]
    public Transform target;
    public float enemyMoveSpeed;
    public float followDistance;

    [Header("攻击设置")]
    public float attackDistance; // 攻击触发的距离
    public float attackDamage;   // 攻击伤害
    public float attackCooldown; // 攻击冷却时间
    private float lastAttackTime; // 上一次攻击的时间
    public float attackDelay = 0.5f; // 新增：攻击动画延迟时间

    [Header("物品掉落")]
    public GameObject[] dropItems; // 可能掉落的物品
    public float dropChance = 0.5f; // 掉落概率

    // 动画组件
    private Animator animator;
    private bool isFollowingPlayer = false;

    private Rigidbody2D rb;


    void Start()
    {
        // 初始化血量
        currentHP = maxHP;

        rb = GetComponent<Rigidbody2D>();

        // 获取目标玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }

        // 获取组件
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }

        // 记录原始颜色
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 初始化攻击冷却
        lastAttackTime = -attackCooldown; // 初始化为可立即攻击
    }

    void Update()
    {
        // 如果敌人死亡，不执行任何逻辑
        if (isDead) return;

        // 保存之前的跟随状态
        bool previousFollowingState = isFollowingPlayer;

        FollowPlayer();

        // 检查跟随状态是否改变
        if (isFollowingPlayer != previousFollowingState)
        {
            // 更新全局追逐敌人计数
            if (isFollowingPlayer)
            {
                // 开始跟随，增加计数
                chasingEnemiesCount++;
            }
            else
            {
                // 停止跟随，减少计数（确保不为负数）
                chasingEnemiesCount = Mathf.Max(0, chasingEnemiesCount - 1);
            }

            // 根据全局计数切换音乐
            UpdateBattleMusic();
        }

        UpdateAnimationState();
        CheckAttack();
    }

    private static void UpdateBattleMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager未找到！");
            return;
        }

        // 如果有敌人在追逐玩家，播放战斗音乐
        if (chasingEnemiesCount > 0 && !isBattleMusicPlaying)
        {
            AudioManager.Instance.PlayBattleMusic(true);
            isBattleMusicPlaying = true;
            Debug.Log($"切换到战斗音乐，当前追逐敌人数：{chasingEnemiesCount}");
        }
        // 如果没有敌人在追逐，播放正常音乐
        else if (chasingEnemiesCount == 0 && isBattleMusicPlaying)
        {
            AudioManager.Instance.PlayNormalBackgroundMusic(true);
            isBattleMusicPlaying = false;
            Debug.Log("切换到正常背景音乐");
        }
    }


    // 受到伤害
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 扣血
        currentHP -= damage;

        // 受伤反馈
        StartCoroutine(DamageFlash());

        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

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

        // 如果死亡时正在追逐，减少计数
        if (isFollowingPlayer)
        {
            isFollowingPlayer = false;
            chasingEnemiesCount = Mathf.Max(0, chasingEnemiesCount - 1);
            UpdateBattleMusic();
        }

        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerName);
        }

        // 停止移动和攻击
        enemyMoveSpeed = 0;

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
        // ========== 新增：给玩家添加经验值 ==========
        GiveExperienceToPlayer();
        // =========================================

        Debug.Log($"{gameObject.name} 已死亡");

        // 延迟销毁敌人
        Destroy(gameObject, destroyDelay);
    }
    /// <summary>
    /// 给玩家添加经验值
    /// </summary>
    void GiveExperienceToPlayer()
    {
        if (expValue <= 0) return;

        // 方法1：使用ExperienceManager（推荐）
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.AddExperience(expValue, expSourceType);
            Debug.Log($"{gameObject.name} 为玩家提供了 {expValue} 点经验值");
        }
        // 方法2：使用LevelManager（备用方案）
        else if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddExperience(expValue);
            Debug.Log($"{gameObject.name} 为玩家提供了 {expValue} 点经验值（通过LevelManager）");
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

    // 治疗敌人
    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHP += healAmount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        Debug.Log($"{gameObject.name} 恢复了 {healAmount} 点血量，当前血量: {currentHP}");
    }

    // 以下为原有代码保持不变，只添加isDead检查
    void FollowPlayer()
    {
        // 如果目标不存在或敌人死亡，不跟随
        if (target == null || isDead) return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // 更新跟随状态
        if (distanceToPlayer < followDistance)
        {
            isFollowingPlayer = true;

            // 只有不在攻击状态时，才移动
            if (!IsAttacking())
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, enemyMoveSpeed * Time.deltaTime);
            }

            // 翻转朝向
            if (transform.position.x - target.position.x > 0)
                transform.eulerAngles = new Vector3(0, 180, 0);
            if (transform.position.x - target.position.x < 0)
                transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            isFollowingPlayer = false;
        }
    }

    void UpdateAnimationState()
    {
        if (animator != null)
        {

            animator.SetBool("isRunning", isFollowingPlayer);
            animator.SetFloat("Speed", isFollowingPlayer ? enemyMoveSpeed : 0f);
        }
    }

    // 检测是否在攻击状态（修改为更准确的检测）
    bool IsAttacking()
    {
        if (animator == null) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Attack") || stateInfo.IsTag("Attack");
    }

    // 检测攻击条件
    void CheckAttack()
    {
        if (target == null || isDead) return;

        // 满足：在攻击距离内 + 不在冷却中 + 不是正在攻击的状态
        if (Vector2.Distance(transform.position, target.position) < attackDistance
            && Time.time >= lastAttackTime + attackCooldown
            && !IsAttacking())
        {
            TriggerAttack();
        }
    }

    // 触发攻击（播放动画 + 施加伤害）
    void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack"); // 触发攻击动画
        }
        lastAttackTime = Time.time; // 记录攻击时间，重置冷却

        // 延迟执行伤害，与攻击动画同步
        StartCoroutine(DealDamageAfterDelay());
    }

    // 延迟攻击伤害
    IEnumerator DealDamageAfterDelay()
    {
        yield return new WaitForSeconds(attackDelay); // 等待攻击动画播放到伤害帧
        DealDamage();
    }

    // 对玩家造成伤害（可以在动画事件中调用，或直接调用）
    void DealDamage()
    {
        if (target == null || isDead) return;

        // 稍微放宽攻击距离检测，确保攻击动作的连贯性
        if (Vector2.Distance(transform.position, target.position) < attackDistance * 1.2f)
        {
            // 获取PlayerAction组件并调用TakeDamage方法
            PlayerAction playerAction = target.GetComponent<PlayerAction>();
            if (playerAction != null)
            {
                playerAction.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} 攻击玩家，造成 {attackDamage} 点伤害");
            }
            else
            {
                Debug.LogWarning("目标没有PlayerAction组件！");
            }
        }
    }

    // 获取当前血量百分比
    public float GetHealthPercentage()
    {
        return currentHP / maxHP;
    }

    // 检查敌人是否存活
    public bool IsAlive()
    {
        return !isDead && currentHP > 0;
    }

    void OnDestroy()
    {
        if (isFollowingPlayer)
        {
            chasingEnemiesCount = Mathf.Max(0, chasingEnemiesCount - 1);
            UpdateBattleMusic();
        }
    }

}