using UnityEngine;
using UnityEngine.AI;

public class NpcInteraction : MonoBehaviour
{
    public NpcSO npcSO; 

    /// <summary>
    /// 플레이어가 이 NPC와 대화 시도할 때 호출
    /// </summary>
    public void TryTalk(DialogTyper typer)
    {
        if (npcSO == null)
        {
            Debug.LogError("[NpcInteraction] npcSO is null.");
            return;
        }

        int npcId = npcSO.BuilderId;
        int stage = PlayerProgress.GetStage(npcId);

        var line = DialogRepository.I.PickNext(npcId, stage);
        if (line == null) return;
        
        string speakerName = line.Speaker == "Player"
            ? "Player"        
            : npcSO.Name;     // NPC 이름 사용 (CSV의 NPC와 동일)

        // 대사 재생
        typer.PlayLine(speakerName, line.Kor);

        // 스토리 진행 로직
        if (line.IsStory)
        {
            int nextOrder = line.Order + 1;
            // 아직 Stage 끝 아니면 Order만 증가
            PlayerProgress.SetOrder(npcId, stage, nextOrder);

            // Stage 끝났는지 체크
            if (DialogRepository.I.IsStageCleared(npcId, stage, nextOrder))
            {
                // Stage+1로 넘어가고, 새 Stage의 Order를 0으로 초기화
                PlayerProgress.SetStage(npcId, stage + 1);
                PlayerProgress.ResetOrder(npcId, stage + 1);
            }
        }
    }
}