using UnityEngine;

public class InteractionTarget : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<Player>();
        if (p == null) return;

        var interactable = gameObject.GetComponent<IInteractable>();
        if (interactable == null) return;

        p.AddInteractable(interactable);
    }

    private void OnTriggerExit(Collider other)
    {
        var p = other.GetComponent<Player>();
        if (p == null) return;

        var interactable = gameObject.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (p.currentInteractable == interactable)
        {
            p.HandleCancel(); 
        }

        p.RemoveInteractable(interactable);
    }
}