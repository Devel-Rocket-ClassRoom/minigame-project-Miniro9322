using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAction : MonoBehaviour, IDamageable
{
    private static readonly int MoveBool = Animator.StringToHash("Move");
    private static readonly int JumpBool = Animator.StringToHash("Jump");
    private static readonly int FallBool = Animator.StringToHash("Fall");
    private static readonly int AttackTriger = Animator.StringToHash("Attack");
    private static readonly int HitTriger = Animator.StringToHash("Hit");
    private static readonly int DodgeTriger = Animator.StringToHash("Dodge");
    private static readonly int ParryTriger = Animator.StringToHash("Parry");

    private static readonly Color32 blinkColor = new(255, 180, 180, 255);

    private DashAfterImage afterImage;
    private Animator animator;
    private Rigidbody2D rb;
    private bool grounded;

    public UnityEvent SuccessParry;

    private enum State
    {
        Idle,
        Move,
        Attack,
        Die,
        Jump,
        Fall,
        Hit,
        Dodge,
        Parry,
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
                    isDodge = false;
                    animator.SetBool(JumpBool, isJump);
                    animator.SetBool(FallBool, isFall);
                    animator.SetBool(MoveBool, isMove);
                    currentstate = value;
                    break;
                case State.Move:
                    isJump = false;
                    isFall = false;
                    isMove = true;
                    animator.SetBool(JumpBool, isJump);
                    animator.SetBool(FallBool, isFall);
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
                    animator.SetBool(JumpBool, isJump);
                    animator.SetBool(MoveBool, isMove);
                    currentstate = value;
                    break;
                case State.Fall:
                    isJump = false;
                    isMove = false;
                    isFall = true;
                    animator.SetBool(JumpBool, isJump);
                    animator.SetBool(FallBool, isFall);
                    animator.SetBool(MoveBool, isMove);
                    currentstate = value;
                    break;
                case State.Hit:
                    animator.SetTrigger(HitTriger);
                    currentstate = value;
                    break;
                case State.Dodge:
                    isDodge = true;
                    animator.SetTrigger(DodgeTriger);
                    currentstate = value;
                    break;
                case State.Parry:
                    parryTime = parryInterval;
                    break;
            }
        }
    }

    [Header("플레이어 이동속도")]
    [SerializeField] private float moveSpeed = 5f;
    [Header("플레이어 점프 힘")]
    [SerializeField] private float jumpPower = 10f;
    [Header("")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform attackZone;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float dodgeAmount = 0f;
    [SerializeField] private float dodgeDuration = 1f;
    [SerializeField] private float parryInterval;
    [SerializeField] private int atk;
    [SerializeField] private float blinkDurationInterval;
    [SerializeField] private float blinkInterval;
    private float dodgeTime = 0f;
    private Vector3 dodgeStart;
    private Vector3 dodgeEnd;
    private Vector2 move;

    private InputAction Jump;
    private InputAction Attack;
    private InputAction Dodge;
    private InputAction Parry;
    private SpriteRenderer sr;
    private bool isJump = false;
    private bool isFall = false;
    private bool isMove = false;
    private float coyoteCounter;
    private float bufferCounter;
    private bool isDodge = false;
    private bool jumpHeld = false;
    private float originalGravityScale;
    private float parryTime = 0f;
    private float blinkTime = 0f;
    private float blinkduration = 0f;
    private Color defaultColor;
    private bool isblinked;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackZone.gameObject.SetActive(false);
        originalGravityScale = rb.gravityScale;
        afterImage = GetComponent<DashAfterImage>();
        defaultColor = GetComponent<SpriteRenderer>().color;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Jump = InputSystem.actions.FindAction("Jump");
        Attack = InputSystem.actions.FindAction("Attack");
        Dodge = InputSystem.actions.FindAction("Dodge");
        Parry = InputSystem.actions.FindAction("Parry");
        Jump.performed += OnJump;
        Jump.canceled += OnJump;
        Attack.performed += OnAttack;
        Dodge.performed += OnDodge;
        Parry.performed += OnParry;
    }

    private void OnDisable()
    {
        Jump.performed -= OnJump;
        Jump.canceled -= OnJump;
        Attack.performed -= OnAttack;
        Dodge.performed -= OnDodge;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if(parryTime > 0f)
        {
            parryTime -= Time.deltaTime;
        }

        if(blinkTime > 0f)
        {
            if(blinkduration > 0f)
            {
                if (!isblinked)
                {
                    sr.color = Color.Lerp(blinkColor, sr.color, Time.deltaTime / blinkduration);
                }
                else
                {
                    sr.color = Color.Lerp(defaultColor, sr.color, Time.deltaTime / blinkduration);
                }
                blinkduration -= Time.deltaTime;
            }
            if(blinkduration <= 0f)
            {
                isblinked = !isblinked;
                blinkduration = blinkDurationInterval;
            }

            blinkTime -= Time.deltaTime;
        }

        if(blinkTime <= 0f)
        {
            sr.color = defaultColor;
        }
    }

    private void FixedUpdate()
    {
        if (isDodge)
        {
            dodgeTime += Time.fixedDeltaTime;
            dodgeEnd = new Vector3(dodgeStart.x + dodgeAmount * transform.localScale.x, dodgeStart.y);
            if (dodgeTime > dodgeDuration)
            {
                transform.position = dodgeEnd;
                rb.gravityScale = originalGravityScale;
                rb.linearVelocity = Vector2.zero;
                dodgeTime = 0f;
                isDodge = false;
                afterImage.StopAfterImage();
            }
            else
            {
                transform.position = Vector3.Lerp(dodgeStart, dodgeEnd, dodgeTime / dodgeDuration);
                return;
            }
        }

        Move(move);

        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if(rb.linearVelocityY < -0.01f && CurrentState != State.Fall)
        {
            CurrentState = State.Fall;
        }

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

        if (!jumpHeld && rb.linearVelocity.y > 0f)
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

        if(CurrentState == State.Jump || CurrentState == State.Fall || CurrentState == State.Parry)
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
            jumpHeld = true;
        }
        if (context.canceled)
        {
            jumpHeld = false;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (CurrentState == State.Attack || CurrentState == State.Dodge)
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
        animator.ResetTrigger(HitTriger);
        animator.ResetTrigger(AttackTriger);
    }

    public IDamageable.DamageInfo SetDamage()
    {
        return new IDamageable.DamageInfo() { canParry = false, damage = atk };
    }

    public void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        if (isDodge || blinkTime > 0f)
        {
            return;
        }

        if(parryTime > 0f && damageInfo.canParry)
        {
            Debug.Log("패링성공");
            SuccessParry?.Invoke();
            return;
        }

        CurrentState = State.Hit;

        blinkTime = blinkInterval;
        blinkduration = blinkDurationInterval;
    }

    private void OnDodge(InputAction.CallbackContext _)
    {
        if (CurrentState == State.Dodge)
            return;

        AttackEnd();
        afterImage.StartAfterImage();
        dodgeStart = transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        CurrentState = State.Dodge;
    }

    private void OnParry(InputAction.CallbackContext _)
    {
        if (CurrentState == State.Parry)
            return;

        CurrentState = State.Parry;
        animator.SetTrigger(ParryTriger);
    }
}
