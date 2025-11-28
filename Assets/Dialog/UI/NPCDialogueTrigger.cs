using UnityEngine;
using DS.ScriptableObjects;

public class NPCDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DSDialogueContainerSO dialogueContainer;
    [SerializeField] private DSDialogueSO startingDialogue;
    private DialogueActor Actor;
    
    private bool playerInRange;

    private void Awake()
    {
        Actor = GetComponent<DialogueActor>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // "E키를 눌러 대화하기" 같은 UI 표시
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // 힌트 UI 끄기
        }
    }

    private void Update()
    {
        if (!playerInRange) return;

        // 예: E 키로 대화 시작
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 컨테이너를 DialogueManager에도 전달하고 싶으면
            //DialogueManager.Instance.SetContainer(dialogueContainer);

            DialogueManager.Instance.StartDialogue(startingDialogue , Actor);

            // 플레이어 움직임 잠그고 싶으면 여기에서 비활성화
        }
    }
}