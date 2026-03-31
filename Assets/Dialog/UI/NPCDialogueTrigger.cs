using AI.BT.Runtime;
using Cysharp.Threading.Tasks;
using TestBT;
using UnityEngine;

public class NPCDialogueTrigger : InteractionTarget, IInteractable
{
    [HideInInspector] public NpcSO npcSO;
    [SerializeField] private string npcId;
    [SerializeField] private DialogueActor actor;
    [SerializeField] private DialogueDatabaseSO database;

    private bool playerInRange;
    private IInteractable _interactable;

    public async void CheckInteract(RoutineState routine, Player player)
    {
        if (routine == RoutineState.Morning)
        {
            if (player._playerStatus.CurrentJobType == Const.JobType.None)
            {
                // 직업선택 가능
                player._playerStatus.ChangeJob(npcSO.Job);
                
                // get 연출 추가
                var presenter = await UIManager.Singleton.GetUI<PlayerHUD>(UIList.PlayerHUD);
                presenter.ShowEffect(Const.EEffect.Change, player);
                
                // 직업있는 npc bt 일반 citizen npc로 전환
                var controller = this.gameObject.GetComponent<SimulationNpcController>();
                controller.ChangeCitizenTree();
                
                // ui update
                Dashboard.Singleton.Init();
            } 
        }  
        else if (routine == RoutineState.Noon)
        {
            BeginInteract(player);
        }
    }

    public async UniTask BeginInteract(Player player)
    {
        if (!database.TryGetContainer(npcSO.name, out var container))
        {
            Debug.LogWarning($"[NPC {npcSO.name}] no container");
            return;
        }
        
        var dialog = await UIManager.Show<DialogueUI>(UIList.DialogueUI);

        var executor = gameObject.GetComponent<SimulationNpcExecutor>();
        
        dialog.SetCurrentNpc(executor);
        dialog.SetContainer(container);
        dialog.StartDialogueAuto(npcSO.name);
    }

    public void EndInteract(Player player)
    {
    }
}

