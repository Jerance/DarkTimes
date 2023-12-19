using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_speed; // 5
    [SerializeField] private float m_jumpForce; // 8
    [SerializeField] private float m_attackRange; // 1.3
    [SerializeField] private float m_playerHeight; // 0.58
    [SerializeField] private Rigidbody2D m_rigidbody;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private LayerMask m_enemyLayer;
    [SerializeField] private Animator m_animSpear;
    [SerializeField] private Animator m_animSword;
    [SerializeField] private bool m_doubleJump;
    [SerializeField] private bool m_facingLeft;
    [SerializeField] private float m_attackStrengh;

    private bool isGrounded = false;
    private bool jump = false;
    private bool jumpedTwice = false;
    private bool attacking = false;
    private bool spearMode = false;
    private bool sprint = false;

    private SpriteRenderer swordRenderer;
    private SpriteRenderer spearRenderer;

    private void Start()
    {
        swordRenderer = m_animSword.GetComponent<SpriteRenderer>();
        spearRenderer = m_animSpear.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateWeaponMode();

        var anim = spearMode ? m_animSpear : m_animSword;
        var horizontalInput = Input.GetAxis("Horizontal");
        var attackMode = "Attack";
        //var sprintMode = "Move";
        var groundMode = "Grounded";
        //var jumpMode = "Jump";
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, m_playerHeight, m_groundLayer);
        anim.SetBool(groundMode, Physics2D.Raycast(transform.position, Vector2.down, m_playerHeight * 1.6f, m_groundLayer));
        attacking = stateInfo.IsName(attackMode);

        if (!attacking)
        {
            if (isGrounded)
            {
                HandleGroundedInput(anim);

                if (Input.GetKeyDown(KeyCode.L))
                {
                    spearMode = !spearMode;
                }

                sprint = Input.GetKey(KeyCode.LeftShift);
            }

            HandleJumpInput(anim);
            HandleMovement(horizontalInput);
        }
       
    }

    private void FixedUpdate()
    {
        if (jump)
        {
            var force = jumpedTwice ? m_jumpForce * 1.1f : m_jumpForce;
            jump = false;
            m_rigidbody.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        }
    }

    private void UpdateWeaponMode()
    {
        if (spearMode)
        {
            swordRenderer.enabled = false;
            spearRenderer.enabled = true;
        }
        else
        {
            swordRenderer.enabled = true;
            spearRenderer.enabled = false;
        }
    }

    private void HandleGroundedInput(Animator anim)
    {
        var sprintMode = "Move";
        var jumpMode = "Jump";

        anim.SetBool(sprintMode, Mathf.Abs(m_rigidbody.velocity.x) > 1);

        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.SetTrigger("Attack");
            Invoke(nameof(HandleAttacking), 0.5f);
            Invoke(nameof(HandleAttacking), 1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
            jumpedTwice = false;
            anim.SetTrigger(jumpMode);
        }
        else if (m_doubleJump && !jumpedTwice && Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
            anim.SetTrigger(jumpMode);
            jumpedTwice = true;
        }
    }

    private void HandleMovement(float horizontalInput)
    {
        var speedMultiplier = sprint ? 1.7f : 1;
        var speed = m_speed * speedMultiplier;

        m_rigidbody.velocity = new Vector2(horizontalInput * speed, m_rigidbody.velocity.y);

        if (horizontalInput > 0.01f)
        {
            m_facingLeft = false;
            transform.localScale = Vector3.one;
        }
        else if (horizontalInput < -0.01f)
        {
            m_facingLeft = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void HandleJumpInput(Animator anim)
    {
        var jumpMode = "Jump";

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                jump = true;
                jumpedTwice = false;
                anim.SetTrigger(jumpMode);
            }
            else if (m_doubleJump && !jumpedTwice)
            {
                jump = true;
                anim.SetTrigger(jumpMode);
                jumpedTwice = true;
            }
        }
    }

    private void HandleAttacking()
    {
        var rayDirection = m_facingLeft ? Vector2.left : Vector2.right;
        var attackRangeMultiplier = spearMode ? 1.6f : 1;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, rayDirection, m_attackRange * attackRangeMultiplier, m_enemyLayer);

        foreach(var hit in hits)
        {
            if (hit.collider != null)
            {

                var controller = hit.collider.GetComponent<PunchingBallController>();

                controller.takeHit(rayDirection.x, m_attackStrengh);
            }

        }
       
    }
}
