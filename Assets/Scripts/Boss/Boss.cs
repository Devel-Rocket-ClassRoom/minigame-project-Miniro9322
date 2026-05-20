using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Boss : MonoBehaviour, IDamageable
{
    private enum State
    {
        Idle,
        Move,
        Attack,
        Die,
    }

    private static readonly Dictionary<string, int> AttackTriger = new()
    {
        { "Attack1", Animator.StringToHash("Attack1")},
        { "Attack2", Animator.StringToHash("Attack2") },
        { "Rush", Animator.StringToHash("Rush") },
    };
    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Parry = Animator.StringToHash("Parry");
    private static readonly int Die = Animator.StringToHash("Die");

    
    [SerializeField] private Transform attackZone;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private BossData data;
    [SerializeField] private float rushAmount;
    [SerializeField] private float rushDuration;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attack2Interval = 5f;
    [SerializeField] private float stunInterval = 3f;
    [SerializeField] private float attack1Range = 3f;
    [SerializeField] private float rushRange = 3f;

    private Animator animator;
    private Vector3 rushVector;
    private Vector3 startPoint;
    
    private bool canParry;
    private bool isRush;
    private bool isGameOver;
    private int atk;
    private float stunTime = 0f;
    private float rushTime;
    private float attack2CoolTime = 0f;
    private float maxHp;
    private float currHp;
    public Animator Animator => animator;


    private State currentState;
    private State CurrentState
    {
        get
        {
            return currentState;
        }

        set
        {
            switch (value)
            {
                case State.Idle:
                    animator.SetBool(Move, false);
                    isRush = false;
                    rushTime = 0f;
                    currentState = value;
                    break;
                case State.Move:
                    animator.SetBool(Move, true);
                    currentState = value;
                    break;
                case State.Attack:
                    animator.SetBool(Move, false);
                    currentState = value;
                    break;
                case State.Die:
                    animator.SetBool(Move, false);
                    currentState = value;
                    break;
            }
        }
    }

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        attackZone.gameObject.SetActive(false);
        maxHp = data.Hp;
        currHp = maxHp;
    }

    private void Update()
    {
        if (CurrentState == State.Die || isGameOver)
        {
            return;
        }

        if (stunTime > 0f)
        {
            stunTime -= Time.deltaTime;
            return;
        }

        if (CurrentState == State.Attack) return;

        if (playerPosition.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }

        attack2CoolTime += Time.deltaTime;
        float distance = Vector3.Distance(transform.position, playerPosition.position);

        if (attack2CoolTime >= attack2Interval)
        {
            Attack2();
            attack2CoolTime = 0f;
        }
        else if (distance < attack1Range)
        {
            Attack1();
        }
        else if (distance > rushRange)
        {
            Rush();
        }
        else
        {
            FollowPlayer();
        }
    }

    private void FixedUpdate()
    {
        if (CurrentState == State.Die || isGameOver)
        {
            return;
        }

        if (isRush)
        {
            rushTime += Time.fixedDeltaTime;
            rushVector = new Vector3(startPoint.x + rushAmount * transform.localScale.x, startPoint.y);
            transform.position = Vector3.Lerp(startPoint, rushVector, rushTime / rushDuration);
            if (rushTime > rushDuration)
            {
                transform.position = rushVector;
                CurrentState = State.Idle;
                rushTime = 0f;
            }
        }
    }

    private void FollowPlayer()
    {
        CurrentState = State.Move;
        Vector3 direction = new Vector3(Mathf.Sign(playerPosition.position.x - transform.position.x), 0f, 0f);
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void Attack1()
    {
        animator.SetTrigger(AttackTriger["Attack1"]);
        CurrentState = State.Attack;
        atk = data.atk;
        canParry = false;
    }

    private void Attack2()
    {
        animator.SetTrigger(AttackTriger["Attack2"]);
        CurrentState = State.Attack;
        atk = data.atk * 2;
        canParry = false;
    }

    private void Rush()
    {
        isRush = true;
        startPoint = transform.position;
        animator.SetTrigger(AttackTriger["Rush"]);
        CurrentState = State.Attack;
        atk = Mathf.CeilToInt(data.atk * 1.5f);
        canParry = true;
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
        foreach(var triger in AttackTriger)
        {
            animator.ResetTrigger(triger.Value);
        }

        if (CurrentState == State.Die)
        {
            return;
        }

        CurrentState = State.Idle;
    }

    public void GetDamage(IDamageable.DamageInfo damageInfo)
    {
        if(CurrentState == State.Die)
        {
            return;
        }

        currHp -= damageInfo.damage;

        if(currHp <= 0)
        {
            currHp = 0;
            animator.SetTrigger(Die);
            CurrentState = State.Die;
        }
    }

    public void OnGameOver()
    {
        CurrentState = State.Idle;
        isGameOver = true;
    }

    public IDamageable.DamageInfo SetDamage()
    {
        return new IDamageable.DamageInfo() { canParry = this.canParry, damage = atk };
    }

    public void GetParryed()
    {
        stunTime = stunInterval;
        animator.SetTrigger(Parry);
        CurrentState = State.Idle;
    }
}
