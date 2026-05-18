using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    private static readonly int MoveBool = Animator.StringToHash("Move");
    private Animator animator;
    private Rigidbody2D rb;

    public enum State
    {
        Idle,
        Move,
        Attack,
        Die,
    }

    private State currentstate = State.Idle;

    public State CurrentState
    {
        get
        {
            return currentstate;
        }

        set
        {
            switch (value)
            {
                case State.Idle:
                    animator.SetBool(MoveBool, false);
                    break;
                case State.Move:
                    animator.SetBool(MoveBool, true);
                    break;
                case State.Attack:
                    break;
                case State.Die:
                    break;
            }
        }
    }

    [Header("플레이어 이동속도")]
    [SerializeField] private float moveSpeed = 5f;
    [Header("플레이어 점프 힘")]
    [SerializeField] private float jumpPower = 10f;
    private Vector2 move;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;

    private InputAction Jump;
    private bool isJump = false;
    private float coyoteCounter;
    private float bufferCounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Jump = InputSystem.actions.FindAction("Jump");
        Jump.performed += OnJump;
        Jump.canceled += OnJump;
    }

    private void OnDisable()
    {
        Jump.performed -= OnJump;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Move(move);
    }

    private void FixedUpdate()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;
        bufferCounter -= Time.fixedDeltaTime;

        if (bufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            bufferCounter = 0f;
            coyoteCounter = 0f;
        }

        if (!isJump && rb.linearVelocity.y > 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
    }

    public void Move(Vector2 move)
    {
        if(move.sqrMagnitude < 0.01)
        {
            CurrentState = State.Idle;
            return;
        }

        if(move.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if(move.x  > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        CurrentState = State.Move;
        transform.position += new Vector3(move.x, 0f) * moveSpeed * Time.deltaTime;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            bufferCounter = jumpBufferTime;
            isJump = true;
        }
        if (context.canceled)
        {
            isJump = false;
        }
    }
}
