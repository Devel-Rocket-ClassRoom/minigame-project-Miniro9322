using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAction : MonoBehaviour
{
    private static readonly int MoveBool = Animator.StringToHash("Move");
    private static readonly int JumpTriger = Animator.StringToHash("Jump");
    private static readonly int FallTriger = Animator.StringToHash("Fall");
    private static readonly int AttackTriger = Animator.StringToHash("Attack");
    private Animator animator;
    private Rigidbody2D rb;

    private enum State
    {
        Idle,
        Move,
        Attack,
        Die,
        Jump,
        Fall,
    }

    private State currentstate;

    private State CurrentState
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
                    isJump = false;
                    isFall = false;
                    isMove = false;
                    animator.SetBool(JumpTriger, isJump);
                    animator.SetBool(FallTriger, isFall);
                    animator.SetBool(MoveBool, isMove);
                    currentstate = value;
                    break;
                case State.Move:
                    isJump = false;
                    isFall = false;
                    isMove = true;
                    animator.SetBool(JumpTriger, isJump);
                    animator.SetBool(FallTriger, isFall);
                    animator.SetBool(MoveBool, isMove);
                    currentstate = value;
                    break;
                case State.Attack:
                    animator.SetTrigger(AttackTriger);
                    currentstate = value;
                    break;
                case State.Die:
                    currentstate = value;
                    break;
                case State.Jump:
                    isJump = true;
                    isMove = false;
                    animator.SetBool(JumpTriger, isJump);
                    currentstate = value;
                    break;
                case State.Fall:
                    isJump = false;
                    isMove = false;
                    isFall = true;
                    animator.SetBool(JumpTriger, isJump);
                    animator.SetBool(FallTriger, isFall);
                    currentstate = value;
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
    [SerializeField] private Transform attackZone;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;

    private InputAction Jump;
    private InputAction Attack;
    private bool isJump = false;
    private bool isFall = false;
    private bool isMove = false;
    private float coyoteCounter;
    private float bufferCounter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackZone.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Jump = InputSystem.actions.FindAction("Jump");
        Attack = InputSystem.actions.FindAction("Attack");
        Jump.performed += OnJump;
        Jump.canceled += OnJump;
        Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        Jump.performed -= OnJump;
        Jump.canceled -= OnJump;
        Attack.performed -= OnAttack;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move(move);

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if(rb.linearVelocityY < -0.01f && CurrentState != State.Fall)
        {
            CurrentState = State.Fall;
        }

        Debug.Log(CurrentState);

        switch (CurrentState)
        {
            case State.Fall:
                if (grounded)
                {
                    CurrentState = State.Idle;
                }
                break;
        }

        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;
        bufferCounter -= Time.fixedDeltaTime;

        if (bufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpPower);
            bufferCounter = 0f;
            coyoteCounter = 0f;
            CurrentState = State.Jump;
        }

        if (!isJump && rb.linearVelocity.y > 0f)
            rb.linearVelocity += (lowJumpMultiplier - 1f) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;

        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += (fallMultiplier - 1f) * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
    }

    public void Move(Vector2 move)
    {
        if (move.x < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (move.x > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        transform.position += moveSpeed * Time.fixedDeltaTime * new Vector3(move.x, 0f);

        if(CurrentState == State.Jump || CurrentState == State.Fall)
        {
            return;
        }

        if (move.sqrMagnitude < 0.01)
        {
            CurrentState = State.Idle;
            return;
        }

        CurrentState = State.Move;
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

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (CurrentState == State.Attack)
        {
            return;
        }

        CurrentState = State.Attack;
    }

    public void AttackStart()
    {
        attackZone.gameObject.SetActive(true);
    }

    public void AttackEnd()
    {
        attackZone.gameObject.SetActive(false);
    }

    public void ClearTrigger()
    {
        animator.ResetTrigger(AttackTriger);
    }
}
