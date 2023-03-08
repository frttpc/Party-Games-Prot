using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player player = Player.None;
    public Rigidbody2D playerRB { get; private set; }
    public SpriteRenderer spriteRenderer;
    public TrailRenderer trailRenderer { get; private set; }
    public LayerMask enemyLayer;
    public Animator animator { get; private set; }

    private BoxCollider2D boxCollider2D;
    private CircleCollider2D circleCollider2D;
    private PlayerInputController playerInputController;
    private Attack attack;

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
    private bool canDash = false;
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
    public float gravityScale { get; private set; }
    private bool isDucking = false;
    private bool isGrounded = false;
    public bool isFacingRight = true;

    private bool dashed = false;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        playerInputController = GetComponent<PlayerInputController>();
        attack = GetComponent<Attack>();
    }

    private void Start()
    {
        gravityScale = playerRB.gravityScale;
        dashCooldownCounter = 0;
    }

    private void Update()
    {
        isGrounded = GroundCheck();

        if (playerRB.velocity.y <= 0)
            isWalled = WallCheck();
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

        if (playerInputController.dashIsPressed && canDash && !isDucking)
        {
            isDashing = true;

            dashTimeCounter = dashTime;
            dashCooldownCounter = 0;
        }

        if (dashTimeCounter < 0 && isDashing)
        {
            isDashing = false;
            playerRB.gravityScale = gravityScale;
            dashTimeCounter = dashTime;
        }
        else
        {
            dashTimeCounter -= Time.deltaTime;
        }

        if (dashCooldownCounter >= dashCooldown)
        {
            canDash = true;
            dashCooldownCounter = dashCooldown;
        }
        else
        {
            dashCooldownCounter += Time.deltaTime;
        }

        #endregion

        moveVector = playerInputController.move.ReadValue<Vector2>();

        isDucking = playerInputController.duck.ReadValue<float>() > 0.5f && isGrounded;

        UpdateAnimator();
        Flip();
        UpdateDashBar();

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

        if (isDashing && !isDucking)
        {
            Dash();
            dashed = true;
        }

        #endregion

        #region Duck

        if (isDucking)
        {
            playerRB.simulated = false;
            playerRB.gravityScale = 0;
        }
        else
        {
            playerRB.simulated = true;
            playerRB.gravityScale = gravityScale;
        }

        #endregion

        playerInputController.jumpIsPressed = false;
        playerInputController.dashIsPressed = false;

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
        canDash = false;

        playerRB.gravityScale = 0;

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        playerRB.AddForce(dir * dashForce - playerRB.velocity, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController enemyController = collision.transform.GetComponent<PlayerController>();

        if (enemyController)
        { 
            Vector2 normal = collision.GetContact(0).normal;

            Push(enemyController, normal);
        }
    }

    private void Push(PlayerController enemyController, Vector2 normal)
    {
        float angle = Mathf.Clamp(Vector2.Angle(-normal, velocityBeforePhysicsUpdate), 1, 90);
        float angleMag = Mathf.Lerp(1, 0, angle / 90);
        float magnitude = Mathf.Clamp(velocityBeforePhysicsUpdate.magnitude, minPushAmount, maxPushAmount);

        enemyController.playerRB.AddForce(angleMag * magnitude * pushMultiplier * -normal, ForceMode2D.Impulse);
    }

    private bool GroundCheck()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, .3f, groundLayer);

        return raycastHit.collider != null;
    }

    private bool WallCheck()
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(circleCollider2D.bounds.center, new(moveVector.x, 0), circleCollider2D.radius + 0.3f , groundLayer);

        return raycastHit.collider != null;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(playerRB.velocity.x));
        animator.SetFloat("ySpeed", playerRB.velocity.y);

        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isDucking", isDucking);
    }

    public void Flip()
    {
        if (!isDashing)
        {
            if (moveVector.x < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                isFacingRight = false;
            }
            else if (moveVector.x > 0)
            {
                transform.rotation = Quaternion.identity;
                isFacingRight = true;
            }
        }
    }

    private void UpdateDashBar() => UIManager.Instance.UpdateDashBar(player, dashCooldownCounter / dashCooldown);

    public void SetColor(Color color) => colorRenderer.color = color;

    public SpriteRenderer GetColorRenderer() => colorRenderer;

}