using UnityEngine;

public class Player : MonoBehaviour
{
    bool isTalking = false;
    NpcInteraction currentNpc;
    [SerializeField] private DialogTyper typer;

    void Update()
    {
        // E 키: 현재 NPC와 대화 시도
        if (isTalking && currentNpc != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("대화 시작");
            currentNpc.TryTalk(typer);
        }

        // TODO :: UI 온오프 적용 필요
        // ESC: 대화 종료 + 스토리 Order 리셋
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentNpc != null && currentNpc.npcSO != null)
            {
                OnDialogClosed(currentNpc.npcSO);
                currentNpc = null;
            }

            isTalking = false;
        }
    }

    public void OnDialogClosed(NpcSO cfg)
    {
        int npcId = cfg.Id;
        int stage = PlayerProgress.GetStage(npcId);

        // ESC로 종료 시, 현재 Stage의 진행중이던 Order를 0으로 리셋
        PlayerProgress.ResetOrder(npcId, stage);
    }

    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<NpcInteraction>();
        if (npc != null)
        {
            currentNpc = npc;
            Debug.Log($"Enter NPC: {currentNpc.npcSO.Name}");

            if (!isTalking)
            {
                isTalking = true;
                Debug.Log("Talking ON");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var npc = other.GetComponent<NpcInteraction>();
        if (npc != null && npc == currentNpc)
        {
            currentNpc = null;
            isTalking = false;
            Debug.Log("Talking OFF");
        }
    }
}