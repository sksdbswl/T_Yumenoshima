using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Chase Player")]
public class ChasePlayerNode : ActionNode
{
    private static readonly int Chase = Animator.StringToHash("Chase");
    
    [System.NonSerialized] public BehaviourTreeRunner runner;
    private NavMeshAgent agent;
    public float stopDistance = 1.5f;

    protected override void OnStart()
    {
        Debug.Log("ChasePlayerNode OnStart :: 추격 중");
        
        agent = runner.GetComponent<NavMeshAgent>();
        runner.GetComponent<Animator>()?.Play(Chase);
    }

    protected override BTNodeState OnUpdate()
    {
        // 추격은 딱 한번 OnStart이후에 update만 체크된다
        if (agent == null) return BTNodeState.Failure;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return BTNodeState.Failure;

        agent.stoppingDistance = stopDistance;
        agent.SetDestination(player.transform.position);

        return BTNodeState.Running;
    }

    protected override void OnStop()
    {
        //ChildAbort();
        //Abort();
    }
}