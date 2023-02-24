using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider2D;
    private CircleCollider2D circleCollider2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerInputController playerInputController;

    [SerializeField] private SpriteRenderer colorRenderer;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement")]
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float moveDecceleration;
    [SerializeField] private float maxMoveSpeed;
    private Vector2 moveVector;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] [Range(0, 0.5f)] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [SerializeField] [Range(0, 0.5f)] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    private bool doubleJumped = false;

    [Header("Dash")]
    [SerializeField] private float dashForce;
    [SerializeField] [Range(0,1)] private float dashTime;
    private float dashTimeCounter;
    [SerializeField] private float dashCooldown;
    private float dashCooldownCounter;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Wall Jump")]
    [SerializeField] [Range(10, 20)] private float wallFriction = 15;
    [SerializeField] [Range(1, 5)] private float wallJumpMultipler;
    private bool isWalled = false;

    [Header("Push")]
    [SerializeField] private float maxPushAmount;
    [SerializeField] private float minPushAmount;
    [SerializeField] [Range(1,5)] private float pushMultiplier;

    private Vector2 velocityBeforePhysicsUpdate;
    private float gravityScale;
    private bool isDucking = false;
    private bool isGrounded = false;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        playerInputController = GetComponent<PlayerInputController>();
    }

    private void Start()
    {
        gravityScale = playerRB.gravityScale;
    }

    private void Update()
    {
        isGrounded = GroundCheck();

        if (playerRB.velocity.y <= 0)
        {
            isWalled = WallCheck();
        }
        else
            isWalled = false;

        if (isGrounded || isWalled)
        {
            coyoteTimeCounter = coyoteTime;
            doubleJumped = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (playerInputController.jumpIsPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        #region Dash

        if (playerInputController.dashIsPressed && canDash)
        {
            dashTimeCounter = dashTime;
            dashCooldownCounter = dashCooldown;
        }

        if (dashTimeCounter < 0)
        {
            isDashing = false;
            playerRB.gravityScale = gravityScale;
            dashTimeCounter = dashTime;
        }

        if(dashCooldownCounter < 0)
        {
            canDash = true;
        }

        dashTimeCounter -= Time.deltaTime;
        dashCooldownCounter -= Time.deltaTime;

        #endregion

        moveVector = playerInputController.move.ReadValue<Vector2>();

        isDucking = playerInputController.duck.ReadValue<float>() > 0.5f && isGrounded;

        UpdateAnimator();
        UpdateSprite();
    }

    private void FixedUpdate()
    {
        #region Move

        if (!isDashing)
        {
            if (isDucking && isGrounded)
                moveVector.x = 0;

            float desiredSpeed = moveVector.x * maxMoveSpeed;

            float speedDif = desiredSpeed - playerRB.velocity.x;

            float accRate = (Mathf.Abs(moveVector.x) > 0.01f) ? moveAcceleration : moveDecceleration;

            playerRB.AddForce(accRate * speedDif * Vector2.right, ForceMode2D.Force);
        }

        #endregion
        
        #region Jump
        
        if (playerInputController.jumpIsPressed && !isDashing)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                if (isWalled)
                    WallJump();
                else
                    Jump();
                coyoteTimeCounter = 0;
            }
            else if (!doubleJumped && !isGrounded)
            {
                Jump();
                doubleJumped = true;
            }
        }

        #endregion

        #region Fall

        if (!isDashing)
        {
            if (isWalled && Mathf.Abs(moveVector.x) > 0)
            {
                playerRB.AddForce(-playerRB.velocity.y * wallFriction * Vector2.up, ForceMode2D.Force);
            }
            else if (Mathf.Abs(playerRB.velocity.y) > maxFallSpeed)
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, maxFallSpeed * Mathf.Sign(playerRB.velocity.y));
                playerRB.AddForce((-playerRB.velocity.y + 90f) * Vector2.up, ForceMode2D.Force);
            }
        }

        #endregion

        #region Dash

        if (playerInputController.dashIsPressed && canDash)
        {
            Dash();
        }

        #endregion

        playerInputController.dashIsPressed = false;
        playerInputController.jumpIsPressed = false;
        
        velocityBeforePhysicsUpdate = playerRB.velocity;

    }

    private void Jump()
    {
        playerRB.AddForce(Vector2.up * (jumpForce + -playerRB.velocity.y), ForceMode2D.Impulse);

        jumpBufferCounter = 0;
    }

    private void WallJump()
    {
        playerRB.AddForce(jumpForce * wallJumpMultipler * new Vector2(-moveVector.x, 1), ForceMode2D.Impulse);
    }

    private void Dash()
    {
        isDashing = true;
        canDash = false;

        playerRB.gravityScale = 0;
        Vector2 dir = (playerRB.velocity * 100).normalized;
        playerRB.AddForce(dir * dashForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<PlayerController>())
        { 
            Vector2 normal = collision.GetContact(0).normal;

            float angle = Mathf.Clamp(Vector2.Angle(-normal, velocityBeforePhysicsUpdate), 1, 90);

            float angleMag = Mathf.Lerp(1, 0, angle / 90);

            float magnitude = Mathf.Clamp(velocityBeforePhysicsUpdate.magnitude, minPushAmount, maxPushAmount);

            collision.transform.GetComponent<Rigidbody2D>().AddForce(angleMag * magnitude * pushMultiplier * -normal, ForceMode2D.Impulse);
        }
    }

    private bool GroundCheck()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, .3f, groundLayer);

        return raycastHit.collider != null;
    }

    private bool WallCheck()
    {
        Vector2 dir = new Vector2(moveVector.x, 0);

        RaycastHit2D raycastHit = Physics2D.Raycast(circleCollider2D.bounds.center, dir, circleCollider2D.radius + 0.3f , groundLayer);

        return raycastHit.collider != null;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(playerRB.velocity.x));
        animator.SetFloat("ySpeed", playerRB.velocity.y);

        if (isGrounded)
            animator.SetBool("isGrounded", true);
        else
            animator.SetBool("isGrounded", false);

        if (isDucking)
            animator.SetBool("isDucking", true);
        else
            animator.SetBool("isDucking", false);
    }

    private void UpdateSprite()
    {
        if (moveVector.x < 0)
            spriteRenderer.flipX = true;
        else if (moveVector.x > 0)
            spriteRenderer.flipX = false;
    }

    public void SetColor(Color color) => colorRenderer.color = color;

    public SpriteRenderer GetColorRenderer() => colorRenderer;
}
