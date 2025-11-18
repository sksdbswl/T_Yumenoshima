using UnityEngine;
using UnityEngine.AI;

public enum NpcRoutineState
{
    Morning,
    Noon,
    Night,
}

public class NpcMovement : NpcInteraction
{
    public NavMeshAgent agent;
    private NpcSO Npc => npcSO;
    public int HouseId => npcSO.BuilderId;

    private NpcRoutineState _routineState;
    private System.Action _onArrived;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        //TODO:: 시간 체크해서 SetRoutineState()
    }

    private void Start()
    {
        GoHome();
    }
    
    public void SetRoutineState(NpcRoutineState state)
    {
        _routineState = state;
    }

    public void GoHome()
    {
        // ================================
        // 1) 집 빌딩 인스턴스 찾기
        // ================================
        var house = PlaceableObject.GetByInstanceId(HouseId);

        if (house == null)
        {
            Debug.LogWarning($"NPC {name} :: homeHouseId={HouseId} 찾을 수 없음 → Wandering");
            StartWander();
            return;
        }

        // ================================
        // 2) Entrance 찾기 (없으면 건물 중심)
        // ================================
        Transform target = house.transform;

        // ================================
        // 3) NavMesh 이동
        // ================================
        GoTo(target, () =>
        {
            Debug.Log($"{name} arrived home: {house.name}");
            // 도착 후 원하는 루틴 실행 (예: Idle, Sleep 등)
        });
    }

    public void StartWander()
    {
        // 랜덤 포인트 찾고 이동
    }

    public void GoTo(Transform target, System.Action onArrived)
    {
        _onArrived = onArrived;
        agent.isStopped = false;
        agent.SetDestination(target.position);
        // Update에서 목적지 도달 체크 → 도착하면 _onArrived 호출
    }

    public void Pause()
    {
        agent.isStopped = true;
    }

    public void Resume()
    {
        agent.isStopped = false;
    }

    private void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!_agentHasArrived) // 적당히 플래그 체크
            {
                _agentHasArrived = true;
                _onArrived?.Invoke();
            }
        }
        else
        {
            _agentHasArrived = false;
        }
    }

    private bool _agentHasArrived;
}