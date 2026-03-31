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
    public RoutineState CurrentRoutine { get; private set; }
    
    public IEnumerator DayRoutineCoroutine()
    {
        while (true)
        {
            SetState(RoutineState.Morning);
            yield return new WaitForSeconds(30f);

            SetState(RoutineState.Noon);
            _routineCoroutine = StartCoroutine(RoutineCoroutine());
            yield return new WaitForSeconds(10f);

            SetState(RoutineState.Night);
            if (_routineCoroutine != null) StopCoroutine(_routineCoroutine);
            yield return new WaitForSeconds(30f);
        }
    }

    void SetState(RoutineState state)
    {
        CurrentRoutine = state;
        
        Debug.Log($"State Changed: {state}");
        
        // TODO: 여기서 날씨나 조명 변경
        OnRoutineChanged?.Invoke(state); 
    }
    
    Coroutine _routineCoroutine;
    public IEnumerator RoutineCoroutine()
    {
        while (true)
        {
            if (CurrentRoutine == RoutineState.Morning)
            {
               // 직업 가져오기 가능
            }

            if (CurrentRoutine == RoutineState.Noon)
            {
                //TryChangeRandomBuildingFired();
                //TryChangeRandomNpcEmotionTired();
            }

            yield return new WaitForSeconds(20f);
        }
    }
    
    /// <summary>
    /// 인게임 화재 전환 : 일정 시간 기준으로 건물에 화재가 일어나는 루틴
    /// </summary>
    public void TryChangeRandomBuildingFired()
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
    
    /// <summary>
    /// 인게임 NPC 상태 전환 : 일정 시간 기준으로 Npc가 환자가 되는 루틴
    /// </summary>
    public void TryChangeRandomNpcEmotionTired()
    {
        if (_spawnedNpcStatuses.Count == 0) return;

        int index = Random.Range(0, _spawnedNpcStatuses.Count);
        var targetNpc = _spawnedNpcStatuses[index];

        targetNpc._npcStatus.ChangeEmotion(targetNpc,Const.EEmotion.Tired);
        targetNpc.executor.currentEmotionIcon = 
            NpcEmotionManager.Instance.ShowEmotion(Const.EEmotion.Tired, targetNpc.transform, Vector3.up * 1.5f);
        
        Debug.Log($"NPC 상태 변경: {targetNpc.name} → Tired");
    }
}