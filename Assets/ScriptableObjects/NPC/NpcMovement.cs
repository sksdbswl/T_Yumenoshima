using UnityEngine;
using UnityEngine.AI;

public enum NpcRoutineState
{
    Morning,
    Noon,
    Night,
}

public class NpcMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public int homeHouseId;

    private NpcRoutineState _routineState;
    private System.Action _onArrived;

    public void SetRoutineState(NpcRoutineState state)
    {
        _routineState = state;
    }

    public void GoHome()
    {
        var entrance = 0;
        // var entrance = HouseRegistry.I.GetEntrance(homeHouseId);
        if (entrance == null)
        {
            StartWander();
            return;
        }
        
        //GoTo(entrance, null);
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