using UnityEngine;

public class Player : MonoBehaviour
{
    bool isTalking = false;
    NpcInteraction currentNpc;
    [SerializeField] private DialogTyper typer;
    

    // Update is called once per frame
    void Update()
    {
        if (isTalking && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("대화 시작");
            currentNpc.TryTalk(currentNpc.npcSO,typer);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isTalking = false;
            OnDialogClosed(currentNpc.npcSO);
        }
    }
    
    public void OnDialogClosed(NpcSO cfg)
    {
        int npcId = cfg.Id;
        int stage = PlayerProgress.GetStage(npcId);

        PlayerProgress.ResetOrder(npcId, stage);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var npc = other.GetComponent<NpcInteraction>();
        if (npc != null)
        {
            currentNpc = npc;                
            Debug.Log(currentNpc.npcSO.Name);

            if (!isTalking)
            {
                Debug.Log("use Talking");
                isTalking = true;
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
        }
    }
}
