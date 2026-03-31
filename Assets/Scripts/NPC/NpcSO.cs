using AI.BT.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Npc/NpcRole", fileName = "Npc")]
public class NpcSO : ScriptableObject
{
    /// <summary>
    /// BuilderId
    /// 0~10 : 특정 직업 Npc 집
    /// 11~90 : 일반 데코 오브젝트
    /// 100~n : 기본 마을 사람들 집
    /// 아이디 중복 불가, Npc마다 각 고유의 집을 보유해야함 
    /// </summary>
    public int Id;
    public int BuilderId;
    public string Name;
    public Const.JobType Job;
    //public GameObject Prefab;
    public Sprite Thumbnail;
    public Vector3 spawnPoint;

    public int WorldStageMin = 1;
    public int WorldStageMax = 1;
    
    public float defaultSpeed = 0f;
    public float runSpeed = 0f;
    
    public bool canChase;
    public bool canAttack;
    public bool canFlee;
    public bool canSteal;
    
    public BTGraphAsset jobBT;
    public BTGraphAsset citizenBT;
    
    /// <summary>
    /// 실제 대화시작 : 플레이어가 이 NPC와 대화 시도할 때 호출
    /// 반환값이 false: 다음 대화 없음 / true : 다음 대사 있음
    /// </summary>
    // public bool TryTalk()
    // {
    //     DialogTyper.Singleton.DialogUI.gameObject.SetActive(true);
    //
    //     int npcId = BuilderId;
    //
    //     // 1) 월드(메인) 스테이지 : GameManager에서 관리
    //     // TODO: GameManager의 실제 필드/프로퍼티 이름에 맞게 수정 (예: CurrentStage, WorldStage 등)
    //     int worldStage = GameManager.Singleton.Stage;
    //
    //     // 2) NPC 개인 스토리 스테이지 : PlayerProgress에서 관리
    //     int npcStoryStage = PlayerProgress.GetNpcStoryStage(npcId);
    //
    //     // 3) Repository에서 다음 대사 한 줄 가져오기
    //     var line = DialogRepository.Singleton.PickNext(npcId, worldStage, npcStoryStage);
    //     if (line == null) return false;
    //     
    //     string speakerName = line.Speaker == "Player"
    //         ? "Player"
    //         : Name; // CSV의 NPC와 동일한 이름 사용
    //
    //     // 대사 재생
    //     DialogTyper.Singleton.PlayLine(speakerName, line.Kor);
    //     
    //     // 스토리 진행 로직 (NPC 스토리 기준)
    //     if (line.IsStory)
    //     {
    //         int nextOrder = line.Order + 1;
    //
    //         // 아직 이 NPC 스토리 Stage 안이라면 Order만 증가
    //         PlayerProgress.SetOrder(npcId, npcStoryStage, nextOrder);
    //
    //         // 이 NPC 스토리 Stage가 끝났는지 체크
    //         if (DialogRepository.Singleton.IsStageCleared(npcId, npcStoryStage, nextOrder))
    //         {
    //             int nextNpcStoryStage = npcStoryStage + 1;
    //
    //             PlayerProgress.SetNpcStoryStage(npcId, nextNpcStoryStage);
    //             PlayerProgress.ResetOrder(npcId, nextNpcStoryStage);
    //             
    //             // 이 챕터 클리어 → 보상 체크
    //             StoryRewardManager.Singleton.TryGrantStoryReward(npcId, npcStoryStage);
    //         }
    //     }
    //     
    //     Debug.Log($"[NpcInteraction] 다음 대사 있음");
    //     return true;
    // }
}