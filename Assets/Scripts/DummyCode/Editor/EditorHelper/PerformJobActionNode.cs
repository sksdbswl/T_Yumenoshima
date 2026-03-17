using UnityEngine;

[CreateAssetMenu(menuName = "AI/Nodes/Action/Perform Job Action")]
public class PerformJobActionNode : ActionNode
{
    [System.NonSerialized] public BehaviourTreeRunner runner;

    // 캐시(인터페이스는 런타임에 바뀔 수 있으니 OnStart/OnUpdate 둘 다 안전장치)
    private IJobHandler job;

    // (선택) 디버그
    [Header("Debug")]
    public bool logStateChanges = false;
    private BTNodeState _lastLogged = (BTNodeState)(-1);

    protected override void OnStart()
    {
        if (runner == null)
        {
            Debug.LogError("[PerformJobActionNode] runner is NULL (binding missing)");
            job = null;
            return;
        }

        // 인터페이스 컴포넌트 안전 탐색
        job = runner.GetComponent(typeof(IJobHandler)) as IJobHandler;

        if (job == null)
        {
            Debug.LogError($"[PerformJobActionNode] IJobHandler not found on '{runner.name}'");
        }
    }

    protected override BTNodeState OnUpdate()
    {
        // runner가 늦게 바인딩되거나, 런타임에 트리가 복제/교체되었을 때 대비
        if (runner == null)
            return BTNodeState.Failure;

        // job이 null이면 매 프레임 재탐색(안전장치)
        if (job == null)
        {
            job = runner.GetComponent(typeof(IJobHandler)) as IJobHandler;
            if (job == null)
                return BTNodeState.Failure;
        }

        // 타겟/프로필이 없으면 액션 성립 불가(직업 구현에 따라 다르지만 보통 안전)
        if (runner.profile == null)
            return BTNodeState.Failure;

        // 실제 행동 수행
        var result = job.PerformAction(runner, Time.deltaTime);

        // (선택) 상태 변화 로그
        if (logStateChanges && result != _lastLogged)
        {
            Debug.Log($"[PerformJobActionNode] {runner.name} -> {result}");
            _lastLogged = result;
        }

        return result;
    }

    protected override void OnStop()
    {
        // 공격/작업 도중 강제 중단 시 핸들러가 별도 정리할 게 있으면,
        // IJobHandler에 Optional 메서드를 추가하거나(예: CancelAction) 여기서 캐스팅 호출 가능.
        // 지금은 최소 구현으로 비워둠.
    }
}