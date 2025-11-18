using UnityEngine;

public enum AnimState
{
    Idle,
    Walk,
    Run,
    Work,
    // 플레이어는 여기 위에 기반으로 추가 enum 따로 만들어도 되고,
    // Animator 파라미터만 더 써도 됨.
}

public abstract class AnimationStateMachine : MonoBehaviour
{
    public AnimState CurrentState { get; private set; }

    [Header("Animator")]
    [SerializeField] protected Animator animator;

    // Animator State 이름을 해시로
    protected readonly int HashIdle = Animator.StringToHash("Idle");
    protected readonly int HashWalk = Animator.StringToHash("Walk");
    protected readonly int HashRun  = Animator.StringToHash("Run");
    protected readonly int HashWork = Animator.StringToHash("Work");

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 상태 전환 (외부에서 호출)
    /// </summary>
    public void SetState(AnimState newState)
    {
        if (newState == CurrentState) return;

        OnExitState(CurrentState);
        CurrentState = newState;
        OnEnterState(newState);
    }

    /// <summary>
    /// 상태 진입 시 공통 처리
    /// </summary>
    protected virtual void OnEnterState(AnimState state)
    {
        switch (state)
        {
            case AnimState.Idle:
                Play(HashIdle);
                break;
            case AnimState.Walk:
                Play(HashWalk);
                break;
            case AnimState.Run:
                Play(HashRun);
                break;
            case AnimState.Work:
                Play(HashWork);
                break;
        }
    }

    /// <summary>
    /// 상태 종료 시 필요한 작업 있으면 자식에서 오버라이드
    /// </summary>
    protected virtual void OnExitState(AnimState state) { }

    protected void Play(int stateHash, float crossFadeTime = 0.1f)
    {
        if (animator == null) return;
        animator.CrossFade(stateHash, crossFadeTime);
    }
}