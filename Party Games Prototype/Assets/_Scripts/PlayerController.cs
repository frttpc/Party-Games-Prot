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

    [SerializeField] private LayerMask groundLayerMask;

    [Header("Movement")]
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float dashForce;
    [SerializeField] private float pushCoeff;

    [Space]
    [SerializeField] private SpriteRenderer colorRenderer;

    private Vector2 moveVector;
    private bool isMoving = false;
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

    private void Update()
    {
        moveVector = move.ReadValue<Vector2>();
        isMoving = moveVector.sqrMagnitude > 0 ? true : false;

        isDucking = duck.ReadValue<bool>();

        UpdateAnimator();
        UpdateSprite();
    }

    private void FixedUpdate()
    {
        if (isMoving && Mathf.Abs(playerRB.velocity.x) < maxMoveSpeed)
            playerRB.AddForce(new Vector2(moveVector.x, 0) * moveAcceleration, ForceMode2D.Force);

        if(playerRB.velocity.sqrMagnitude > maxMoveSpeed * maxMoveSpeed)
            playerRB.velocity = playerRB.velocity.normalized * maxMoveSpeed;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (GroundCheck())
            playerRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        //else if (!doubleJumped)
        //{
        //    playerRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        //    doubleJumped = true;
        //}
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
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, .5f, groundLayerMask);

        //if(raycastHit.collider.CompareTag())
        //ResetDoubleJump();

        return raycastHit.collider != null;
    }

    private void ResetDoubleJump()
    {
        doubleJumped = false;
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
