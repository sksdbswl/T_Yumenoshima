using UnityEngine;
using UnityEngine.AI;

public class ThiefJobHandler : MonoBehaviour, IJobHandler
{
    private enum Phase { None, Attacking, Cooldown }
    private Phase phase = Phase.None;

    private float timer;
    private NavMeshAgent agent;
    private float distance;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 타깃 확인 ( 현재 player기준 )
    /// </summary>
    public Transform GetPriorityTarget(BehaviourTreeRunner runner)
    {
        var player = runner.player;
        distance = Vector3.Distance(runner.transform.position, player.position);

        // 감지 범위 밖이면 Work 끊기 -> Root에서 Patrol로
        if (distance > runner.profile.detectionRange)
        {
            phase = Phase.None;
            timer = 0f;
            StopMovement(false);
            return null;
        }

        // if (phase == Phase.Cooldown && distance <= runner.profile.interactionRange)
        // {
        //     phase = Phase.None;
        //     timer = 0f;
        //     StopMovement(false);
        //     return null;
        // }

        // 감지 범위 안이면 무조건 타겟 유지 (쿨다운이어도 유지)
        return player;
    }

    /// <summary>
    /// 노드 성공 처리
    /// </summary>
    public BTNodeState PerformAction(BehaviourTreeRunner runner, float dt)
    {
        // 공격 사거리 밖이면 "AttackSequence 실패" -> WorkSelector가 MoveTo(추적)로 내려감
        if (distance > runner.profile.interactionRange)
        {
            phase = Phase.None;
            timer = 0f;
            StopMovement(false);
            return BTNodeState.Failure;
        }

        // 쿨다운(사거리 안에서만 멈춤)
        if (phase == Phase.Cooldown)
        {
            StopMovement(true);
            timer += dt;

            if (timer >= runner.profile.interactionCooldown)
            {
                timer = 0f;
                phase = Phase.None;
                StopMovement(false);
                
            }

            return BTNodeState.Running;
        }

        // ✅ 공격 시작/진행
        if (phase != Phase.Attacking)
        {
            phase = Phase.Attacking;
            timer = 0f;
            StopMovement(true);
            // 애니메이션 트리거는 여기
        }

        timer += dt;
        if (timer >= runner.profile.interactionTime)
        {
            timer = 0f;
            phase = Phase.Cooldown;
            // 데미지 판정은 여기
        }

        return BTNodeState.Running;
    }


    private void StopMovement(bool stop)
    {
        if (agent == null) return;
        agent.isStopped = stop;
        if (stop) agent.velocity = Vector3.zero;
    }
}
