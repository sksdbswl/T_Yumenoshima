using UnityEngine;
using UnityEngine.AI;

public class NpcMovement : NpcInteraction
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
    // 집 가는 로직 (그대로)
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

    // 외부에서 이 함수만 호출하면:
    // 랜덤 지점 찾고 run → 도착하면 idle → 다시 찾고 run 반복
    public void StartWanderLoop()
    {
        _isWandering = true;

        if (_wanderCoroutine != null)
            StopCoroutine(_wanderCoroutine);

        // 바로 첫 스텝 시작
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

        // 잠깐 idle 상태 유지 후 다시 배회
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

    // NavMesh 위에서 랜덤 포인트 찾기
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

        return center; // 실패하면 그냥 현재 위치
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
        // 또는 Npc.SetState(NpcState.Idle); 이런 식으로
    }

    private void SetRunAnim()
    {
        // 예시: Npc.Animator.SetBool("IsMoving", true);
        // 또는 Npc.SetState(NpcState.Run); 이런 식으로
    }
}
