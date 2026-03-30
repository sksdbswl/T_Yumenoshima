using Cysharp.Threading.Tasks;
using UnityEngine;

public class DoorInteraction : InteractionTarget, IInteractable
{
    public PlaceableInteraction Place { get; set; }
    
    public void CheckInteract(int stage)
    {
    }

    public void CheckInteract(RoutineState routine, Player player)
    {
       BeginInteract(player).Forget();
    }

    public async UniTask BeginInteract(Player player)
    {
        Debug.Log($"[Door] Building Interact: {Place.SourceItem?.DisplayName} (Role: {Place.Role})");
    }

    public void EndInteract(Player player)
    {
    }
}