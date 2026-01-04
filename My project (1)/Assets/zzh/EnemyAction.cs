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

    // 添加Animator引用
    private Animator animator;
    private bool isFollowingPlayer = false;

    void Start()
    {
        HP = maxHP;
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // 获取Animator组件
        animator = GetComponent<Animator>();

        // 如果没有Animator组件，给出警告
        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }
    }

    void Update()
    {
        FollowPlayer();
        UpdateAnimationState();
    }

    void FollowPlayer()
    {
        bool wasFollowing = isFollowingPlayer;

        // 检查是否在跟随距离内
        if (Vector2.Distance(transform.position, target.position) < followDistance)
        {
            isFollowingPlayer = true;

            // 移动敌人
            transform.position = Vector2.MoveTowards(transform.position, target.position, enemyMoveSpeed * Time.deltaTime);

            // 翻转敌人朝向
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
        // 如果有Animator组件，更新动画参数
        if (animator != null)
        {
            // 设置奔跑状态参数
            animator.SetBool("isRunning", isFollowingPlayer);

            // 可选：根据移动速度设置速度参数
            animator.SetFloat("Speed", isFollowingPlayer ? enemyMoveSpeed : 0f);
        }
    }
}