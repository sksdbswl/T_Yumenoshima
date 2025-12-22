using System.Collections.Generic;
using DS.ScriptableObjects;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Database")]
public class DialogueDatabaseSO : ScriptableObject
{
    public List<DialogueMeta> entries = new List<DialogueMeta>();

    // 기존 방식 (stage 기반 startNode 선택) – 이제 안 씀
    public bool TryGetDialogue(
        string npcId,
        int stage,
        out DSDialogueContainerSO container,
        out DSDialogueSO startNode)
    {
        container = null;
        startNode = null;

        DialogueMeta best = null;

        foreach (var e in entries)
        {
            if (e.npcId != npcId) continue;
            if (stage < e.minStage || stage > e.maxStage) continue;

            best = e;
        }

        if (best == null || best.container == null)
            return false;

        container = best.container;
        startNode = FindStartingNode(best.container);
        return startNode != null;
    }

    // 새 방식: npcId로 컨테이너만 반환
    public bool TryGetContainer(string npcId, out DSDialogueContainerSO container)
    {
        container = null;

        foreach (var e in entries)
        {
            if (e.npcId != npcId) continue;
            if (e.container == null) continue;

            container = e.container;
            return true;
        }

        return false;
    }

    private DSDialogueSO FindStartingNode(DSDialogueContainerSO container)
    {
        foreach (var d in container.UngroupedDialogues)
            if (d.IsStartingDialogue) return d;

        foreach (var kv in container.DialogueGroups)
        foreach (var d in kv.Value)
            if (d.IsStartingDialogue) return d;

        return null;
    }
}