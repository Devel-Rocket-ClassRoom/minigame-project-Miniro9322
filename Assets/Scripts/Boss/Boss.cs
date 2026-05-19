using System.Collections.Generic;
using UnityEngine;

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
    private float rushDuration;
    private float rushTime;
    private Vector3 rushVector;
    private Vector3 startPoint;
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
            rushTime += Time.fixedDeltaTime;
            rushVector = new Vector3(startPoint.x + rushAmount, startPoint.y);
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
    }

    private void Attack2()
    {
        animator.SetTrigger(AttackTriger["Attack2"]);
    }

    private void Rush()
    {
        isRush = true;
        startPoint = transform.position;
        Debug.Log(startPoint);
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
