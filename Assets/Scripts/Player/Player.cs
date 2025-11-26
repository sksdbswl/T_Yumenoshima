using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerInputHandler inputHandler;
    public IInteractable currentInteractable;

    // 플레이어 주변에 있는 모든 상호작용 대상들
    public List<IInteractable> interactablesInRange = new List<IInteractable>();

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

    // =======================
    // 상호작용 대상 관리 (겹치는 트리거 대응)
    // =======================

    /// <summary>
    /// 트리거에 들어온 IInteractable 등록
    /// </summary>
    public void AddInteractable(IInteractable target)
    {
        if (target == null) return;

        if (!interactablesInRange.Contains(target))
            interactablesInRange.Add(target);

        currentInteractable = ChooseBest(interactablesInRange);
    }

    /// <summary>
    /// 트리거에서 나간 IInteractable 제거
    /// </summary>
    public void RemoveInteractable(IInteractable target)
    {
        if (target == null) return;

        if (interactablesInRange.Remove(target))
        {
            currentInteractable = ChooseBest(interactablesInRange);
        }
    }

    /// <summary>
    /// 현재 범위 안의 interactable 중에서 우선순위가 가장 높은 것을 선택
    /// 문(씬 이동) > 건물 > 기타 순으로 우선순위
    /// </summary>
    private IInteractable ChooseBest(List<IInteractable> list)
    {
        if (list == null || list.Count == 0)
            return null;

        // 1) 문 상호작용이 있으면 가장 우선 (DoorInteraction)
        foreach (var it in list)
        {
            if (it is DoorInteraction) 
                return it;
        }

        // 2) 건물 상호작용 (PlaceableObject)
        foreach (var it in list)
        {
            if (it is PlaceableInteraction)
                return it;
        }

        // 3) 그 외에는 (NPC 등)
        return list[0];
    }
}
