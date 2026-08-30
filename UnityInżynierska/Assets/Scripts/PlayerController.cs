using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;

    [Header("Sprint")]
    public float sprintSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckGround();
        Move();
        FlipPlayer();
        Jump();
        UpdateAnimations();
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void Move()
    {
        bool sprintHeld =
            Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        float currentSpeed = sprintHeld ? sprintSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(
            moveInput.x * currentSpeed,
            rb.linearVelocity.y
        );
    }

    private void FlipPlayer()
    {
        if (moveInput.x > 0)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (moveInput.x < 0)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void Jump()
    {
        if (!jumpPressed) return;

        Debug.Log("[PlayerController] Próba skoku. isGrounded = " + isGrounded);

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }

        jumpPressed = false;
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        anim.SetBool("IsGrounded", isGrounded);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        Debug.Log("[PlayerController] OnJump wywołany!");

        if (value.isPressed)
            jumpPressed = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}