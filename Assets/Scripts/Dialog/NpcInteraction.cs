using UnityEngine;

public class NpcInteraction : MonoBehaviour, IInteractable
{
    private NpcMovement movement;
    public NpcSO npcSO;
    public Player player;
    public bool isTalkable = false;
    private bool canTalk = false;

    public void Awake()
    {
        movement = GetComponent<NpcMovement>();
    }
    
    // =======================
    // 대화 구현부
    // =======================
    
    /// <summary>
    ///  setTrigger 설정
    /// </summary>
    /// <param name="available"></param>
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
    /// <param name="pl"></param>
    public void RequestTalk(Player pl)
    {
        
        Debug.Log("Talk");
        
        if (!canTalk) return;      // 근처에 있어야 대화 가능
        if (isTalkable) return;     // 이미 대화 중이면 시작 X
        isTalkable = true;
        player = pl;
        
        //movement = npc;
        
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
        //movement = null;
        player = null;
        DialogTyper.Singleton.DialogUI.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 실제 대화시작 : 플레이어가 이 NPC와 대화 시도할 때 호출
    /// 반환값이 false: 다음 대화 없음 / true : 다음 대사 있음
    /// </summary>
    public bool TryTalk()
    {
        DialogTyper.Singleton.DialogUI.gameObject.SetActive(true);
        
        if (npcSO == null)
        {
            Debug.LogError("[NpcInteraction] npcSO is null.");
            return false;
        }

        int npcId = npcSO.BuilderId;
        int stage = PlayerProgress.GetStage(npcId);

        var line = DialogRepository.Singleton.PickNext(npcId, stage);
        if (line == null) return false;;
        
        string speakerName = line.Speaker == "Player"
            ? "Player"        
            : npcSO.Name;     // NPC 이름 사용 (CSV의 NPC와 동일)

        // 대사 재생
        DialogTyper.Singleton.PlayLine(speakerName, line.Kor);
        
        // 스토리 진행 로직
        if (line.IsStory)
        {
            int nextOrder = line.Order + 1;
            // 아직 Stage 끝 아니면 Order만 증가
            PlayerProgress.SetOrder(npcId, stage, nextOrder);

            // Stage 끝났는지 체크
            if (DialogRepository.Singleton.IsStageCleared(npcId, stage, nextOrder))
            {
                // Stage+1로 넘어가고, 새 Stage의 Order를 0으로 초기화
                PlayerProgress.SetStage(npcId, stage + 1);
                PlayerProgress.ResetOrder(npcId, stage + 1);
            }
        }
        
        Debug.Log($"[NpcInteraction] 다음 대사 있음");
        return true;
    }
    
    // =======================
    // IInteractable 구현부
    // =======================

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