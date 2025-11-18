using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NpcMovement))]
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