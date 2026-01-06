using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float HP;
    public float maxHP;
    public Transform target;
    public float enemyMoveSpeed;
    public float followDistance;

    // 攻击相关参数
    public float attackDistance; // 攻击触发的距离
    public float attackDamage;   // 攻击伤害
    public float attackCooldown; // 攻击冷却时间
    private float lastAttackTime; // 上一次攻击的时间

    private Animator animator;
    private bool isFollowingPlayer = false;

    void Start()
    {
        HP = maxHP;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }
        lastAttackTime = 0; // 初始化冷却时间
    }

    void Update()
    {
        FollowPlayer();
        UpdateAnimationState();
        CheckAttack(); // 检测是否满足攻击条件
    }

    void FollowPlayer()
    {
        bool wasFollowing = isFollowingPlayer;

        if (Vector2.Distance(transform.position, target.position) < followDistance)
        {
            isFollowingPlayer = true;

            // 只有不在攻击状态时，才移动
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
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

    // 检测攻击条件
    void CheckAttack()
    {
        if (target == null) return;

        // 满足：在攻击距离内 + 不在冷却中 + 不是正在攻击的状态
        if (Vector2.Distance(transform.position, target.position) < attackDistance
            && Time.time >= lastAttackTime + attackCooldown
            && !animator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
        {
            TriggerAttack();
        }
    }

    // 触发攻击（播放动画 + 施加伤害）
    void TriggerAttack()
    {
        animator.SetTrigger("Attack"); // 触发攻击动画
        lastAttackTime = Time.time; // 记录攻击时间，重置冷却

        // （可选）攻击动画播放到“伤害帧”时，再调用DealDamage()
        // 这里先简单处理为动画开始时直接伤害，更严谨的做法是用动画事件
        DealDamage();
    }

    // 对玩家造成伤害
    void DealDamage()
    {
        if (Vector2.Distance(transform.position, target.position) < attackDistance)
        {
            // 获取PlayerAction组件并调用TakeDamage方法
            PlayerAction playerAction = target.GetComponent<PlayerAction>();
            if (playerAction != null)
            {
                playerAction.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogWarning("目标没有PlayerAction组件！");
            }
        }
    }
}