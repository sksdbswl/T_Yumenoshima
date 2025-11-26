using UnityEngine;

public interface IInteractionData
{
    bool IsInteractable { get; set; }
    bool IsStackable { get; set; }

    Sprite GetThumbnail();
    string GetDescription();
    string GetContentId();
}