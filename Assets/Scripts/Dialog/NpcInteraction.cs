using System;
using UnityEngine;

public class NpcInteraction : MonoBehaviour, IInteractable
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

        TryTalk();
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
    }
    
    /// <summary>
    /// 실제 대화시작 : 플레이어가 이 NPC와 대화 시도할 때 호출
    /// 반환값이 false: 다음 대화 없음 / true : 다음 대사 있음
    /// </summary>
    public bool TryTalk()
    {
        if (npcSO == null)
        {
            Debug.LogError("[NpcInteraction] npcSO is null.");
            return false;
        }
        
        DialogTyper.Singleton.DialogUI.gameObject.SetActive(true);

        int npcId = npcSO.BuilderId;

        // 1) 월드(메인) 스테이지 : GameManager에서 관리
        // TODO: GameManager의 실제 필드/프로퍼티 이름에 맞게 수정 (예: CurrentStage, WorldStage 등)
        int worldStage = GameManager.Singleton.Stage;

        // 2) NPC 개인 스토리 스테이지 : PlayerProgress에서 관리
        int npcStoryStage = PlayerProgress.GetNpcStoryStage(npcId);

        // 3) Repository에서 다음 대사 한 줄 가져오기
        var line = DialogRepository.Singleton.PickNext(npcId, worldStage, npcStoryStage);
        if (line == null) return false;
        
        string speakerName = line.Speaker == "Player"
            ? "Player"
            : npcSO.Name; // CSV의 NPC와 동일한 이름 사용

        // 대사 재생
        DialogTyper.Singleton.PlayLine(speakerName, line.Kor);
        
        // 스토리 진행 로직 (NPC 스토리 기준)
        if (line.IsStory)
        {
            int nextOrder = line.Order + 1;

            // 아직 이 NPC 스토리 Stage 안이라면 Order만 증가
            PlayerProgress.SetOrder(npcId, npcStoryStage, nextOrder);

            // 이 NPC 스토리 Stage가 끝났는지 체크
            if (DialogRepository.Singleton.IsStageCleared(npcId, npcStoryStage, nextOrder))
            {
                int nextNpcStoryStage = npcStoryStage + 1;

                PlayerProgress.SetNpcStoryStage(npcId, nextNpcStoryStage);
                PlayerProgress.ResetOrder(npcId, nextNpcStoryStage);
                
                // 이 챕터 클리어 → 보상 체크
                StoryRewardManager.Singleton.TryGrantStoryReward(npcId, npcStoryStage);
            }
        }
        
        Debug.Log($"[NpcInteraction] 다음 대사 있음");
        return true;
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
        
        // 초기에 npc별 마크 할당
        if (hasStoryQuest && questMarkerUI == null)
        {
            var marker = DialogRepository.Singleton.SpawnMarker();
            questMarkerUI = marker;
        }
        
        // 머리 위 UI 토글
        questMarkerUI.SetQuestActive(hasStoryQuest);
    }
        
    /// <summary>
    /// 상호작용 시작 / 대화 한 줄 진행
    /// Player.OnInteractPerformed 에서 호출
    /// </summary>
    public void BeginInteract(Player player)
    {
        if (isTalkable)
        {
            bool hasNext = TryTalk();
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

        RequestEndTalk();
    }
}
