using Cysharp.Threading.Tasks;
using TestBT;
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

    public async UniTask BeginInteract(Player player)
    {
        if (!database.TryGetContainer(npcId, out var container))
        {
            Debug.LogWarning($"[NPC {npcId}] no container");
            return;
        }
        
        var dialog = await UIManager.Show<DialogueUI>(UIList.DialogueUI);

        var executor = gameObject.GetComponent<SimulationNpcExecutor>();
        
        dialog.SetCurrentNpc(executor);
        dialog.SetContainer(container);
        dialog.StartDialogueAuto(npcId);
    }

    public void EndInteract(Player player)
    {
    }
}

