using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
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
            StartCoroutine(FireRoutineCoroutine());
            yield return new WaitForSeconds(60f);

            SetState(RoutineState.Noon);
            StopCoroutine(FireRoutineCoroutine());
            yield return new WaitForSeconds(10f);

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
    
    /// <summary>
    /// 인게임 화재 전환 : 일정 시간 기준으로 건물에 화재가 일어나는 루틴
    /// 테스트로 아침에만 일어나도록 되어있음
    /// </summary>
    public IEnumerator FireRoutineCoroutine()
    {
        while (true)
        {
            if (CurrentState == RoutineState.Morning)
            {
                TryIgniteRandomBuilding();
            }

            yield return new WaitForSeconds(20f);
        }
    }
    
    public void TryIgniteRandomBuilding()
    {
        var buildings = PlacementManager.Singleton.BuildingInstances;
        var candidates = new List<PlaceableInteraction>();

        for (int i = 0; i < buildings.Count; i++)
        {
            var b = buildings[i];
            if (b == null) continue;
            if (b.SourceItem == null) continue;
            if (!b.SourceItem.IsFire) continue;
            if (b.IsOnFire) continue;

            candidates.Add(b);
        }

        if (candidates.Count == 0)
            return;

        int index = Random.Range(0, candidates.Count);
        candidates[index].SetFire(true);

        Debug.Log($"화재 발생: {candidates[index].name}");
    }
}