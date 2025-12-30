using UnityEngine;
using UnityEngine.AI;

public class ThiefJobHandler : MonoBehaviour, IJobHandler
{
    private enum Phase { Idle, Chasing, Attacking, Cooldown }
    private Phase phase = Phase.Idle;

    private float timer;
    private NavMeshAgent agent;

    private BehaviourTreeRunner runnerCache;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        runnerCache = GetComponent<BehaviourTreeRunner>();
    }

    // 1) BT의 SetJobTargetNode가 호출: "지금 추격할 타겟이 있나?"
    public Transform GetPriorityTarget(BehaviourTreeRunner runner)
    {
        if (runner == null || runner.profile == null) return null;

        // player 캐싱이 없으면 한번 더 찾는 안전장치(선택)
        if (runner.player == null)
            runner.player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (runner.player == null) return null;

        float dist = Vector3.Distance(runner.transform.position, runner.player.position);

        // 공격/쿨다운 중에는 "업무(Work) 브랜치를 유지"하려고 타겟을 계속 잡아둘지 선택해야 함.
        // - 공격 중에는 타겟 유지(플레이어) 하는게 자연스러움
        // - 쿨다운 중에는 멈춰있지만 타겟은 유지해도 됨 (MoveTo에서 움직이면 안되니 agent stop 처리)
        if (phase == Phase.Attacking || phase == Phase.Cooldown)
            return runner.player;

        // 감지 범위 안이면 추격 시작
        if (dist <= runner.profile.detectionRange)
        {
            phase = Phase.Chasing;
            return runner.player;
        }

        // 감지 범위 밖이면 BT는 Work 실패 → Patrol로 떨어짐
        phase = Phase.Idle;
        return null;
    }

    // 2) BT의 PerformJobActionNode가 호출: "도착 후 상호작용(공격) 처리"
    public bool PerformAction(BehaviourTreeRunner runner, float dt)
    {
        if (runner == null || runner.profile == null) return true;
        if (runner.player == null) return true;

        float dist = Vector3.Distance(runner.transform.position, runner.player.position);

        // 공격 사거리 밖이면 공격은 진행하지 않음(추격은 MoveToTarget이 해줌)
        if (dist > runner.profile.interactionRange)
        {
            // 공격하다가 멀어졌으면 다시 추격 상태로
            if (phase == Phase.Attacking) phase = Phase.Chasing;

            // "업무 완료"가 아니므로 계속 Running 유지하고 싶다 -> false
            // (BT에서는 false = Running)
            return false;
        }

        // 공격 사거리 안

        // 쿨다운 처리(공격 후 일시 정지)
        if (phase == Phase.Cooldown)
        {
            StopMovement(true);
            timer += dt;
            if (timer >= runner.profile.interactionCooldown)
            {
                timer = 0f;
                phase = Phase.Attacking; // 다음 공격 사이클로 재개(혹은 Chasing으로 돌려도 됨)
                StopMovement(false);
            }
            return false; // 아직 진행중
        }

        // 공격 시작/진행
        if (phase != Phase.Attacking)
        {
            phase = Phase.Attacking;
            timer = 0f;
            StopMovement(true);

            // 애니메이션 트리거가 있으면 여기
            // runner.GetComponent<Animator>()?.Play("Attack");
        }

        timer += dt;
        if (timer >= runner.profile.interactionTime)
        {
            // 한 사이클 공격 완료 -> 쿨다운으로
            timer = 0f;
            phase = Phase.Cooldown;

            // 여기서 데미지/히트 처리 넣기(한 사이클 종료 시점)
            // DealDamage();

            return false; // 쿨다운까지 포함해 계속 루프 돌릴 거라면 false 유지
            // 만약 "공격 1회 완료 = Success"로 보고 싶으면 true 반환하고,
            // 트리에서 다음 프레임 다시 Work 들어오게 해도 됨.
        }

        return false;
    }

    private void StopMovement(bool stop)
    {
        if (agent == null) return;
        agent.isStopped = stop;
        // 필요하면 velocity 0 처리, 애니 상태도 여기서
    }
}


// using UnityEngine;
//
// public class ThiefJobHandler : MonoBehaviour, IJobHandler
// {
//     private float timer;
//
//     public Transform GetPriorityTarget(BehaviourTreeRunner runner)
//     {
//         if (runner == null || runner.player == null || runner.profile == null) return null;
//
//         float d = Vector3.Distance(runner.transform.position, runner.player.position);
//         return d <= runner.profile.detectionRange ? runner.player : null;
//     }
//
//     public bool PerformAction(BehaviourTreeRunner runner, float dt)
//     {
//         if (runner == null || runner.player == null || runner.profile == null) return true;
//
//         // 상호작용 거리 안에 들어왔을 때만 공격 진행
//         float d = Vector3.Distance(runner.transform.position, runner.player.position);
//         if (d > runner.profile.interactionRange)
//         {
//             timer = 0f;               // 거리 벗어나면 공격 리셋
//             return false;             // 아직 작업 완료 아님(Running)
//         }
//
//         // 공격(상호작용) 진행
//         timer += dt;
//         if (timer >= runner.profile.interactionTime)
//         {
//             timer = 0f;
//             // 여기서 데미지/이펙트 트리거 가능
//             return true;              // 작업 완료(Success)
//         }
//
//         return false;                 // 진행중(Running)
//     }
// }