using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAction : MonoBehaviour
{
    [Header("移动设置")]
    public float playerMoveSpeed;
    public float playerJumpSpeed;
    public bool isGround;
    public Transform foot;
    public LayerMask Ground;
    public Rigidbody2D playerRB;
    public Collider2D playerColl;
    public Animator playerAnim;

    [Header("血量设置")]
    public float maxHealth = 100f;
    public float currentHealth = 85f;
    public float invincibleTime = 1f;
    private float invincibleTimer = 0f;
    private bool isInvincible = false;
    private bool isDead = false;

    [Header("UI引用")]
    public Image healthBarImage;

    [Header("怪物攻击设置")]
    public float enemyDamage = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;


    [Header("基础属性")]
    [SerializeField] private float baseAttackDamage = 10f;  // 基础攻击力
    private float equipmentAttackBonus = 0f;  // 装备攻击力加成
    [Header("攻击设置")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;
    public float attackDelay = 0.3f;
    private bool isAttacking = false;
    // 计算总攻击力
    public float AttackDamage
    {
        get { return baseAttackDamage + equipmentAttackBonus; }
    }

    [Header("二段跳设置")]
    private int jumpCount = 0;
    private int maxJumpCount = 2;
    private bool canDoubleJump = false;
    private bool isJumping = false;

    [Header("死亡设置")]
    public GameObject deathEffect;
    public string deathTriggerName = "Die";

    [Header("测试设置")]
    public KeyCode testDamageKey = KeyCode.Space;

    void Start()
    {
        playerColl = GetComponent<Collider2D>();
        playerRB = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();

        // 初始化血量
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 初始化血条UI
        if (healthBarImage == null)
        {
            FindHealthBar();
        }
        else
        {
            // 确保图片类型设置正确
            healthBarImage.type = Image.Type.Filled;
            healthBarImage.fillMethod = Image.FillMethod.Horizontal;
            healthBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        UpdateHealthBar();
        Debug.Log($"阿岛血量初始化: {currentHealth}/{maxHealth}");
    }

    // 应用攻击力加成
    public void ApplyAttackBonus(float bonus)
    {
        equipmentAttackBonus += bonus;
        Debug.Log($"攻击力加成: {bonus}, 当前总攻击力: {AttackDamage}");
    }

    // 移除攻击力加成
    public void RemoveAttackBonus(float reduction)
    {
        equipmentAttackBonus -= reduction;
        if (equipmentAttackBonus < 0) equipmentAttackBonus = 0;
        Debug.Log($"移除攻击力加成: {reduction}, 当前总攻击力: {AttackDamage}");
    }

    // 获取基础攻击力
    public float GetBaseAttackDamage()
    {
        return baseAttackDamage;
    }

    // 获取总攻击力
    public float GetTotalAttackDamage()
    {
        return AttackDamage;
    }

    void Update()
    {
        if (isDead) return;

        // 无敌时间倒计时
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }

        PlayerMove();
        PlayerJump();
        isGround = Physics2D.OverlapCircle(foot.position, 0.1f, Ground);

        // 在地面时重置跳跃次数
        if (isGround)
        {
            jumpCount = 0;
            canDoubleJump = true;
            isJumping = false;
            playerAnim.SetBool("jump", false);
        }

        // 设置动画参数
        playerAnim.SetBool("isGrounded", isGround);

        PlayerAttack();

        // 测试用按键
        if (Input.GetKeyDown(testDamageKey))
        {
            TakeDamage(enemyDamage);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10f);
        }
    }

    void PlayerMove()
    {
        if (isDead) return;

        float horizontalNum = Input.GetAxis("Horizontal");
        float faceNum = Input.GetAxisRaw("Horizontal");
        playerRB.velocity = new Vector2(playerMoveSpeed * horizontalNum, playerRB.velocity.y);
        playerAnim.SetFloat("run", Mathf.Abs(playerMoveSpeed * horizontalNum));
        if (faceNum != 0)
        {
            transform.localScale = new Vector3(faceNum, transform.localScale.y, transform.localScale.z);
        }
    }

    void PlayerJump()
    {
        if (isDead) return;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGround)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, playerJumpSpeed);
                jumpCount = 1;
                isJumping = true;
                playerAnim.SetBool("jump", true);
            }
            else if (!isGround && jumpCount < maxJumpCount && canDoubleJump)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, 0f);
                playerRB.velocity = new Vector2(playerRB.velocity.x, playerJumpSpeed);
                jumpCount++;
                canDoubleJump = false;
                playerAnim.SetBool("jump", true);
            }
        }
    }

    void PlayerAttack()
    {
        if (isDead || isAttacking) return;

        if (Input.GetButtonDown("Fire1"))
        {
            // 播放攻击动画
            playerAnim.SetTrigger("attack");
            isAttacking = true;

            // 延迟检测并攻击敌人，与攻击动画同步
            StartCoroutine(AttackAfterDelay());
        }
    }

    IEnumerator AttackAfterDelay()
    {
        yield return new WaitForSeconds(attackDelay);

        // 检测并攻击敌人
        AttackEnemies();

        // 重置攻击状态
        isAttacking = false;
    }

    void AttackEnemies()
    {
        Vector2 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

        Debug.Log($"检测到 {hitEnemies.Length} 个敌人");

        bool hitAnyEnemy = false;

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 先尝试检测普通小怪
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                enemy.TakeDamage(AttackDamage);
                hitAnyEnemy = true;
                Debug.Log($"攻击小怪: {enemy.name}, 造成 {AttackDamage} 伤害");
                continue; // 处理完就继续下一个
            }

            // 再尝试检测Boss
            BossController boss = enemyCollider.GetComponent<BossController>();
            if (boss != null && !boss.isDead)
            {
                boss.TakeDamage(AttackDamage);
                hitAnyEnemy = true;
                Debug.Log($"攻击Boss: {boss.name}, 造成 {AttackDamage} 伤害");
            }
        }

        if (!hitAnyEnemy)
        {
            Debug.Log("攻击未命中任何敌人");
        }
    }

    // 在场景中显示攻击范围（调试用）
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        // 如果死亡或无敌，不扣血
        if (isDead || isInvincible)
        {
            Debug.Log("玩家处于死亡或无敌状态，不扣血");
            return;
        }

        // 扣血
        currentHealth -= damage;

        // 触发受伤动画
        playerAnim.SetTrigger("hurt");

        // 进入无敌状态
        isInvincible = true;
        invincibleTimer = invincibleTime;

        // 确保血量不会低于0
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("当前血量小于等于0，调用Die()方法");
            Die();  // 调用死亡方法
        }

        // 更新血条显示
        UpdateHealthBar();

        Debug.Log($"阿岛受到 {damage} 点伤害，剩余血量: {currentHealth}");
    }

    // 自动被怪物攻击的方法
    public void GetAttackedByEnemy()
    {
        if (Time.time - lastAttackTime > attackCooldown)
        {
            TakeDamage(enemyDamage);
            lastAttackTime = Time.time;
        }
    }

    void Die()
    {
        Debug.Log("进入Die()方法");
        if (isDead)
        {
            Debug.Log("玩家已经死亡，防止重复死亡");
            return;  // 防止重复死亡
        }

        isDead = true;

        // 1. 播放死亡动画
        playerAnim.SetTrigger(deathTriggerName);
        Debug.Log($"触发死亡动画触发器: {deathTriggerName}");

        // 2. 停止移动
        playerRB.velocity = Vector2.zero;
        playerRB.isKinematic = true;  // 变成运动学刚体，不再受物理影响

        // 3. 禁用碰撞体（可选，防止尸体还和物体碰撞）
        if (playerColl != null)
            playerColl.enabled = false;

        // 4. 播放死亡特效（如果有）
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // 5. 停止所有输入
        // 通过设置isDead=true，Update中的操作已经会被阻止

        Debug.Log("阿岛已经阵亡！");

        // 6. 延迟销毁
        StartCoroutine(DestroyAfterDeath());
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthBar();
        Debug.Log($"阿岛恢复了 {healAmount} 点血量，当前血量: {currentHealth}");
    }

    void UpdateHealthBar()
    {
        if (healthBarImage == null)
        {
            Debug.LogWarning("血条UI为空，尝试查找...");
            FindHealthBar();
            if (healthBarImage == null) return;
        }

        float healthPercent = currentHealth / maxHealth;
        healthBarImage.fillAmount = healthPercent;

        // 血量颜色变化
        if (healthPercent < 0.3f)
        {
            healthBarImage.color = Color.red;
        }
        else if (healthPercent < 0.6f)
        {
            healthBarImage.color = Color.yellow;
        }
        else
        {
            healthBarImage.color = Color.green;
        }
    }

    void FindHealthBar()
    {
        GameObject hpMask = GameObject.Find("Image_HPbar/Image_Mask");
        if (hpMask != null)
        {
            healthBarImage = hpMask.GetComponent<Image>();
            if (healthBarImage != null)
            {
                healthBarImage.type = Image.Type.Filled;
                healthBarImage.fillMethod = Image.FillMethod.Horizontal;
                healthBarImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                Debug.Log("成功找到并设置血条UI");
            }
        }
        else
        {
            Debug.LogError("没有找到血条UI对象！请检查对象路径是否为'Image_HPbar/Image_Mask'");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            GetAttackedByEnemy();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyAttack"))
        {
            GetAttackedByEnemy();
        }
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}