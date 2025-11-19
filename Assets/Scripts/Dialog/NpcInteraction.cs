using UnityEngine;
using UnityEngine.AI;


public class NpcInteraction : MonoBehaviour
{
    private NpcMovement movement;
    public NpcSO npcSO; 
    public bool isTalkable = false;
    private bool canTalk = false;    
    
    public void SetInteractionAvailable(bool available)
    {
        canTalk = available;
        RequestEndTalk();
    }

    public void RequestTalk(Player player, NpcMovement npc)
    {
        Debug.Log("Talk");
        
        if (!canTalk) return;      // 근처에 있어야 대화 가능
        if (isTalkable) return;     // 이미 대화 중이면 시작 X
        isTalkable = true;
        
        movement = npc;
        movement.StopWanderLoop(); 

        TryTalk(player.typer);
    }

    public void RequestEndTalk()
    {
        Debug.Log("EndTalk");
        
        if (!isTalkable) return;
        isTalkable = false;
        
        movement.StartWanderLoop();
        movement = null;
        UIManager.Singleton.DialogUI.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 플레이어가 이 NPC와 대화 시도할 때 호출
    /// 반환값이 false: 다음 대화 없음 / true : 다음 대사 있음
    /// </summary>
    public bool TryTalk(DialogTyper typer)
    {
        UIManager.Singleton.DialogUI.gameObject.SetActive(true);
        
        if (npcSO == null)
        {
            Debug.LogError("[NpcInteraction] npcSO is null.");
            return false;
        }

        int npcId = npcSO.BuilderId;
        int stage = PlayerProgress.GetStage(npcId);

        var line = DialogRepository.Singleton.PickNext(npcId, stage);
        if (line == null) return false;;
        
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
            if (DialogRepository.Singleton.IsStageCleared(npcId, stage, nextOrder))
            {
                // Stage+1로 넘어가고, 새 Stage의 Order를 0으로 초기화
                PlayerProgress.SetStage(npcId, stage + 1);
                PlayerProgress.ResetOrder(npcId, stage + 1);
            }
        }
        
        Debug.Log($"[NpcInteraction] 다음 대사 있음");
        return true;
    }
}