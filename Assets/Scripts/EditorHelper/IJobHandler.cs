using UnityEngine;

public interface IJobHandler
{
    // 이 직업이 지금 우선 처리해야 할 타겟(없으면 null)
    Transform GetPriorityTarget(BehaviourTreeRunner runner);

    // 이동 후 수행할 행동(소화/검문/업무/공격 등)을 실행
    // true면 완료(Success), false면 진행중(Running)
    bool PerformAction(BehaviourTreeRunner runner, float dt);
}