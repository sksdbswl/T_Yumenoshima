using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerInputHandler inputHandler;
    public IInteractable currentInteractable;

    [Header("Player Settings")]
    bool Gather = false;
    bool Fishing = false;
    
    private void OnEnable()
    {
        inputHandler.OnInteract += HandleInteract;
        inputHandler.OnCancel   += HandleCancel;
    }
    
    private void OnDisable()
    {
        inputHandler.OnInteract -= HandleInteract;
        inputHandler.OnCancel   -= HandleCancel;
    }
    
    /// <summary>
    /// 상호작용 시작 / 진행 
    /// </summary>
    public void HandleInteract()
    {
        currentInteractable?.BeginInteract(this);
    }

    /// <summary>
    /// 상호작용 종료 (ESC 등)
    /// </summary>
    public void HandleCancel()
    {
        currentInteractable?.EndInteract(this);
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
}
