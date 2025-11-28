using System.Collections.Generic;
using DS.ScriptableObjects;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Database")]
public class DialogueDatabaseSO : ScriptableObject
{
    public List<DialogueMeta> entries = new List<DialogueMeta>();

    public bool TryGetDialogue(string npcId, int stage,
        out DSDialogueContainerSO container, out DSDialogueSO startNode)
    {
        container = null;
        startNode = null;

        DialogueMeta best = null;

        foreach (var e in entries)
        {
            if (e.npcId != npcId) continue;
            if (stage < e.minStage || stage > e.maxStage) continue;

            // 필요하면 우선순위 필드 추가해서 제일 좋은 것 고르기
            best = e;
        }

        if (best == null || best.container == null)
            return false;

        container = best.container;
        startNode = FindStartingNode(best.container);
        return startNode != null;
    }

    private DSDialogueSO FindStartingNode(DSDialogueContainerSO container)
    {
        // 컨테이너 안에서 IsStartingDialogue 체크
        foreach (var d in container.UngroupedDialogues)
            if (d.IsStartingDialogue) return d;

        foreach (var kv in container.DialogueGroups)
        foreach (var d in kv.Value)
            if (d.IsStartingDialogue) return d;

        // 못 찾으면 그냥 첫 번째라도 리턴(옵션)
        if (container.UngroupedDialogues.Count > 0)
            return container.UngroupedDialogues[0];

        foreach (var kv in container.DialogueGroups)
            if (kv.Value.Count > 0)
                return kv.Value[0];

        return null;
    }
}