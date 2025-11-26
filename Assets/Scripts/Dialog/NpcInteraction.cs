using System;
using UnityEngine;

public class NpcInteraction : InteractionTarget, IInteractable
{
    private NpcMovement movement;
    public NpcSO npcSO;
    public Player player;
    public bool isTalkable = false;
    private bool canTalk = false;
    private QuestMarkerUI questMarkerUI;
    
    public void Awake()
    {
        movement = GetComponent<NpcMovement>();
        questMarkerUI = DialogRepository.Singleton.SpawnMarker();
        questMarkerUI.target = transform;
        GameManager.Singleton.OnStageChanged += CheckInteract;
    }

    public void Start()
    {
        // 초기에만 스테이지 확인
        CheckInteract(GameManager.Singleton.Stage);
    }

    // =======================
    // 대화 구현부
    // =======================
    
    /// <summary>
    ///  setTrigger 설정
    /// </summary>
    public void SetInteractionAvailable(bool available)
    {
        canTalk = available;

        if (available) return;
        
        player?.OnDialogClosed(npcSO);
        RequestEndTalk();
    }

    /// <summary>
    /// 대화 시작 설정
    /// </summary>
    public void RequestTalk(Player pl)
    {
        Debug.Log("Talk");
        
        if (!canTalk) return;      // 근처에 있어야 대화 가능
        if (isTalkable) return;    // 이미 대화 중이면 시작 X
        isTalkable = true;
        player = pl;
        
        Debug.Log($"==========[NpcInteraction] RequestTalk: {pl.name}, {movement}");
        movement.StopWanderLoop(); 

        npcSO.TryTalk();
    }

    /// <summary>
    /// 대화 종료 설정
    /// </summary>
    public void RequestEndTalk()
    {
        Debug.Log("EndTalk");
        
        if (!isTalkable) return;
        isTalkable = false;
        
        movement.StartWanderLoop();
        player = null;
        DialogTyper.Singleton.DialogUI.gameObject.SetActive(false);
        CheckInteract(GameManager.Singleton.Stage);
    }
    
    // =======================
    // IInteractable 구현부
    // =======================

    /// <summary>
    /// 상호작용 시작전 스토리 진행 퀘스트 여부 확인
    /// </summary>
    public void CheckInteract(int stage)
    {
        if (npcSO == null)
            return;

        int npcId = npcSO.BuilderId;
        int worldStage = stage;

        // 플레이어가 이 NPC에 대해 어디까지 진행했는지
        int npcStoryStage = PlayerProgress.GetNpcStoryStage(npcId);
        int currentOrder   = PlayerProgress.GetOrder(npcId, npcStoryStage); 

        // 현재 월드/스토리 진행도 기준으로
        // 남아 있는 스토리 대사가 있는지 체크
        bool hasStoryQuest = DialogRepository
            .Singleton
            .HasStoryQuest(npcId, worldStage, npcStoryStage, currentOrder);

        Debug.Log($"[NpcInteraction] CheckInteract: {hasStoryQuest}");
        
        // 머리 위 UI 토글
        questMarkerUI.SetQuestActive(hasStoryQuest);
    }
        
    /// <summary>
    /// 상호작용 시작 / 대화 한 줄 진행
    /// Player.OnInteractPerformed 에서 호출
    /// </summary>
    public void BeginInteract(Player player)
    {
        SetInteractionAvailable(true); 
        
        if (isTalkable)
        {
            bool hasNext = npcSO.TryTalk();
            if (!hasNext)
            {
                Debug.Log("[NpcInteraction] 다음 대사 없음, 대화 종료함");
                RequestEndTalk();
            }
        }
        else
        {
            // 첫 대화 시작
            RequestTalk(player);
        }
    }
    
    public void ContinueInteract(Player player) {}

    /// <summary>
    /// 상호작용 강제 종료 (ESC 등)
    /// Player.OnInteractCanceled 에서 호출
    /// </summary>
    public void EndInteract(Player player)
    {
        Debug.Log("Interact 강제 종료");

        if (npcSO != null)
            player.OnDialogClosed(npcSO); 

        SetInteractionAvailable(false);
        player.currentInteractable = null;
        
        RequestEndTalk();
    }
}
