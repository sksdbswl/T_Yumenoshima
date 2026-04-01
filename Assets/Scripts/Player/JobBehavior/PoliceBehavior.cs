using Cysharp.Threading.Tasks;
using UnityEngine;

public class PoliceBehavior : IJobBehavior
{
    public Const.JobType JobType => Const.JobType.Police;
    public bool CanInteract(IInteractable target)
    {
        if (target is not NPCDialogueTrigger npcTrigger)
            return false;
        var npc = npcTrigger.GetComponent<Npc>();
        if (npc == null || npc.npcSO.Job != Const.JobType.Thief) return false;

        Debug.Log(" 도둑 제압 가능");
        return true;
    }

    public async UniTask Execute(Player player, IInteractable target)
    {
        await UniTask.Delay(500);
    }
}