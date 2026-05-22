using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;


[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Weighted Random Selector",
    description: "가중치 기반 랜덤 자식 선택. 미선택 시 가중치 상승 (불운 방지).",
    category: "Boss2",
    id: "b2c8d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"
)]
public partial class WeightedRandomSelector : Composite
{
    [Tooltip("각 자식 노드의 기본 가중치 (자식 순서와 일치)")]
    public List<float> BaseWeights = new List<float>();

    [Tooltip("미선택 시 가중치 증가량 (자식 순서와 일치)")]
    public List<float> WeightIncrements = new List<float>();

    private List<float> m_CurrentWeights;
    private int         m_ActiveIndex = -1;

    protected override Status OnStart()
    {
        if (Children.Count == 0) return Status.Failure;

        InitWeightsIfNeeded();
        m_ActiveIndex = PickWeightedRandom();
        ApplyWeightUpdate();

        return StartNode(Children[m_ActiveIndex]);
    }

    protected override Status OnUpdate()
    {
        if (m_ActiveIndex < 0 || m_ActiveIndex >= Children.Count)
            return Status.Failure;

        return Children[m_ActiveIndex].CurrentStatus;
    }

    protected override void OnEnd()
    {
        if (m_ActiveIndex >= 0 && m_ActiveIndex < Children.Count)
        {
            Node child = Children[m_ActiveIndex];
            if (child.CurrentStatus == Status.Running)
                EndNode(child);
        }
    }

    private void InitWeightsIfNeeded()
    {
        if (m_CurrentWeights != null && m_CurrentWeights.Count == Children.Count)
            return;

        m_CurrentWeights = new List<float>(Children.Count);
        for (int i = 0; i < Children.Count; i++)
        {
            float w = (i < BaseWeights.Count) ? BaseWeights[i] : 10f;
            m_CurrentWeights.Add(w);
        }
    }

    private int PickWeightedRandom()
    {
        float total = 0f;
        foreach (float w in m_CurrentWeights) total += w;

        float roll = UnityEngine.Random.Range(0f, total);
        float cum  = 0f;
        for (int i = 0; i < m_CurrentWeights.Count; i++)
        {
            cum += m_CurrentWeights[i];
            if (roll <= cum) return i;
        }
        return m_CurrentWeights.Count - 1;
    }

    private void ApplyWeightUpdate()
    {
        for (int i = 0; i < m_CurrentWeights.Count; i++)
        {
            if (i == m_ActiveIndex)
            {
                m_CurrentWeights[i] = (i < BaseWeights.Count) ? BaseWeights[i] : 10f;
            }
            else
            {
                float inc = (i < WeightIncrements.Count) ? WeightIncrements[i] : 5f;
                m_CurrentWeights[i] += inc;
            }
        }
    }
}
