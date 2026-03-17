using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NpcMovement))]
public class NpcAnimationStateMachine : AnimationStateMachine
{
    private Npc Npc;

    [Header("Speed Thresholds")]
    public float walkSpeedThreshold = 0.1f;  // 이 이상이면 이동으로 판단
    public float runSpeedThreshold  = 2.5f;  // 이 이상이면 Run, 이 이하면 Walk

    // Npc가 일을 하는 상태인지, Job 시스템 쪽에서 세팅해도 될듯
    public bool IsWorking { get; set; }

    private void Awake()
    {
        base.Awake();    
        Npc = GetComponent<Npc>();
    }
    
    private void Update()
    {
        UpdateStateFromMovement();
    }

    private void UpdateStateFromMovement()
    {
        if (Npc.agent == null || !Npc.agent.isOnNavMesh)
        {
            SetState(AnimState.Idle);
            return;
        }

        // NavMeshAgent 속도 기반으로 상태 결정
        float speed = Npc.agent.velocity.magnitude;

        if (IsWorking)
        {
            SetState(AnimState.Work);
        }
        else if (speed < walkSpeedThreshold)
        {
            SetState(AnimState.Idle);
        }
        else if (speed < runSpeedThreshold)
        {
            SetState(AnimState.Walk);
        }
        else
        {
            SetState(AnimState.Run);
        }
    }
}