using UnityEngine;

public class FirefighterJob : MonoBehaviour, IJobHandler
{
    public Transform fireTarget; // 외부 시스템이 넣어줌
    private float timer;

    public Transform GetPriorityTarget(BehaviourTreeRunner runner)
    {
        return fireTarget != null ? fireTarget : null;
    }

    public bool PerformAction(BehaviourTreeRunner runner, float dt)
    {
        if (fireTarget == null) return true; // 이미 꺼짐

        timer += dt;
        if (timer >= 3f)
        {
            timer = 0f;
            fireTarget = null;       // 작업 끝나면 타겟 해제
            runner.currentTarget = null;
            return true;
        }
        return false;
    }
}