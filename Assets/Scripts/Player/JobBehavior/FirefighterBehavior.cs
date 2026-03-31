using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirefighterBehavior : IJobBehavior
{
    public Const.JobType JobType => Const.JobType.Firefighter;
    public bool CanInteract(IInteractable target)
    {
        if (target is not PlaceableInteraction placeable)
            return false;

        Debug.Log($"화재 진압 가능 여부 :: {placeable.IsOnFire}");
        return placeable.IsOnFire;
    }

     public async UniTask Execute(Player player, IInteractable target)
    {
        if (target is not PlaceableInteraction placeable)
            return;

        if (!placeable.IsOnFire)
            return;

        placeable.ExtinguishFire();

        // 연출 추가 
        await UniTask.Delay(0);
    }
}