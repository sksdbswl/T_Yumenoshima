using UnityEngine;

public class DoorInteraction :InteractionTarget, IInteractable
{
    public PlaceableInteraction Place { get; set; }
    
    public void CheckInteract(int stage)
    {
    }

    public void BeginInteract(Player player)
    {
        Debug.Log($"[Door] Building Interact: {Place.SourceItem?.DisplayName} (Role: {Place.Role})");
    }

    public void EndInteract(Player player)
    {
    }
}