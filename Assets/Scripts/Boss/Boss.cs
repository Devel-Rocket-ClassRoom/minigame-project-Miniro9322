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

    [SerializeField]
    private float rushAmount;
    [SerializeField]
    private float rushDuration;
    [SerializeField]
    private float moveSpeed = 2f;
    private float rushTime;
    private Vector3 rushVector;
    private Vector3 startPoint;
    private bool isRush;
    private Animator animator;
    [SerializeField] private Transform attackZone;
    private float attack2CoolTime = 0f;
    [SerializeField] private float attack2Interval = 5f;
    [SerializeField] private Transform playerPosition;

    private static readonly Dictionary<string, int> AttackTriger = new()
    { 
        { "Attack1", Animator.StringToHash("Attack1")},
        { "Attack2", Animator.StringToHash("Attack2") },
        { "Rush", Animator.StringToHash("Rush") },
    };
    private static readonly int Move = Animator.StringToHash("Move");

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

    private void Update()
    {
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
        else if (distance < 3f)
        {
            Attack1();
        }
        else if (distance > 7f)
        {
            Rush();
        }
        else
        {
            FollowPlayer();
        }
    }

    private void FollowPlayer()
    {
        CurrentState = State.Move;
        Vector3 direction = new Vector3(Mathf.Sign(playerPosition.position.x - transform.position.x), 0f, 0f);
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        attackZone.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (isRush)
        {
            rushTime += Time.fixedDeltaTime;
            rushVector = new Vector3(startPoint.x + rushAmount * transform.localScale.x, startPoint.y);
            transform.position = Vector3.Lerp(startPoint, rushVector, rushTime / rushDuration);
            if(rushTime > 1f)
            {
                transform.position = rushVector;
                isRush = false;
                rushTime = 0f;
            }
        }
    }

    private void Attack1()
    {
        animator.SetTrigger(AttackTriger["Attack1"]);
        CurrentState = State.Attack;
    }

    private void Attack2()
    {
        animator.SetTrigger(AttackTriger["Attack2"]);
        CurrentState = State.Attack;
    }

    private void Rush()
    {
        isRush = true;
        startPoint = transform.position;
        Debug.Log(startPoint);
        animator.SetTrigger(AttackTriger["Rush"]);
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
        foreach(var triger in AttackTriger)
        {
            animator.ResetTrigger(triger.Value);
        }

        CurrentState = State.Idle;
    }

    public void OnDamage(int damage)
    {
        Debug.Log(damage);
    }

    public int GetDamageAmount()
    {
        return 10;
    }
}
