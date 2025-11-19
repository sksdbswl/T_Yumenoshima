using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NpcMovement))]
[RequireComponent(typeof(NpcInteraction))]
[RequireComponent(typeof(NpcAnimationStateMachine))]
public class Npc : MonoBehaviour
{
    public NavMeshAgent Agent;
    public NpcMovement Movement { get; private set; }
    public NpcAnimationStateMachine Anim { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<NpcMovement>();
        Anim     = GetComponent<NpcAnimationStateMachine>();
        Agent = GetComponent<NavMeshAgent>();
    }
    
    private void OnEnable()
    {
        GameManager.Singleton.OnRoutineChanged += HandleRoutineChange;
        //HandleRoutineChange(GameManager.Singleton.CurrentState);
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
        switch (state)
        {
            case RoutineState.Morning:
                Debug.Log("아침 입니다. 일어나세요");
                Movement.StartWanderLoop();
                //Anim.IsWorking = false;
                break;

            case RoutineState.Noon:
                Debug.Log("오후 입니다. 일하세요");

                //Movement.StartWander();
                //Anim.IsWorking = false;
                break;

            case RoutineState.Night:
                Debug.Log("저녁 입니다. 귀가하세요");
                Movement.StopWanderLoop();
                Movement.GoHome();
                //Anim.IsWorking = false;
                break;
        }
    }
    
    // public void GoHome()
    // {
    //     Movement.GoHome();
    // }
    //
    // public void StartWander()
    // {
    //     Movement.StartWander();
    // }
    //
    // public void SetWorking(bool working)
    // {
    //     Anim.IsWorking = working;
    // }
}