using System.Collections.Generic;
using DS.Enumerations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using DS.ScriptableObjects;  

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Data")]
    [SerializeField] private DSDialogueContainerSO dialogueContainer;

    // NodeID -> DSDialogueSO 매핑
    private Dictionary<string, DSDialogueSO> nodeLookup;

    private DSDialogueSO currentNode;
    private DialogueActor currentActor;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        BuildNodeLookup();
        dialoguePanel.SetActive(false);
    }

    private void BuildNodeLookup()
    {
        nodeLookup = new Dictionary<string, DSDialogueSO>();

        // 컨테이너 구조는 실제 프로젝트 SO 정의에 맞게 수정
        // 여기는 "그룹 + 비그룹" 둘 다에서 다이얼로그 긁어오는 예시
        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            foreach (var dialogue in pair.Value)
            {
                nodeLookup[dialogue.name] = dialogue;   // ID 필드가 따로 있으면 그걸 쓰면 됨
            }
        }

        foreach (var dialogue in dialogueContainer.UngroupedDialogues)
        {
            nodeLookup[dialogue.name] = dialogue;
        }
    }

    public void StartDialogue(DSDialogueSO startNode, DialogueActor actor)
    {
        currentActor = actor;
        currentNode = startNode;
        dialoguePanel.SetActive(true);
        ShowCurrentNode();
    }

    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        speakerText.text = currentNode.DialogueName;
        bodyText.text = currentNode.Text;

        ClearChoices();

        if (currentNode.DialogueType == DSDialogueType.SingleChoice)
        {
            // 다음 버튼 하나 생성해서 첫 번째 choice 따라가기
            var button = Instantiate(choiceButtonPrefab, choicesParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "다음";

            button.onClick.AddListener(() =>
            {
                // choice 리스트 중 첫 번째의 NodeID / NextDialogue로 이동
                if (currentNode.Choices.Count == 0)
                {
                    EndDialogue();
                    return;
                }

                var choice = currentNode.Choices[0];

                // 1) NodeID 문자열을 사용하는 경우
                // if (!string.IsNullOrEmpty(choice.NodeID) && nodeLookup.TryGetValue(choice.NodeID, out var nextNodeById))
                // {
                //     currentNode = nextNodeById;
                //     ShowCurrentNode();
                // }
                
                // 2) DSDialogueSO 참조를 직접 들고 있는 경우
                if (choice.NextDialogue != null)
                {
                    currentNode = choice.NextDialogue;
                    ShowCurrentNode();
                }
                else
                {
                    EndDialogue();
                }
            });
        }
        else // MultipleChoice
        {
            for (int i = 0; i < currentNode.Choices.Count; i++)
            {
                int index = i;
                var choiceData = currentNode.Choices[index];

                var button = Instantiate(choiceButtonPrefab, choicesParent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = choiceData.Text;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() =>
                {
                    // 선택한 choice의 다음 노드로 이동
                    DSDialogueSO nextNode = null;

                    // if (!string.IsNullOrEmpty(choiceData.NodeID))
                    // {
                    //     nodeLookup.TryGetValue(choiceData.NodeID, out nextNode);
                    // }
                    
                    if (choiceData.NextDialogue != null)
                    {
                        nextNode = choiceData.NextDialogue;
                    }

                    currentNode = nextNode;
                    ShowCurrentNode();
                });
            }
        }

        Debug.Log(
            $"Node '{currentNode.DialogueName}' ({currentNode.name}) has clip: " +
            $"{(currentNode.NpcAnimationClip != null ? currentNode.NpcAnimationClip.name : "NULL")}",
            currentNode
        );

        // 만약 클립이 있는 노드라면
        if (currentNode.NpcAnimationClip)
        {
            Debug.Log($"Playing clip {currentNode.NpcAnimationClip.name}");
            currentActor.PlayClip(currentNode.NpcAnimationClip);
        }
        else
        {
            Debug.Log($"No Playing clip");
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        ClearChoices();

        // 여기서 플레이어 컨트롤 다시 켜주거나,
        // NPC 상호작용 다시 가능하게 만드는 로직 등 넣으면 됨
    }
}
