using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NpcMovement))]
[RequireComponent(typeof(NpcAnimationStateMachine))]
public class Npc : MonoBehaviour
{
    public NavMeshAgent Agent;
    public NpcMovement Movement { get; private set; }
    public NpcAnimationStateMachine Anim { get; private set; }
    
    private BehaviourTreeRunner bt;

    private void Awake()
    {
        Movement = GetComponent<NpcMovement>();
        Anim     = GetComponent<NpcAnimationStateMachine>();
        Agent    = GetComponent<NavMeshAgent>();
        bt       = GetComponent<BehaviourTreeRunner>();
    }
    
    private void OnEnable()
    {
        GameManager.Singleton.OnRoutineChanged += HandleRoutineChange;
    }
    
    private void OnDisable()
    {
        GameManager.Singleton.OnRoutineChanged -= HandleRoutineChange;
    }

    private void Start()
    {
        StartCoroutine(GameManager.Singleton.DayRoutineCoroutine());
    }
    
    private void HandleRoutineChange(RoutineState state)
    {
        if (bt != null) bt.routineState = state;
        bt.routineState = state;
        
        switch (state)
        {
            case RoutineState.Morning:
                Debug.Log("아침 입니다. 일어나세요");
                // BT가 알아서 Patrol(배회)하도록
                break;

            case RoutineState.Noon:
                Debug.Log("오후 입니다. 일하세요");
                // 직업별로 work target 세팅(은행원은 카운터, 경찰은 순찰지점 등)
                break;

            case RoutineState.Night:
                Debug.Log("저녁 입니다. 귀가하세요");
                // 여기서 이동을 직접 하지 말고 "집으로 가야 한다"만 표시
                var house = PlaceableInteraction.GetByInstanceId(Movement.npcSO.BuilderId);
                bt.homeTarget = house?.transform;
                
                break;
        }
    }
}