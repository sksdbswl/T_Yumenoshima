using UnityEngine;

public class InteractionTarget : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<Player>();
        if (p == null) return;

        p.currentInteractable = gameObject.GetComponent<IInteractable>();
    }

    private void OnTriggerExit(Collider other)
    {
        var p = other.GetComponent<Player>();
        if (p == null) return;

        p.HandleCancel();
    }
}