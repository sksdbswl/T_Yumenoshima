using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        
        var dialog = await UIManager.Singleton.GetUI<DialogueUI>(UIList.DialogueUI);

        dialog.SetContainer(container);
        dialog.StartDialogueAuto(npcId);
    }
    
    // public void BeginInteract(Player player)
    // {
    //     Debug.Log($"[NPC {npcId}] BeginInteract");
    //
    //     if (database == null)
    //     {
    //         Debug.LogError($"[NPC {npcId}] database is null");
    //         return;
    //     }
    //
    //     if (!database.TryGetContainer(npcId, out var container))
    //     {
    //         Debug.LogWarning($"[NPC {npcId}] no container");
    //         return;
    //     }
    //
    //
    //     var dialog = UIManager.Show<DialogueUI>(UIList.DialogueUI);
    //     
    //     dialog.SetContainer(container);
    //     dialog.StartDialogueAuto(npcId);
    // }

    public void EndInteract(Player player)
    {
    }
}

