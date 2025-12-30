using UnityEngine;

public class ThiefJobHandler : MonoBehaviour, IJobHandler
{
    private float timer;

    public Transform GetPriorityTarget(BehaviourTreeRunner runner)
    {
        if (runner == null || runner.profile == null) return null;

        if (runner.player == null)
            runner.player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (runner.player == null) return null;

        float d = Vector3.Distance(runner.transform.position, runner.player.position);
        return d <= runner.profile.detectionRange ? runner.player : null;
    }

    public BTNodeState PerformAction(BehaviourTreeRunner runner, float dt)
    {
        if (runner == null || runner.profile == null || runner.player == null)
            return BTNodeState.Failure;

        float d = Vector3.Distance(runner.transform.position, runner.player.position);

        // ✅ 사거리 밖이면 "공격 못함" -> Failure 반환 -> Sequence가 리셋되고 다시 추적함
        if (d > runner.profile.interactionRange)
        {
            timer = 0f;
            return BTNodeState.Failure;
        }

        // 공격(상호작용) 진행
        timer += dt;
        if (timer >= runner.profile.interactionTime)
        {
            timer = 0f;
            return BTNodeState.Success; // 1회 공격 완료
        }

        return BTNodeState.Running;
    }
}