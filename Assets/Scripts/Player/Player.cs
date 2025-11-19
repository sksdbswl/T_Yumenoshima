using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] public InputActionReference moveAction;
    [SerializeField] public InputActionReference jumpAction;
    [SerializeField] public InputActionReference interactAction;
    [SerializeField] public InputActionReference cancelAction;

    private IInteractable currentInteractable;

    /// <summary>
    /// 상호작용 시작 / 진행 
    /// </summary>
    public void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;

        Debug.Log("Interact");
        // 여기서는 단순히 상호작용 요청만 처리
        currentInteractable.BeginInteract(this);
    }

    /// <summary>
    /// 상호작용 종료 (ESC 등)
    /// </summary>
    public void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        if (currentInteractable == null) return;

        currentInteractable.EndInteract(this);
    }

    /// <summary>
    /// 대화 진행도 리셋 : npc 대화 강제 종료시 사용
    /// </summary>
    public void OnDialogClosed(NpcSO cfg)
    {
        int npcId = cfg.BuilderId;
        int stage = PlayerProgress.GetStage(npcId);

        // ESC로 종료 시, 현재 Stage의 진행중이던 Order를 0으로 리셋
        PlayerProgress.ResetOrder(npcId, stage);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 상호작용 가능한 모든 IInteractable 처리
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && currentInteractable == null)
        {
            currentInteractable = interactable;

            // NPC라면
            if (interactable is NpcInteraction npc)
            {
                npc.SetInteractionAvailable(true);  // 플레이어가 근처에 있다
                Debug.Log($"Enter NPC: {npc.npcSO.Name}");
            }

            // TODO:: 다른 타입의 상호작용 오브젝트도 여기에 분기 가능
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (interactable == currentInteractable)
        {
            // NPC라면
            if (interactable is NpcInteraction npc)
            {
                npc.SetInteractionAvailable(false); // 플레이어가 멀어졌다
                Debug.Log("Talking OFF");
            }

            currentInteractable = null;
        }

        // TODO:: 다른 상호작용 오브젝트 처리
    }
}
