using TestBT;
using UnityEngine;
using UnityEngine.AI;

public sealed class Npc : MonoBehaviour
{
    public NpcSO npcSO;
    
    [HideInInspector]public NavMeshAgent agent;
    [HideInInspector]public SimulationNpcSensor sensor;
    [HideInInspector]public SimulationNpcExecutor executor;

    private void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        sensor   = GetComponent<SimulationNpcSensor>();
        executor = GetComponent<SimulationNpcExecutor>();
        sensor.npcSO = npcSO;
        executor.npcSO = npcSO;
    }
    
    private void OnEnable()
    {
        GameManager.Singleton.OnRoutineChanged += HandleRoutineChange;
    }
    
    private void OnDisable()
    {
        GameManager.Singleton.OnRoutineChanged -= HandleRoutineChange;
    }
    
    private void HandleRoutineChange(RoutineState state)
    {
        switch (state)
        {
            case RoutineState.Morning:
                Debug.Log("아침 입니다. 일어나세요");
                // BT가 알아서 Patrol(배회)하도록
                sensor.Blackboard.canHome = false;
                
                break;

            case RoutineState.Noon:
                Debug.Log("오후 입니다. 일하세요");
                // 직업별로 work target 세팅(은행원은 카운터, 경찰은 순찰지점 등)
                break;

            case RoutineState.Night:
                Debug.Log("저녁 입니다. 귀가하세요");
                // 여기서 이동을 직접 하지 말고 집으로 가야 한다만 표시
                sensor.Blackboard.canHome = true;
                
                break;
        }
    }
}