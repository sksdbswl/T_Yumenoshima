using UnityEngine;

public interface IJobHandler
{
    Transform GetPriorityTarget(BehaviourTreeRunner runner);

    // 핵심: 성공/실패/진행을 구분
    BTNodeState PerformAction(BehaviourTreeRunner runner, float dt);
}