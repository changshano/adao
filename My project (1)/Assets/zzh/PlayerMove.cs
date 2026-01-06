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
        if (isDead) return;

        if (Input.GetButtonDown("Fire1"))
        {
            playerAnim.SetTrigger("attack");
        }
    }

    // 受到伤害
    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        // 冷却时间检查
        if (Time.time - lastAttackTime < attackCooldown) return;

        currentHealth -= damage;
        lastAttackTime = Time.time;

        playerAnim.SetTrigger("hurt");

        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

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
        if (isDead) return;

        isDead = true;
        playerAnim.SetTrigger(deathTriggerName);

        playerRB.velocity = Vector2.zero;
        playerRB.isKinematic = true;

        if (playerColl != null)
            playerColl.enabled = false;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Debug.Log("阿岛已经阵亡！");
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