using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName="AI/Nodes/Action/Idle At Home")]
public class IdleAtHomeNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;

    protected override void OnStart()
    {
        if (runner == null) return;
        agent = runner.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        // 애니메이션 Idle로 바꾸고 싶으면 여기서
    }

    protected override BTNodeState OnUpdate()
    {
        // 밤이면 계속 Running 유지해서 이 브랜치 고정
        if (runner != null && runner.routineState == RoutineState.Night)
            return BTNodeState.Running;

        // 아침/낮 되면 브랜치 빠져나가게 Success(또는 Failure) 반환
        if (agent != null) agent.isStopped = false;
        return BTNodeState.Success;
    }
}