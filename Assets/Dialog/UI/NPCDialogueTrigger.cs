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
        int stage = PlayerDialogueProgress.Instance.GetNpcStoryStage(npcId);

        if (!database.TryGetDialogue(npcId, stage,
                out var container, out var startNode))
        {
            Debug.Log($"[NPC {npcId}] no dialogue for stage {stage}");
            return;
        }

        
        DialogueManager.Instance.SetContainer(container);
        //DialogueManager.Instance.Progress = PlayerProgress.Instance; // 네 프로젝트 진행도 싱글톤
        DialogueManager.Instance.StartDialogueAuto(actor); // currentStage: 1~100
        
        // DialogueManager.Instance.SetContainer(container);
        // DialogueManager.Instance.StartDialogue(startNode, actor);
    }

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

