using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossFSM Fsm { get; private set; }
    public Animator Animator { get; private set; }
    public float PlayerDistance { get; private set; }
    public int CurrHp { get; private set; }
    public bool IsAttackEnd { get; set; }

    [SerializeField] private BossData data;
    [SerializeField] private Dictionary<string, IState> states;

    private AttackZone attackZone;
    private Transform player;
    private int maxHp;



    private void Awake()
    {
        Animator = GetComponent<Animator>();
        attackZone = GetComponent<AttackZone>();
    }

    private void Start()
    {
        if(GameObject.FindWithTag("Player") != null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
        maxHp = data.Hp;
        CurrHp = maxHp;
        Fsm = new BossFSM();
        Fsm.ChangeState(new BossIdle(this));
    }

    private void Update()
    {
        PlayerDistance = Vector3.Distance(transform.position, player.position);
        Fsm.Update();
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
        IsAttackEnd = true;
    }
}
