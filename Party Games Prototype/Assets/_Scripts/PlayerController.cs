using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRB;
    private BoxCollider2D boxCollider2D;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
    private InputAction move;
    private InputAction duck;

    [Header("Movement")]
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float moveDecceleration;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float velocityPower;
    [SerializeField] private bool test;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] [Range(0,1)] private float fallAcceleration;
    [SerializeField] private float maxFallSpeed;
    private float gravityScale;

    [Header("Dash/Push")]
    [SerializeField] private float dashForce;
    [SerializeField] private float pushCoeff;

    [Space]
    [SerializeField] private SpriteRenderer colorRenderer;
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveVector;
    private bool wantToMove = false;
    private bool isDucking = false;
    private bool doubleJumped = false;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        inputActionAsset = GetComponent<PlayerInput>().actions;
        playerActionMap = inputActionAsset.FindActionMap("Player");
    }

    private void OnEnable()
    {
        move = playerActionMap.FindAction("Movement");
        duck = playerActionMap.FindAction("Duck");
        playerActionMap.FindAction("Jump").performed += Jump;
        playerActionMap.FindAction("Dash/Push").performed += DashPush;
        playerActionMap.Enable();
    }

    private void OnDisable()
    {
        playerActionMap.FindAction("Jump").performed -= Jump;
        playerActionMap.FindAction("Dash/Push").performed -= DashPush;
        playerActionMap.Disable();
    }

    private void Start()
    {
        gravityScale = playerRB.gravityScale;
    }

    private void Update()
    {
        moveVector = move.ReadValue<Vector2>();
        wantToMove = moveVector.sqrMagnitude > 0 ? true : false;

        isDucking = duck.ReadValue<bool>();

        UpdateAnimator();
        UpdateSprite();
    }

    private void FixedUpdate()
    {
        #region Run

        float desiredSpeed = moveVector.x * maxMoveSpeed;

        float speedDif = desiredSpeed - playerRB.velocity.x;

        float accRate = (Mathf.Abs(moveVector.x) > 0.01f) ? moveAcceleration : moveDecceleration;

        float movement = test ? (Mathf.Pow(Mathf.Abs(speedDif) * accRate, velocityPower) * Mathf.Sign(speedDif)) : (speedDif * accRate);

        playerRB.AddForce(Vector2.right * movement, ForceMode2D.Force);

        //if (wantToMove && Mathf.Abs(playerRB.velocity.x) < maxMoveSpeed)
        //    playerRB.AddForce(new Vector2(moveVector.x, 0) * moveAcceleration, ForceMode2D.Force);

        //if (playerRB.velocity.sqrMagnitude > maxMoveSpeed * maxMoveSpeed)
        //    playerRB.velocity = playerRB.velocity.normalized * maxMoveSpeed;

        #endregion

        #region Fall

        if (playerRB.velocity.y < 0)
            playerRB.gravityScale += fallAcceleration;
        else
            playerRB.gravityScale = gravityScale;

        #endregion

        Debug.DrawLine(playerRB.position, playerRB.velocity + playerRB.position, Color.red);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (GroundCheck())
            playerRB.AddForce(Vector2.up * (jumpForce + -playerRB.velocity.y), ForceMode2D.Impulse);

        else if (!doubleJumped)
        {
            playerRB.AddForce(Vector2.up * (jumpForce + -playerRB.velocity.y), ForceMode2D.Impulse);
            doubleJumped = true;
        }
    }

    private void DashPush(InputAction.CallbackContext context)
    {
        playerRB.AddForce(playerRB.velocity * dashForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CompareTag(collision.transform.tag))
        {
            collision.transform.GetComponent<Rigidbody2D>().AddForce(playerRB.velocity * pushCoeff, ForceMode2D.Impulse);
            playerRB.AddForce(-playerRB.velocity, ForceMode2D.Impulse);
        }
    }

    private bool GroundCheck()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, .5f, groundLayer);

        if(raycastHit.collider != null)
            doubleJumped = false;
        
        return raycastHit.collider != null;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(playerRB.velocity.x));
        animator.SetFloat("ySpeed", playerRB.velocity.y);

        if (!GroundCheck())
            animator.SetBool("isGrounded", false);
        else
            animator.SetBool("isGrounded", true);

        if (isDucking)
            Debug.Log("Ducking");
        //    animator.SetBool("isDucking", true);
        //else
        //    animator.SetBool("isDucking", false);
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
}
