using UnityEngine;

public class Player : MonoBehaviour
{
    NpcMovement currentNpc;

    void Update()
    {
        // E: 대화 요청
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentNpc.isTalkable)
            {
                // 다음 대화
                var checkTalk = currentNpc.TryTalk();

                if (!checkTalk)
                {
                    Debug.Log($"[NpcInteraction] 다음 대사 없음으로 대화 종료함");
                    currentNpc.RequestEndTalk();
                }
            }
            else
            {
                // 첫 대화 시작
                if (currentNpc != null)
                    currentNpc.RequestTalk(this, currentNpc);
            }
        }
        
        // ESC: 대화 강제 종료 + 스토리 Order 리셋
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentNpc != null)
            {
                // 스토리 진행 중이던 거 처음부터 시작하도록 리셋
                if (currentNpc.npcSO != null)
                    OnDialogClosed(currentNpc.npcSO);

                currentNpc.RequestEndTalk();
                // 필요하다면 여기서 currentNpc = null; 할지 말지는 너 구조에 따라 결정
            }
        }
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
        var npc = other.GetComponent<NpcMovement>();
        if (npc != null)
        {
            currentNpc = npc;
            npc.SetInteractionAvailable(true);  // 플레이어가 근처에 있다
            Debug.Log($"Enter NPC: {npc.npcSO.Name}");
        }
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
    }

}
