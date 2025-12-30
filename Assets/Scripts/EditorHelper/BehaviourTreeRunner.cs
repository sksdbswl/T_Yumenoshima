using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class BehaviourTreeRunner : MonoBehaviour
{
    public BTTree treeAsset; // Enemy NodeTree
    // 모든 노드 상태 초기화 : BTNodeState.Failure
    public AIRoleProfile profile;

    public Transform currentTarget;
    public Transform player;
    public Transform homeTarget;
    public RoutineState routineState;
    
    [HideInInspector] public IJobHandler job;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        job = GetComponent<IJobHandler>(); // FirefighterJob, PoliceJob, BankerJob 같은 컴포넌트
        
        foreach (var node in treeAsset.nodes)
        {
            node.ResetState();
            if (node is IsNightNode n) n.runner = this;
            if (node is SetHomeTargetNode h) h.runner = this;
            if (node is SetJobTargetNode s) s.runner = this;
            if (node is PerformJobActionNode a) a.runner = this;
            if (node is MoveToTargetNode m) m.runner = this;
            if (node is PatrolNode p) p.runner = this;
        }
    }

    private void Update() => treeAsset?.Update();

    // === 여기부터 기즈모 ===
    private void OnDrawGizmos()
    {
        if (treeAsset == null || !Application.isPlaying) return;

        // 이름 기준으로 Attack / Chase 시퀀스 찾기
        // 에디터에서 노드 이름이 "AttackSequenceNode", "ChaseSequenceNode" 이런 식이어야 함
        var attackSeq = treeAsset.nodes
            .OfType<SequenceNode>()
            .FirstOrDefault(n => n.name.Contains("Attack"));

        var chaseSeq = treeAsset.nodes
            .OfType<SequenceNode>()
            .FirstOrDefault(n => n.name.Contains("Chase"));

        bool isAttack = attackSeq != null && attackSeq.state == BTNodeState.Running;
        bool isChase  = chaseSeq  != null && chaseSeq.state == BTNodeState.Running;

        // Patrol은 별도 노드 상태로 확인
        var patrolNode = treeAsset.nodes.OfType<PatrolNode>().FirstOrDefault();
        bool isPatrol = !isAttack && !isChase &&
                        patrolNode != null &&
                        patrolNode.state == BTNodeState.Running;

        Color fill;
        Color line;
        float radius;

        if (isAttack)
        {
            // Attack 상태
            fill = new Color(1f, 0f, 0f, 0.1f);
            line = new Color(1f, 0f, 0f, 1f);
            radius = 2f; // 공격 범위(원하는 값으로)
        }
        else if (isChase)
        {
            // Chase 상태
            fill = new Color(1f, 1f, 0f, 0.1f);
            line = new Color(1f, 1f, 0f, 1f);
            radius = 5f; // 추적 범위
        }
        else if (isPatrol)
        {
            // Patrol 상태
            float patrolRadius = patrolNode != null ? patrolNode.patrolRadius : 3f;

            fill = new Color(0f, 0.5f, 1f, 0.1f);
            line = new Color(0f, 0.5f, 1f, 1f);
            radius = patrolRadius;
        }
        else
        {
            // 아무 브랜치도 Running이 아니면 기본값
            fill = new Color(0.3f, 0.3f, 0.3f, 0.1f);
            line = new Color(0.3f, 0.3f, 0.3f, 1f);
            radius = 2f;
        }

        Gizmos.color = fill;
        Gizmos.DrawSphere(transform.position, radius);

        Gizmos.color = line;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
