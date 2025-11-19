using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : NpcInteraction, IInteractable
{
    private Npc Npc;
    private NpcSO NpcData => npcSO;
    private int HouseId => npcSO.BuilderId;
    private bool IsTalk => isTalkable;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float idleTimeMin = 1.0f;
    [SerializeField] private float idleTimeMax = 3.0f;

    private RoutineState _routineState;
    private System.Action _onArrived;
    private bool _agentHasArrived;

    private bool _isWandering;
    private Coroutine _wanderCoroutine;

    private void Awake()
    {
        Npc = GetComponent<Npc>();
        // TODO: 시간 체크해서 SetRoutineState()
    }

    public void SetRoutineState(RoutineState state)
    {
        _routineState = state;
    }

    // =======================
    // IInteractable 구현부
    // =======================

    /// <summary>
    /// 상호작용 시작 / 대화 한 줄 진행
    /// Player.OnInteractPerformed 에서 호출
    /// </summary>
    public void BeginInteract(Player player)
    {
        if (isTalkable)
        {
            // 이미 대화 중 → 다음 대사 시도
            bool hasNext = TryTalk();
            if (!hasNext)
            {
                Debug.Log("[NpcInteraction] 다음 대사 없음으로 대화 종료함");
                RequestEndTalk();
                // 자연 종료이므로 ESC 종료용 OnDialogClosed는 호출하지 않음 (기존 로직 유지)
            }
        }
        else
        {
            // 첫 대화 시작
            RequestTalk(player, this);
        }
    }

    /// <summary>
    /// 필요하면 "누르고 있는 동안" 등 추가 로직에 사용 가능
    /// 지금은 Begin과 동일하게 동작시키거나 비워둬도 된다.
    /// </summary>
    public void ContinueInteract(Player player)
    {
        // 프로젝트 정책에 따라:
        // BeginInteract와 동일하게 "다음 대사"로 써도 되고,
        // 또는 아무 것도 안 해도 됨.
        // 여기서는 일단 비워둠.
    }

    /// <summary>
    /// 상호작용 강제 종료 (ESC 등)
    /// Player.OnInteractCanceled 에서 호출
    /// </summary>
    public void EndInteract(Player player)
    {
        Debug.Log("Interact 강제 종료");

        if (npcSO != null)
            player.OnDialogClosed(npcSO);  // 기존 Player.OnInteractCanceled 로직을 여기로 이동

        RequestEndTalk();
    }

    // =======================
    // 집 가는 로직
    // =======================
    public void GoHome()
    {
        var house = PlaceableObject.GetByInstanceId(HouseId);

        if (house == null)
        {
            Debug.LogWarning($"NPC {name} :: homeHouseId={HouseId} 찾을 수 없음 → Wandering");
            StartWanderLoop();
            return;
        }

        Transform target = house.transform;

        GoTo(target, () =>
        {
            Debug.Log($"{name} arrived home: {house.name}");
            SetIdleAnim();
        });
    }

    // =======================
    // 배회 루프
    // =======================

    public void StartWanderLoop()
    {
        _isWandering = true;

        if (_wanderCoroutine != null)
            StopCoroutine(_wanderCoroutine);

        NextWanderStep();
    }

    public void StopWanderLoop()
    {
        _isWandering = false;

        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _wanderCoroutine = null;
        }

        Npc.Agent.isStopped = true;
        SetIdleAnim();
    }

    private void NextWanderStep()
    {
        if (!_isWandering) return;

        Vector3 center = transform.position;
        Vector3 randomPoint = GetRandomPointOnNavMesh(wanderRadius, center);

        SetRunAnim();

        GoTo(randomPoint, OnWanderArrived);
    }

    private void OnWanderArrived()
    {
        if (!_isWandering) return;

        SetIdleAnim();

        if (_wanderCoroutine != null)
            StopCoroutine(_wanderCoroutine);

        _wanderCoroutine = StartCoroutine(WanderIdleDelay());
    }

    private System.Collections.IEnumerator WanderIdleDelay()
    {
        float wait = Random.Range(idleTimeMin, idleTimeMax);
        yield return new WaitForSeconds(wait);

        if (_isWandering)
            NextWanderStep();
    }

    private Vector3 GetRandomPointOnNavMesh(float radius, Vector3 center)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }

    // =======================
    // 이동 관련 공통 로직
    // =======================

    public void GoTo(Transform target, System.Action onArrived)
    {
        GoTo(target.position, onArrived);
    }

    public void GoTo(Vector3 destination, System.Action onArrived)
    {
        _onArrived = onArrived;
        _agentHasArrived = false;

        Npc.Agent.isStopped = false;
        Npc.Agent.SetDestination(destination);
    }

    public void Pause()
    {
        // Npc.Agent.isStopped = true;
        // SetIdleAnim();
    }

    public void Resume()
    {
        // Npc.Agent.isStopped = false;
        // SetRunAnim();
    }

    private void Update()
    {
        if (!Npc.Agent.pathPending && Npc.Agent.remainingDistance <= Npc.Agent.stoppingDistance)
        {
            if (!_agentHasArrived)
            {
                _agentHasArrived = true;

                var callback = _onArrived;
                _onArrived = null;
                callback?.Invoke();
            }
        }
        else
        {
            _agentHasArrived = false;
        }
    }

    // =======================
    // 애니메이션 헬퍼
    // =======================
    private void SetIdleAnim()
    {
        // 예시: Npc.Animator.SetBool("IsMoving", false);
    }

    private void SetRunAnim()
    {
        // 예시: Npc.Animator.SetBool("IsMoving", true);
    }
}
