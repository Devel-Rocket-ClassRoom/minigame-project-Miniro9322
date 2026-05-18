using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class Boss : MonoBehaviour
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
    private float rushSpeed;
    private Vector2 rushVector;
    private bool isRush;
    private Animator animator;
    [SerializeField] private Transform attackZone;

    private static readonly Dictionary<string, int> AttackTriger = new()
    { 
        { "Attack1", Animator.StringToHash("Attack1")},
        { "Attack2", Animator.StringToHash("Attack2") },
        { "Rush", Animator.StringToHash("Rush") },
    };

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
                    break;
                case State.Move:
                    break;
                case State.Attack:
                    break;
                case State.Die:
                    break;
            }
        }
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
            rushVector = new Vector2(transform.position.x + rushAmount, transform.position.y);
        }
    }

    private void Attack1()
    {
        animator.SetTrigger(AttackTriger["Attack1"]);
    }

    private void Attack2()
    {
        animator.SetTrigger(AttackTriger["Attack2"]);
    }

    private void Rush()
    {
        isRush = true;
        animator.SetTrigger(AttackTriger["Rush"]);
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
    }
}
