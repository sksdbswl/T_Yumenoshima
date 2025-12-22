using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] private string npcId;
    [SerializeField] private DialogueActor actor;
    [SerializeField] private DialogueDatabaseSO database;

    private bool playerInRange;

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialogueAuto();
        }
    }

    private void StartDialogueAuto()
    {
        Debug.Log($"[NPC {npcId}] talk");

        if (!database.TryGetContainer(npcId, out var container))
        {
            Debug.Log($"[NPC {npcId}] no container");
            return;
        }

        DialogueManager.Singleton.SetContainer(container);
        DialogueManager.Singleton.StartDialogueAuto(npcId); // ✅ 월드 스테이지는 DialogueManager 내부 STAGE 사용
    }
    
    // private void StartDialogueAuto()
    // {
    //     int stage = PlayerDialogueProgress.Singleton.GetNpcStoryStage(npcId);
    //
    //     Debug.Log($"[NPC {npcId}] start dialogue for stage {stage}");
    //
    //     if (!database.TryGetDialogue(npcId, stage,
    //             out var container, out var startNode))
    //     {
    //         Debug.Log($"[NPC {npcId}] no dialogue for stage {stage}");
    //         return;
    //     }
    //     
    //     DialogueManager.Singleton.SetContainer(container);
    //     //DialogueManager.Instance.Progress = PlayerProgress.Instance; // 네 프로젝트 진행도 싱글톤
    //     DialogueManager.Singleton.StartDialogueAuto(npcId); // currentStage: 1~100
    //     
    //     // DialogueManager.Instance.SetContainer(container);
    //     // DialogueManager.Instance.StartDialogue(startNode, actor);
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}

