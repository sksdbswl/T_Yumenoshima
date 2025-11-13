using Unity.VisualScripting;
using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    public int id = 0;
    public NpcSO npcSO;

    void Awake()
    {
        var data = AssetManager.Singleton.GetNpcDataSO(); 
        if (!data.Items.TryGetValue(id, out npcSO))
        {
            Debug.LogError($"[NpcInteraction] NPC id {id} not found in NpcDataSO");
            npcSO = data.Items[id];
        }
    }
    
    public void TryTalk(NpcSO cfg, DialogTyper typer)
    {
        int npcId = cfg.Id;
        int stage = PlayerProgress.GetStage(npcId);

        var line = DialogRepository.I.PickNext(npcId, stage);
        if (line == null) return;

        typer.PlayLine(cfg.Name, line.Kor);

        if (line.IsStory)
        {
            // 다음 순서로 진행
            int nextOrder = line.Order + 1;
            PlayerProgress.SetOrder(npcId, stage, nextOrder);

            // 마지막 스토리면 Stage++ 하고 Order 초기화
            if (DialogRepository.I.IsStageCleared(npcId, stage, nextOrder))
            {
                PlayerProgress.SetStage(npcId, stage + 1);
                PlayerProgress.ResetOrder(npcId, stage + 1);
            }
        }
    }

    
    // public void TryTalk(NpcSO cfg, DialogTyper typer)
    // {
    //     var npcId = cfg.Id;
    //     var stage = PlayerProgress.GetStage(npcId);
    //
    //     // 있나 체크
    //     if (!DialogRepository.I.HasStory(npcId, stage) &&
    //         !DialogRepository.I.HasAmbient(npcId))
    //         return;
    //
    //     // 하나 뽑기
    //     var line = DialogRepository.I.PickNext(npcId, stage);
    //     if (line == null) return;
    //
    //     // 재생
    //     typer.PlayLine(cfg.Name, line.Kor);
    //
    //     // 스토리면 소진/승급
    //     if (line.IsStory)
    //     {
    //         PlayerProgress.MarkStorySeen(line.Key);
    //         if (DialogRepository.I.IsStageCleared(npcId, stage))
    //             PlayerProgress.SetStage(npcId, stage + 1);
    //     }
    // }
}
