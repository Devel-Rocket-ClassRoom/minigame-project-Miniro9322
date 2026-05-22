using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
    private static readonly int MoveBool = Animator.StringToHash("Move");

    [SerializeField] private Transform groundCheck;
    [SerializeField] private AttackZone attackZone;
    [SerializeField] private LayerMask groundLayer;

    public FSM Fsm { get; private set; }
    public DashAfterImage AfterImage { get; private set; }
    public Animator Animator { get; private set; }
    public Queue<string> CommandQueue { get; private set; } = new();
    public IState AttackState { get; private set; }
    public IState DeathState { get; private set; }
    public IState DodgeState { get; private set; }
    public IState FallState { get; private set; }
    public IState IdleState { get; private set; }
    public IState JumpState { get; private set; }
    public IState ParryState { get; private set; }
    public IState HitState { get; private set; }
    public float OriginalGravityScale { get; private set; }
    public bool JumpHeld { get; private set; } = false;
    public bool Grounded { get; private set; }
    public bool IsAttackEnd { get; private set; } = false;

    public UnityEvent SuccessParry;
    public UnityEvent OnGameOver;
    public UnityEvent OnHit;
    public UnityEvent ParryStart;
    public PlayerData Data;
    public Rigidbody2D Rb => rb;
    public SpriteRenderer Sr => sr;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private InputAction Jump;
    private InputAction Attack;
    private InputAction Dodge;
    private InputAction Parry;

    private Vector2 move;
    private int currHp;
    private int jumpCount = 0;
    private bool Invincible = false;
    private bool parrying = false;
    private bool isQueueOpen = false;
    private float jumpBufferCounter = 0f;
    private float coyoteCounter = 0f;
    private float dodgeInterval = 1f;
    private float dodgeCool = 0f;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackZone.gameObject.SetActive(false);
        OriginalGravityScale = rb.gravityScale;
        AfterImage = GetComponent<DashAfterImage>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Jump = InputSystem.actions.FindAction("Jump");
        Attack = InputSystem.actions.FindAction("Attack");
        Dodge = InputSystem.actions.FindAction("Dodge");
        Parry = InputSystem.actions.FindAction("Parry");

        AttackState = new AttackState(this);
        DeathState = new DeathState(this);
        DodgeState = new DodgeState(this);
        FallState = new FallState(this);
        IdleState = new IdleState(this);
        JumpState = new JumpState(this);
        ParryState = new ParryState(this);
        HitState = new HitState(this);
        Fsm = new FSM();

        Jump.performed += OnJump;
        Jump.canceled += OnJump;
        Attack.performed += OnAttack;
        Dodge.performed += OnDodge;
        Parry.performed += OnParry;
        currHp = Data.MaxHp;
        dodgeCool = dodgeInterval;
    }

    private void OnDisable()
    {
        Jump.performed -= OnJump;
        Jump.canceled -= OnJump;
        Attack.performed -= OnAttack;
        Dodge.performed -= OnDodge;
    }

    private void Start() => Fsm.ChangeState(IdleState);

    public void OnMove(InputAction.CallbackContext context) => move = context.ReadValue<Vector2>();

    private void Update()
    {
        if (Fsm.CurrentState == DeathState)
            return;

        if(dodgeCool < dodgeInterval)
        {
            dodgeCool += Time.deltaTime;
        }

        Fsm.Update();
    }

    private void FixedUpdate()
    {
        if (Fsm.CurrentState == DeathState)
            return;

        Fsm.FixedUpdate();

        if (Fsm.CurrentState == DodgeState || Fsm.CurrentState == DeathState)
            return;

        Move(move);

        Grounded = Physics2D.OverlapCircle(groundCheck.position, Data.GroundCheckRadius, groundLayer);

        if (Grounded)
        {
            jumpCount = 0;
            coyoteCounter = Data.CoyoteTime;
        }
        else
        {
            if (coyoteCounter > 0f)
            {
                coyoteCounter -= Time.fixedDeltaTime;
                if (coyoteCounter <= 0f && jumpCount == 0)
                    jumpCount = 1;
            }
        }

        jumpBufferCounter -= Time.fixedDeltaTime;

        bool isAttacking = Fsm.CurrentState == AttackState;
        bool isHit = Fsm.CurrentState == HitState;

        if (jumpBufferCounter > 0f && (coyoteCounter > 0f || jumpCount < Data.MaxJumpCount))
        {
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, Data.JumpPower);

            if (!isAttacking && !isHit && Fsm.CurrentState != JumpState)
                Fsm.ChangeState(JumpState);

            jumpCount++;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        if (isAttacking || isHit)
        {
            if (!JumpHeld && Rb.linearVelocity.y > 0f)
            {
                Rb.linearVelocity += (Data.LowJumpMultiplier - 1f)
                    * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
            }

            if (Rb.linearVelocity.y < 0f)
            {
                Rb.linearVelocity += (Data.FallMultiplier - 1f)
                    * Physics2D.gravity.y * Time.fixedDeltaTime * Vector2.up;
            }
        }

        if (!isAttacking
            && !isHit
            && !Grounded
            && Rb.linearVelocity.y < -0.01f
            && Fsm.CurrentState != FallState
            && Fsm.CurrentState != JumpState)
        {
            Fsm.ChangeState(FallState);
        }
    }

    public void Move(Vector2 move)
    {
        if (move.x < 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            Animator.SetBool(MoveBool, true);

        }
        else if (move.x > 0f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            Animator.SetBool(MoveBool, true);
        }
        else
            Animator.SetBool(MoveBool, false);

        transform.position += Data.MoveSpeed * Time.fixedDeltaTime * new Vector3(move.x, 0f);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (Fsm.CurrentState == HitState) return;

        if (context.performed)
        {
            JumpHeld = true;
            jumpBufferCounter = Data.JumpBufferTime;
        }

        if (context.canceled)
            JumpHeld = false;
    }

    private void OnAttack(InputAction.CallbackContext _)
    {
        if (Fsm.CurrentState == HitState) return;

        if (isQueueOpen)
        {
            CommandQueue.Enqueue("A");
            return;
        }

        Fsm.ChangeState(AttackState);
    }

    public void AttackStart()
    {
        attackZone.Activate();
        IsAttackEnd = false;
    }

    public void AttackEnd()
    {
        attackZone.Deactivate();
        IsAttackEnd = true;
    }

    public IDamageable.DamageInfo SetDamage() => new() { canParry = false, damage = Data.Atk };

    public void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        if (Invincible)
        {
            return;
        }

        if (parrying && damageInfo.canParry)
        {
            SuccessParry?.Invoke();
            return;
        }


        currHp -= damageInfo.damage;

        if (currHp <= 0)
        {
            currHp = 0;
            Fsm.ChangeState(DeathState);
            return;
        }
        
        Fsm.ChangeState(HitState);



        OnHit?.Invoke();
    }

    private void OnDodge(InputAction.CallbackContext _)
    {
        if (Fsm.CurrentState == HitState || dodgeCool < dodgeInterval) return;
        dodgeCool = 0f;
        Fsm.ChangeState(DodgeState);
    }

    private void OnParry(InputAction.CallbackContext _)
    {
        if (Fsm.CurrentState == HitState) return;
        Fsm.ChangeState(ParryState);
    }

    public void OpenInputQueue()
    {
        isQueueOpen = true;
    }

    public void CloseInputQueue() => isQueueOpen = false;

    public void ToggleInvincible() => Invincible = !Invincible;

    public void ToggleParry() => parrying = !parrying;

    public void ResetAttackEnd() => IsAttackEnd = false;
}