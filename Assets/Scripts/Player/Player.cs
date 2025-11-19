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
    
    NpcMovement currentNpc;
    readonly List<NpcMovement> nearbyNpcs = new List<NpcMovement>();
    
    /// <summary>
    /// 상호작용 시작 : npc대화, item 상호작용 등
    /// </summary>
    public void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (currentNpc == null) return;
        
        if (currentNpc.isTalkable)
        {
            bool hasNext = currentNpc.TryTalk();
            if (!hasNext)
            {
                Debug.Log("[NpcInteraction] 다음 대사 없음으로 대화 종료함");
                currentNpc.RequestEndTalk();
            }
        }
        else
        {
            // 첫 대화 시작
            currentNpc.RequestTalk(this, currentNpc);
        }
    }

    /// <summary>
    /// 상호작용 종료
    /// </summary>
    public void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact 강제 종료");
        
        if (currentNpc == null)
            return;

        if (currentNpc.npcSO != null)
            OnDialogClosed(currentNpc.npcSO);

        currentNpc.RequestEndTalk();
    }

    
    public void OnDialogClosed(NpcSO cfg)
    {
        int npcId = cfg.BuilderId;
        int stage = PlayerProgress.GetStage(npcId);

        // ESC로 종료 시, 현재 Stage의 진행중이던 Order를 0으로 리셋
        PlayerProgress.ResetOrder(npcId, stage);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // npc 처리
        var npc = other.GetComponent<NpcMovement>();
        if (npc != null && currentNpc == null)
        {
            currentNpc = npc;
            
            npc.SetInteractionAvailable(true);  // 플레이어가 근처에 있다
            Debug.Log($"Enter NPC: {npc.npcSO.Name}");
        }
        // Todo:: 상호작용 아이템 처리
    }
    
    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponent<NpcMovement>();
        if (npc != null && npc == currentNpc)
        {
            npc.SetInteractionAvailable(false); // 플레이어가 멀어졌다
            
            currentNpc = null;
            Debug.Log("Talking OFF");
        }

        // TODO:: 상호작용 아이템 처리
    }
}
