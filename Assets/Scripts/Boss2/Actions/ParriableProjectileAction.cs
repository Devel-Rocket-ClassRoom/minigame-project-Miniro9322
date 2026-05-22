using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Parriable Projectile Attack",
    story: "Boss fires parriable projectiles",
    category: "Boss2/Actions",
    id: "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"
)]
public partial class ParriableProjectileAction : Action
{
    private Boss2Controller m_Controller;
    private Coroutine       m_Coroutine;
    private bool            m_Done;

    protected override Status OnStart()
    {
        m_Controller = GameObject.GetComponent<Boss2Controller>();
        if (m_Controller == null || m_Controller.IsActing)
            return Status.Failure;

        m_Done      = false;
        m_Coroutine = m_Controller.StartCoroutine(
            m_Controller.AttackParriableProjectile(_ => m_Done = true));

        return Status.Running;
    }

    protected override Status OnUpdate() =>
        m_Done ? Status.Success : Status.Running;

    protected override void OnEnd()
    {
        if (!m_Done && m_Controller != null && m_Coroutine != null)
            m_Controller.StopCoroutine(m_Coroutine);
    }
}
