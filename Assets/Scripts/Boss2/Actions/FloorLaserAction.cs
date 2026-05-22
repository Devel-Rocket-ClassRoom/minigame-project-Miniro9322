using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Floor Laser Attack",
    story: "Boss activates floor lasers",
    category: "Boss2/Actions",
    id: "d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"
)]
public partial class FloorLaserAction : Action
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
            m_Controller.AttackFloorLaser(_ => m_Done = true));

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
