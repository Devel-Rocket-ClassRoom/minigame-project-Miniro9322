using System;
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
    public Vector2 KnockbackDir { get; private set; }

    public UnityEvent SuccessParry;
    public UnityEvent OnGameOver;
    public UnityEvent OnHit;
    public UnityEvent<int, int> OnHpChange;
    public GameObject Effect;
    public UnityEvent ParryStart;
    public UnityEvent GamePause;
    public PlayerData Data;
    public Rigidbody2D Rb => rb;
    public SpriteRenderer Sr => sr;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private InputAction Jump;
    private InputAction Attack;
    private InputAction Dodge;
    private InputAction Parry;
    private InputAction Pause;

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
    private int notGroundedFrames = 0;  // 연속으로 공중에 있던 프레임 수

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackZone.gameObject.SetActive(false);
        OriginalGravityScale = rb.gravityScale;
        AfterImage = GetComponent<DashAfterImage>();
        sr = GetComponent<SpriteRenderer>();
        Effect.SetActive(false);
    }

    private void OnEnable()
    {
        Jump = InputSystem.actions.FindAction("Jump");
        Attack = InputSystem.actions.FindAction("Attack");
        Dodge = InputSystem.actions.FindAction("Dodge");
        Parry = InputSystem.actions.FindAction("Parry");
        Pause = InputSystem.actions.FindAction("Pause");

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
        Pause.performed += OnPause;
        currHp = Data.MaxHp;
        dodgeCool = dodgeInterval;
        OnHpChange?.Invoke(currHp, Data.MaxHp);
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

        // Grounded를 먼저 갱신해야 같은 프레임에서 FallState가 올바른 값을 참조함
        Grounded = Physics2D.OverlapCircle(groundCheck.position, Data.GroundCheckRadius, groundLayer);

        Fsm.FixedUpdate();

        if (Fsm.CurrentState == DodgeState || Fsm.CurrentState == DeathState)
            return;

        Move(move);

        if (Grounded)
        {
            jumpCount = 0;
            coyoteCounter = Data.CoyoteTime;
            notGroundedFrames = 0;
        }
        else
        {
            notGroundedFrames++;
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
            && notGroundedFrames > 2        // 2프레임 이상 연속으로 공중일 때만 전환 (1프레임 깜빡임 무시)
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
        {
            Animator.SetBool(MoveBool, false);
            // 입력 없을 때 x 속도 제거 → 발판 끝 미끄러짐 방지
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        transform.position += Data.MoveSpeed * Time.fixedDeltaTime * new Vector3(move.x, 0f);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (Fsm.CurrentState == HitState || Fsm.CurrentState == DodgeState) return;

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
        if (Fsm.CurrentState == HitState || Fsm.CurrentState == DodgeState) return;

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
        Debug.Log(1);
        IsAttackEnd = false;
    }

    public void AttackEnd()
    {
        attackZone.Deactivate();
        Debug.Log(2);
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


        KnockbackDir = damageInfo.knockbackDir;
        currHp -= damageInfo.damage;

        if (currHp <= 0)
        {
            currHp = 0;
            Fsm.ChangeState(DeathState);
            return;
        }
        
        Fsm.ChangeState(HitState);

        OnHit?.Invoke();
        OnHpChange?.Invoke(currHp, Data.MaxHp);
    }

    private void OnDodge(InputAction.CallbackContext _)
    {
        if (Fsm.CurrentState == HitState || dodgeCool < dodgeInterval) return;
        dodgeCool = 0f;
        Fsm.ChangeState(DodgeState);
    }

    private void OnParry(InputAction.CallbackContext _)
    {
        if (Fsm.CurrentState == HitState || Fsm.CurrentState == DodgeState) return;
        Fsm.ChangeState(ParryState);
    }

    private void OnPause(InputAction.CallbackContext _)
    {
        if(Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        GamePause?.Invoke();
    }

    public void OpenInputQueue()
    {
        isQueueOpen = true;
    }

    public void CloseInputQueue() => isQueueOpen = false;

    public void ToggleInvincible() => Invincible = !Invincible;

    public void ToggleParry() => parrying = !parrying;

    public void ResetAttackEnd() => IsAttackEnd = false;

    public void EnableEffect()  => Effect.SetActive(true);
    public void DisableEffect() => Effect.SetActive(false);

    private void ToggleEffect()
    {
        Effect.SetActive(!Effect.activeSelf);
    }
}