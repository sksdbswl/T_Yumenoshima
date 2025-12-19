using System;
using System.Collections.Generic;
using DS.Enumerations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DS.ScriptableObjects;

public class DialogueManager : SingletonBase<DialogueManager>
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Data (debug only)")]
    [SerializeField] private DSDialogueContainerSO dialogueContainer;

    // 임시: 나중에 GameManager에서 받아올 예정
    private const int STAGE = 3;

    [Header("Chapter Rules")]
    [SerializeField] private List<ChapterRule> chapterRules = new();

    // 런타임 상태
    private DSDialogueSO currentNode;
    private DialogueActor currentActor;

    // 캐시(성능)
    private bool built;
    private Dictionary<DSDialogueSO, int> nodeGroupIndex; // 노드 -> 그룹 인덱스 (ungrouped는 -1)
    private DSDialogueSO[] groupStartNode;                // 그룹 인덱스 -> 시작 노드
    private ChapterRule[] rulesByGroupIndex;              // 그룹 인덱스 -> 룰
    private string[] groupNameByIndex;                    // 그룹 인덱스 -> 그룹명
    private Dictionary<string, int> chapterIdToGroupIndex; 

    public IGameProgress Progress { get; set; } 

    [Serializable]
    public class ChapterRule
    {
        public string groupName;      // DialogueGroupSO.GroupName 과 동일
        public string chapterId;      // 예: "CH1"
        [Range(1, 100)] public int minStage = 1;
        [Range(1, 100)] public int maxStage = 100;
        public string prerequisiteChapterId; // 예: "CH1"
    }
    
    private void Awake()
    {
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// npc 다이얼로그 전체 저장 : 대화 그래프 전체(파일 1개)
    /// </summary>
    public void SetContainer(DSDialogueContainerSO container)
    {
        dialogueContainer = container;
        built = false; 
    }

    /// <summary>
    /// 말 걸 때: 현재 stage(STAGE=1) 기준으로 시작 노드를 자동 선택해서 시작
    /// </summary>
    public void StartDialogueAuto(DialogueActor actor)
    {
        if (dialogueContainer == null)
        {
            Debug.LogWarning("DialogueManager: dialogueContainer is null");
            return;
        }

        BuildCacheIfNeeded();

        currentActor = actor;
        currentNode = ResolveStartNodeForStage(STAGE);

        dialoguePanel.SetActive(true);
        ShowCurrentNode();
    }

    // =========================
    // Cache Build (한 번만) 
    // =========================
    private void BuildCacheIfNeeded()
    {
        if (built) return;

        // 컨테이너 내 그룹을 배열 인덱스로 관리 (foreach에서 순서 고정)
        int groupCount = dialogueContainer.DialogueGroups.Count;

        nodeGroupIndex = new Dictionary<DSDialogueSO, int>(128);
        groupStartNode = new DSDialogueSO[groupCount];
        rulesByGroupIndex = new ChapterRule[groupCount];
        groupNameByIndex = new string[groupCount];

        // groupName -> groupIndex
        var groupIndexByName = new Dictionary<string, int>(groupCount, StringComparer.Ordinal);

        int gi = 0;

        // 1) 그룹들 순회하면서: 그룹명/시작노드/노드->그룹 매핑 구축
        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            var groupSO = pair.Key;
            var list = pair.Value;

            string gName = (groupSO != null) ? groupSO.GroupName : string.Empty;
            groupNameByIndex[gi] = gName;

            if (!string.IsNullOrEmpty(gName))
                groupIndexByName[gName] = gi;

            // 시작 노드 찾기 + 노드->그룹 인덱스
            DSDialogueSO start = null;

            for (int i = 0; i < list.Count; i++)
            {
                var node = list[i];
                if (node == null) continue;

                nodeGroupIndex[node] = gi;

                if (start == null && node.IsStartingDialogue)
                    start = node;
            }

            // 시작 플래그가 없으면 첫 노드로 fallback
            if (start == null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null) { start = list[i]; break; }
                }
            }

            groupStartNode[gi] = start;
            gi++;
        }

        // 2) ungrouped 노드 등록 (그룹 인덱스 = -1)
        for (int i = 0; i < dialogueContainer.UngroupedDialogues.Count; i++)
        {
            var node = dialogueContainer.UngroupedDialogues[i];
            if (node == null) continue;
            nodeGroupIndex[node] = -1;
        }

        // 3) 룰을 groupIndex에 매핑 (룰 없는 그룹은 null)
        chapterIdToGroupIndex = new Dictionary<string, int>(chapterRules.Count, StringComparer.Ordinal);
        
        for (int i = 0; i < chapterRules.Count; i++)
        {
            var r = chapterRules[i];
            if (r == null || string.IsNullOrEmpty(r.groupName)) continue;

            if (groupIndexByName.TryGetValue(r.groupName, out int idx))
            {
                rulesByGroupIndex[idx] = r;
                if (!string.IsNullOrEmpty(r.chapterId))
                    chapterIdToGroupIndex[r.chapterId] = idx;
            }
        }

        built = true;
    }

    // =========================
    // Start Node Resolve
    // =========================
    
    private DSDialogueSO ResolveStartNodeForStage(int stage)
    {
        // 정책:
        // - stage 조건 맞는 그룹 중 "가장 뒤(인덱스 큰 쪽)"부터 찾는다
        // - prerequisiteChapterId가 있으면 클리어 필요
        // - 이미 chapterId 클리어된 그룹은 스킵해서 다음 챕터로(원래 요구사항)
        
        // 1) 현재 stage에 해당하는 "가장 뒤 챕터" 후보 찾기
        int bestGroup = -1;
    
        for (int gi = groupStartNode.Length - 1; gi >= 0; gi--)
        {
            var start = groupStartNode[gi];
            if (start == null) continue;
    
            var rule = rulesByGroupIndex[gi];
            if (!IsStageAllowed(rule, stage)) continue;
    
            bestGroup = gi;
            break;
        }
    
        if (bestGroup == -1)
            return FindFallbackStart();
    
        // 2) 후보 챕터의 prerequisite가 미클리어면 → prerequisite 챕터 시작으로 강제
        var bestRule = rulesByGroupIndex[bestGroup];
        if (Progress != null && bestRule != null && !string.IsNullOrEmpty(bestRule.prerequisiteChapterId))
        {
            if (!Progress.IsChapterCleared(bestRule.prerequisiteChapterId))
            {
                if (chapterIdToGroupIndex.TryGetValue(bestRule.prerequisiteChapterId, out int prereqGroup))
                    return groupStartNode[prereqGroup]; // stage가 달라도 선행 챕터로 내려감
            }
        }
    
        // 3) (옵션) 이미 best 챕터 자체가 클리어면 뒤 챕터로 넘어가고 싶다면 스킵 정책 적용 가능
        // 여기서는 “현재 stage의 챕터를 시작”이 기본.
        return groupStartNode[bestGroup];
    }

    // =========================
    // 만약 앞의 선택지가 클리어가 아닐경우 fallback
    // =========================
    private DSDialogueSO FindFallbackStart()
    {
        for (int gi = 0; gi < groupStartNode.Length; gi++)
            if (groupStartNode[gi] != null) return groupStartNode[gi];

        for (int i = 0; i < dialogueContainer.UngroupedDialogues.Count; i++)
            if (dialogueContainer.UngroupedDialogues[i] != null) return dialogueContainer.UngroupedDialogues[i];

        return null;
    }
    
    // =========================
    // Transition Check (선택지 이동 체크)
    // =========================
    private bool CanMoveTo(DSDialogueSO next)
    {
        if (next == null) return true; // null이면 EndDialogue로 가는 흐름

        if (!nodeGroupIndex.TryGetValue(next, out int gi))
            return true; // 캐시에 없으면 막지 않음(안전)

        if (gi < 0) return true; // ungrouped는 조건 없음

        var rule = rulesByGroupIndex[gi];

        // 룰이 없으면 조건 없음 (원하면 false로 바꿔도 됨)
        if (rule == null) return true;

        if (!IsStageAllowed(rule, STAGE)) return false;
        if (!IsPrerequisiteAllowed(rule)) return false;

        return true;
    }

    private static bool IsStageAllowed(ChapterRule rule, int stage)
    {
        if (rule == null) return true; // 룰이 없으면 허용
        return stage >= rule.minStage && stage <= rule.maxStage;
    }

    private bool IsPrerequisiteAllowed(ChapterRule rule)
    {
        if (rule == null) return true;
        if (Progress == null) return true;

        if (!string.IsNullOrEmpty(rule.prerequisiteChapterId) &&
            !Progress.IsChapterCleared(rule.prerequisiteChapterId))
            return false;

        return true;
    }

    // =========================
    // UI Render
    // =========================

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
            var button = Instantiate(choiceButtonPrefab, choicesParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = "다음";

            button.onClick.AddListener(() =>
            {
                if (currentNode.Choices.Count == 0)
                {
                    EndDialogue();
                    return;
                }

                var next = currentNode.Choices[0].NextDialogue;

                if (!CanMoveTo(next))
                {
                    Debug.Log("조건 미충족: 다음 챕터로 이동 불가");
                    return;
                }

                currentNode = next;
                ShowCurrentNode();
            });
        }
        else
        {
            for (int i = 0; i < currentNode.Choices.Count; i++)
            {
                var choice = currentNode.Choices[i];
                var next = choice.NextDialogue;

                var button = Instantiate(choiceButtonPrefab, choicesParent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.Text;

                // ✅ 미리 조건 체크해서 버튼 비활성화
                button.interactable = CanMoveTo(next);

                button.onClick.AddListener(() =>
                {
                    if (!CanMoveTo(next))
                    {
                        Debug.Log("조건 미충족: 선택 불가");
                        return;
                    }

                    currentNode = next;
                    ShowCurrentNode();
                });
            }
        }

        if (currentNode.NpcAnimationClip != null && currentActor != null)
            currentActor.PlayClip(currentNode.NpcAnimationClip);
    }

    private void ClearChoices()
    {
        for (int i = choicesParent.childCount - 1; i >= 0; i--)
            Destroy(choicesParent.GetChild(i).gameObject);
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNode = null;
        ClearChoices();
    }
}

public interface IGameProgress
{
    bool IsChapterCleared(string npcId);
    //void MarkChapterCleared(string npcId, string chapterId); // 필요하면
}
