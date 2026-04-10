using Cysharp.Threading.Tasks;
using UnityEngine;

public class DoctorBehavior : IJobBehavior
{
    public Const.JobType JobType => Const.JobType.Doctor;

    public bool CanInteract(Player player, IInteractable target)
    {
        if (target is not NPCDialogueTrigger npcTrigger)
            return false;

        var npc = npcTrigger.GetComponent<Npc>();
        if (npc == null) return false;

        Debug.Log($"치료가능 :: {npc._npcStatus.CurrentEmotion == Const.EEmotion.Tired}");

        // todo :: player state emotion 변경  -> 번경 시 이모션 변경관련 컴포넌트 추가해서 처리할 것
        // EmotionManager.Instance.ShowEmotion(
        //     Const.EEmotion.Exclamation,
        //     player.transform,
        //     Vector3.up * 1.5f
        // );
        
        return npc._npcStatus.CurrentEmotion == Const.EEmotion.Tired;
    }

    public async UniTask Execute(Player player, IInteractable target)
    {
        if (target is not NPCDialogueTrigger npcTrigger)
            return;

        var npc = npcTrigger.GetComponent<Npc>();
        if (npc == null)
            return;

        npc._npcStatus.ChangeEmotion(npc, Const.EEmotion.Neutral);
        npc.executor.isAnomaly = false;
        npc.agent.isStopped = false;
        EmotionManager.Instance.ReturnEmotion(npc.executor.currentEmotionIcon);

        Debug.Log($"{npc.name} 치료 완료");

        await UniTask.CompletedTask;
    }
}