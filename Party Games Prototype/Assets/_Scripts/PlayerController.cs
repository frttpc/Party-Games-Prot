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

    private InputActionAsset playerInputActions;
    private InputActionMap playerActionMap;
    private InputAction move;
    private InputAction duck;

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
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    private bool jumpIsPressed = false;
    private bool doubleJumped = false;

    [Header("Dash")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashTime;
    private float dashTimeCounter;
    [SerializeField] private float dashCooldown;
    private float dashCooldownCounter;
    private bool dashIsPressed = false;
    private bool canDash = true;
    private bool isDashing = false;

    [Header("Wall Jump")]
    [SerializeField] [Range(1, 20)] private float wallFriction = 10;
    private bool isWalled = false;

    [Header("Push")]
    [SerializeField] private float pushCoeff;

    [SerializeField] private float some;

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

        playerInputActions = GetComponent<PlayerInput>().actions;
        playerActionMap = playerInputActions.FindActionMap("Player");
    }

    private void OnEnable()
    {
        playerActionMap.Enable();
        move = playerActionMap.FindAction("Movement");
        duck = playerActionMap.FindAction("Duck");
        playerActionMap.FindAction("Jump").performed += JumpIsPressed;
        playerActionMap.FindAction("Dash/Push").performed += DashIsPressed;
    }

    private void OnDisable()
    {
        playerActionMap.Disable();
        playerActionMap.FindAction("Jump").performed -= JumpIsPressed;
        playerActionMap.FindAction("Dash/Push").performed -= DashIsPressed;
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

        if (jumpIsPressed)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (dashIsPressed && canDash)
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

        moveVector = move.ReadValue<Vector2>();

        isDucking = duck.ReadValue<float>() > 0.1f && isGrounded;

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

        if (jumpIsPressed)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                if (isWalled)
                    Jump(new Vector2(-moveVector.x, 1) * some);
                else
                    Jump(Vector2.up);
                coyoteTimeCounter = 0;
            }
            else if (!doubleJumped && !isGrounded)
            {
                Jump(Vector2.up);
                doubleJumped = true;
            }
        }
        
        jumpIsPressed = false;

        #endregion

        #region Fall

        if (isWalled && Mathf.Abs(moveVector.x) > 0)
            playerRB.velocity = new Vector2(moveVector.x, 0);
            //playerRB.AddForce(-playerRB.velocity.y * wallFriction * Vector2.up, ForceMode2D.Force);

        else if (Mathf.Abs(playerRB.velocity.y) > maxFallSpeed)
            playerRB.AddForce(-playerRB.velocity.y * Vector2.up, ForceMode2D.Force);

        #endregion

        #region Dash

        if (dashIsPressed && canDash)
        {
            Dash();
        }

        #endregion
    }

    private void JumpIsPressed(InputAction.CallbackContext context)
    {
        jumpIsPressed = context.performed;
    }

    private void Jump(Vector2 dir)
    {
        playerRB.AddForce(dir * (jumpForce + -playerRB.velocity.y), ForceMode2D.Impulse);

        jumpBufferCounter = 0;
    }

    private void DashIsPressed(InputAction.CallbackContext context)
    {
        dashIsPressed = context.performed;
    }

    private void Dash()
    {
        isDashing = true;
        canDash = false;

        playerRB.gravityScale = 0;
        Vector2 dir = (playerRB.velocity * 1000).normalized;
        playerRB.AddForce(dir * dashForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CompareTag(collision.transform.tag))
        {
            if (isDashing)
                playerRB.velocity = Vector2.zero;
            else
                playerRB.AddForce(-playerRB.velocity, ForceMode2D.Impulse);

            collision.transform.GetComponent<Rigidbody2D>().AddForce(playerRB.velocity * pushCoeff, ForceMode2D.Impulse);
        }
    }

    private bool GroundCheck()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, .3f, groundLayer);

        return raycastHit.collider != null;
    }

    private bool WallCheck()
    {
        Vector2 dir;

        if (moveVector.x < 0)
            dir = Vector2.left;
        else
            dir = Vector2.right;

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
        else if(moveVector.x > 0)
            spriteRenderer.flipX = false;
    }

    public void SetColor(Color color)
    {
        colorRenderer.color = color;
    }

    public SpriteRenderer GetColorRenderer()
    {
        return colorRenderer;
    }
}
