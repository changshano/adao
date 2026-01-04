using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public float playerMoveSpeed;
    public float playerJumpSpeed;
    public bool isGround;
    public Transform foot;
    public LayerMask Ground;
    public Rigidbody2D playerRB;
    public Collider2D playerColl;
    public Animator playerAnim;

    // 添加二段跳相关变量
    private int jumpCount = 0; // 当前跳跃次数
    private int maxJumpCount = 2; // 最大跳跃次数
    private bool canDoubleJump = false; // 是否可以进行二段跳
    private bool isJumping = false; // 是否正在跳跃

    void Start()
    {
        playerColl = GetComponent<Collider2D>();
        playerRB = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
    }

    void Update()
    {
        PlayerMove();
        PlayerJump();
        isGround = Physics2D.OverlapCircle(foot.position, 0.1f, Ground);

        // 在地面时重置跳跃次数
        if (isGround)
        {
            jumpCount = 0;
            canDoubleJump = true; // 重置二段跳能力
            isJumping = false;
            playerAnim.SetBool("jump", false);
        }

        // 设置动画参数
        playerAnim.SetBool("isGrounded", isGround);

        PlayerAttack();

    }

    void PlayerMove()
    {
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
        // 检测跳跃按键按下
        if (Input.GetButtonDown("Jump"))
        {
            // 一段跳：在地面时可以跳跃
            if (isGround)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, playerJumpSpeed);
                jumpCount = 1;
                isJumping = true;
                playerAnim.SetBool("jump", true);
            }
            // 二段跳：在空中且还有跳跃次数且可以进行二段跳
            else if (!isGround && jumpCount < maxJumpCount && canDoubleJump)
            {
                // 重置垂直速度，使二段跳高度一致
                playerRB.velocity = new Vector2(playerRB.velocity.x, 0f);
                playerRB.velocity = new Vector2(playerRB.velocity.x, playerJumpSpeed);
                jumpCount++;
                canDoubleJump = false; // 使用完二段跳
                playerAnim.SetBool("jump", true);
            }
        }
    }

    void PlayerAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerAnim.SetTrigger("attack");
        }
    }
}