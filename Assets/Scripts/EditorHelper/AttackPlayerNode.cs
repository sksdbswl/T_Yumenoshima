using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Attack Player")]
public class AttackPlayerNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;
    public float attackTime = 1.5f;
    private float timer;
    private NavMeshAgent agent;

    protected override void OnStart()
    {
        Debug.Log("AttackPlayerNode OnStart :: 공격 시작");
        
        timer = 0f;
        agent = runner.GetComponent<NavMeshAgent>();
        if (agent != null) agent.isStopped = true;

        // TODO: 애니메이션 트리거
        // runner.GetComponent<Animator>()?.SetTrigger("Attack");
    }

    protected override BTNodeState OnUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= attackTime)
        {
            if (agent != null) agent.isStopped = false;
            return BTNodeState.Success;
        }
        return BTNodeState.Running;
    }
    
    protected override void OnStop()
    {
        if (agent == null && runner != null)
            agent = runner.GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.isStopped = false;
    }
}