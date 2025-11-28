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

    [Header("Data (debug only)")]
    [SerializeField] private DSDialogueContainerSO dialogueContainer;

    private Dictionary<string, DSDialogueSO> nodeLookup;

    private DSDialogueSO currentNode;
    private DialogueActor currentActor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    // NPC 쪽에서 컨테이너 바꿔줄 때 사용
    public void SetContainer(DSDialogueContainerSO container)
    {
        dialogueContainer = container;
        BuildNodeLookup();
    }

    private void BuildNodeLookup()
    {
        nodeLookup = new Dictionary<string, DSDialogueSO>();

        if (dialogueContainer == null)
        {
            Debug.LogWarning("DialogueManager: dialogueContainer is null");
            return;
        }

        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            foreach (var dialogue in pair.Value)
            {
                nodeLookup[dialogue.name] = dialogue;
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
            var button = Object.Instantiate(choiceButtonPrefab, choicesParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "다음";

            button.onClick.AddListener(() =>
            {
                if (currentNode.Choices.Count == 0)
                {
                    EndDialogue();
                    return;
                }

                var choice = currentNode.Choices[0];

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

                var button = Object.Instantiate(choiceButtonPrefab, choicesParent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = choiceData.Text;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() =>
                {
                    DSDialogueSO nextNode = null;

                    if (choiceData.NextDialogue != null)
                    {
                        nextNode = choiceData.NextDialogue;
                    }

                    currentNode = nextNode;
                    ShowCurrentNode();
                });
            }
        }

        // 애니메이션 재생
        if (currentNode.NpcAnimationClip != null && currentActor != null)
        {
            currentActor.PlayClip(currentNode.NpcAnimationClip);
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesParent)
        {
            Object.Destroy(child.gameObject);
        }
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        ClearChoices();

        // TODO: 필요하면 여기서 진행도 갱신 (예: NPC 스토리 stage 올리기)
        // PlayerProgress.Instance.SetNpcStoryStage(currentNpcId, newStage);
    }
}
