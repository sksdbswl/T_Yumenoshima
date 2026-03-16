using System.Collections;
using UnityEngine;

public partial class GameManager : SingletonBase<GameManager>
{
    /// <summary>
    /// 인게임 루틴 전환 : npc 상태 변경 또는 player 행동 제약 ( 임시 5분마다 변경 )
    /// 300f = 5분
    /// </summary>
    public event System.Action<RoutineState> OnRoutineChanged;
    public RoutineState CurrentState { get; private set; }

    public IEnumerator DayRoutineCoroutine()
    {
        while (true)
        {
            SetState(RoutineState.Morning);
            yield return new WaitForSeconds(10f); 

            SetState(RoutineState.Noon);
            yield return new WaitForSeconds(20f);

            SetState(RoutineState.Night);
            yield return new WaitForSeconds(30f);
        }
    }

    void SetState(RoutineState state)
    {
        CurrentState = state;
        
        Debug.Log($"State Changed: {state}");
        
        // TODO: 여기서 조명 변경, NPC 상태 변경 등 처리
        OnRoutineChanged?.Invoke(state); 
    }
}