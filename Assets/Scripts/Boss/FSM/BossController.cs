using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class BossController : MonoBehaviour, IDamageable
{
    public FSM Fsm { get; private set; }
    public Animator Animator { get; private set; }
    public float PlayerDistance { get; private set; }
    public int CurrHp { get; protected set; }
    public bool IsAttack { get; set; }
    public DecideState DecideState { get; protected set; }

    [SerializeField] private BossData data;
    public BossData Data => data;

    [SerializeField] private AttackZone attackZone;
    private Transform player;
    private int maxHp;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        DecideState = new DecideState(this);
    }

    private void Start()
    {
        if(GameObject.FindWithTag("Player") != null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
        maxHp = data.Hp;
        CurrHp = maxHp;
        Fsm = new FSM();
        attackZone.Deactivate();
        InitStates();
    }

    protected virtual void Update()
    {
        PlayerDistance = Vector3.Distance(transform.position, player.position);
        if (!IsAttack)
        {
            if (player.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        
        Fsm.Update();
    }

    protected abstract void InitStates();
    public abstract IState ChooseNextAction();

    public void StartAttack()
    {
        IsAttack = true;
    }

    public void OnAttackZone()
    {
        attackZone.Activate();
    }

    public void OffAttackZone()
    {
        attackZone.Deactivate();
    }

    public void EndAttack()
    {
        IsAttack = false;
    }

    public abstract IDamageable.DamageInfo SetDamage();

    public abstract void GetDamage(IDamageable.DamageInfo damageInfo);
}
