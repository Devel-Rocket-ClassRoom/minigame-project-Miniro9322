using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Fire Pillar Attack",
    story: "Boss summons fire pillars",
    category: "Boss2/Actions",
    id: "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"
)]
public partial class FirePillarAction : Action
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
            m_Controller.AttackFirePillar(_ => m_Done = true));

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
