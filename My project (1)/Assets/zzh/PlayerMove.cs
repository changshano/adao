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
        if (Input.GetButton("Jump"))
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerJumpSpeed);
            playerAnim.SetBool("jump", true);
        }
        if (isGround)
        {
            playerAnim.SetBool("jump", false);
        }
    }
}