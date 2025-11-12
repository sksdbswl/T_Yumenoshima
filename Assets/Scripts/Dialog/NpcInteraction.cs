using Unity.VisualScripting;
using UnityEngine;

public class NpcInteraction : MonoBehaviour
{
    public Npc npcSO;

    public void TryTalk(Npc cfg, DialogTyper typer)
    {
        var npcId = cfg.Id;
        var stage = PlayerProgress.GetStage(npcId);

        // 있나 체크
        if (!DialogRepository.I.HasStory(npcId, stage) &&
            !DialogRepository.I.HasAmbient(npcId))
            return;

        // 하나 뽑기
        var line = DialogRepository.I.PickNext(npcId, stage);
        if (line == null) return;

        // 재생
        typer.PlayLine(cfg.Name, line.Kor);

        // 스토리면 소진/승급
        if (line.IsStory)
        {
            PlayerProgress.MarkStorySeen(line.Key);
            if (DialogRepository.I.IsStageCleared(npcId, stage))
                PlayerProgress.SetStage(npcId, stage + 1);
        }
    }
}
