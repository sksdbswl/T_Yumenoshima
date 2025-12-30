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

        // 공용 노드 상태로 "지금 뭐 하는 중인지" 판단
        var nightSeq   = treeAsset.nodes.OfType<SequenceNode>().FirstOrDefault(n => n.name.Contains("Night"));
        var workSeq    = treeAsset.nodes.OfType<SequenceNode>().FirstOrDefault(n => n.name.Contains("Work"));
        var patrolNode = treeAsset.nodes.OfType<PatrolNode>().FirstOrDefault();

        var moveHome   = treeAsset.nodes.OfType<MoveToTargetNode>().FirstOrDefault(n => n.name.Contains("Home"));
        var moveToTgt  = treeAsset.nodes.OfType<MoveToTargetNode>().FirstOrDefault(n => !n.name.Contains("Home"));

        var performJob = treeAsset.nodes.OfType<PerformJobActionNode>().FirstOrDefault();

        bool isNight  = nightSeq != null && nightSeq.state == BTNodeState.Running;
        bool isWork   = !isNight && workSeq != null && workSeq.state == BTNodeState.Running;

        // Work 중에서도 "공격/상호작용(Perform)" vs "추적/이동(MoveToTarget)" 구분
        bool isInteract = isWork && performJob != null && performJob.state == BTNodeState.Running;
        bool isMove     = isWork && !isInteract &&
                          (
                              (moveToTgt != null && moveToTgt.state == BTNodeState.Running) ||
                              (moveHome != null && moveHome.state == BTNodeState.Running)
                          );

        bool isPatrol = !isNight && !isWork &&
                        patrolNode != null &&
                        patrolNode.state == BTNodeState.Running;

        // === 공용 프로필 값 ===
        float patrolRadius      = profile != null ? profile.patrolRadius      : 3f;
        float detectionRange    = profile != null ? profile.detectionRange    : 5f;
        float interactionRange  = profile != null ? profile.interactionRange  : 2f;

        Color fill;
        Color line;
        float radius;

        if (isNight)
        {
            // 밤: 집(은신처)으로 이동/대기 상태 -> "이동/홈" 느낌으로 표시
            fill = new Color(0.7f, 0.2f, 1f, 0.10f);
            line = new Color(0.7f, 0.2f, 1f, 1f);
            radius = detectionRange; // 밤에도 주변 인지 범위로 보여주고 싶다면 detectionRange
        }
        else if (isInteract)
        {
            // 상호작용(도적이면 공격): interactionRange로 표시
            fill = new Color(1f, 0f, 0f, 0.10f);
            line = new Color(1f, 0f, 0f, 1f);
            radius = interactionRange;
        }
        else if (isMove)
        {
            // 이동/추적: detectionRange(인지/추적 시작 거리)로 표시
            fill = new Color(1f, 1f, 0f, 0.10f);
            line = new Color(1f, 1f, 0f, 1f);
            radius = detectionRange;
        }
        else if (isPatrol)
        {
            // 순찰: patrolRadius로 표시
            fill = new Color(0f, 0.5f, 1f, 0.10f);
            line = new Color(0f, 0.5f, 1f, 1f);
            radius = patrolRadius;
        }
        else
        {
            // 기본
            fill = new Color(0.3f, 0.3f, 0.3f, 0.10f);
            line = new Color(0.3f, 0.3f, 0.3f, 1f);
            radius = 2f;
        }

        Gizmos.color = fill;
        Gizmos.DrawSphere(transform.position, radius);

        Gizmos.color = line;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    // private void OnDrawGizmos()
    // {
    //     if (treeAsset == null || !Application.isPlaying) return;
    //
    //     // 이름 기준으로 Attack / Chase 시퀀스 찾기
    //     // 에디터에서 노드 이름이 "AttackSequenceNode", "ChaseSequenceNode" 이런 식이어야 함
    //     var attackSeq = treeAsset.nodes
    //         .OfType<SequenceNode>()
    //         .FirstOrDefault(n => n.name.Contains("Attack"));
    //
    //     var chaseSeq = treeAsset.nodes
    //         .OfType<SequenceNode>()
    //         .FirstOrDefault(n => n.name.Contains("Chase"));
    //
    //     bool isAttack = attackSeq != null && attackSeq.state == BTNodeState.Running;
    //     bool isChase  = chaseSeq  != null && chaseSeq.state == BTNodeState.Running;
    //
    //     // Patrol은 별도 노드 상태로 확인
    //     var patrolNode = treeAsset.nodes.OfType<PatrolNode>().FirstOrDefault();
    //     bool isPatrol = !isAttack && !isChase &&
    //                     patrolNode != null &&
    //                     patrolNode.state == BTNodeState.Running;
    //
    //     Color fill;
    //     Color line;
    //     float radius;
    //
    //     if (isAttack)
    //     {
    //         // Attack 상태
    //         fill = new Color(1f, 0f, 0f, 0.1f);
    //         line = new Color(1f, 0f, 0f, 1f);
    //         radius = 2f; // 공격 범위(원하는 값으로)
    //     }
    //     else if (isChase)
    //     {
    //         // Chase 상태
    //         fill = new Color(1f, 1f, 0f, 0.1f);
    //         line = new Color(1f, 1f, 0f, 1f);
    //         radius = 5f; // 추적 범위
    //     }
    //     else if (isPatrol)
    //     {
    //         // Patrol 상태
    //         float patrolRadius = patrolNode != null ? patrolNode.patrolRadius : 3f;
    //
    //         fill = new Color(0f, 0.5f, 1f, 0.1f);
    //         line = new Color(0f, 0.5f, 1f, 1f);
    //         radius = patrolRadius;
    //     }
    //     else
    //     {
    //         // 아무 브랜치도 Running이 아니면 기본값
    //         fill = new Color(0.3f, 0.3f, 0.3f, 0.1f);
    //         line = new Color(0.3f, 0.3f, 0.3f, 1f);
    //         radius = 2f;
    //     }
    //
    //     Gizmos.color = fill;
    //     Gizmos.DrawSphere(transform.position, radius);
    //
    //     Gizmos.color = line;
    //     Gizmos.DrawWireSphere(transform.position, radius);
    // }
}
