using UnityEngine;

public class NPCDialogueTrigger : InteractionTarget, IInteractable
{
    [SerializeField] private string npcId;
    [SerializeField] private DialogueActor actor;
    [SerializeField] private DialogueDatabaseSO database;

    private bool playerInRange;
    private IInteractable _interactableImplementation;

    public void CheckInteract(int stage)
    {
    }

    public void BeginInteract(Player player)
    {
        Debug.Log($"[NPC {npcId}] BeginInteract");

        if (database == null)
        {
            Debug.LogError($"[NPC {npcId}] database is null");
            return;
        }

        if (DialogueManager.Singleton == null)
        {
            Debug.LogError($"[NPC {npcId}] DialogueManager.Singleton is null");
            return;
        }

        if (!database.TryGetContainer(npcId, out var container))
        {
            Debug.LogWarning($"[NPC {npcId}] no container");
            return;
        }

        DialogueManager.Singleton.SetContainer(container);
        DialogueManager.Singleton.StartDialogueAuto(npcId);
    }

    public void EndInteract(Player player)
    {
    }
}

