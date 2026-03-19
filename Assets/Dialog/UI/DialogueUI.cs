using System;
using System.Collections.Generic;
using System.Reflection;
using DS.Data;
using DS.Enumerations;
using UnityEngine;
using DS.ScriptableObjects;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : UIBase
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Transform choicesParent;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Source Data")]
    [SerializeField] private DSDialogueContainerSO dialogueContainer;

    // ===== Runtime =====
    private DSDialogueSO currentNode;

    // “이번에 시작한 대화”의 시작 노드 정보(진행 저장용)
    private DSDialogueSO currentStartNode;
    private DSDialogueGroupSO currentGroupSO;

    // 캐싱: 노드 -> 그룹SO
    private bool built;
    private Dictionary<DSDialogueSO, DSDialogueGroupSO> nodeToGroupSO;
    
    public void SetContainer(DSDialogueContainerSO container)
    {
        dialogueContainer = container;
        built = false;
    }

    // =========================
    // BUILD CACHE
    // =========================
    private void Build()
    {
        if (built || dialogueContainer == null) return;

        nodeToGroupSO = new Dictionary<DSDialogueSO, DSDialogueGroupSO>(128);

        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            var groupSO = pair.Key;
            var nodes = pair.Value;
            if (nodes == null) continue;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null) continue;
                nodeToGroupSO[node] = groupSO;
            }
        }

        // ungrouped
        foreach (var node in dialogueContainer.UngroupedDialogues)
        {
            if (node == null) continue;
            nodeToGroupSO[node] = null;
        }

        built = true;
    }

    // =========================
    // START AUTO (story -> quest -> daily)
    // =========================
    public void StartDialogueAuto(string npcId)
    {
        if (dialogueContainer == null)
        {
            Debug.LogWarning("[DialogueManager] dialogueContainer is null");
            return;
        }

        Build();

        // 1) Story
        var storyStart = FindNextStartNode(DialogueGroupType.NpcStory, npcId);
        if (storyStart != null)
        {
            BeginFromStartNode(storyStart);
            return;
        }

        // 2) Quest
        var questStart = FindNextStartNode(DialogueGroupType.Quest, npcId);
        if (questStart != null)
        {
            BeginFromStartNode(questStart);
            return;
        }

        // 3) Daily random
        var dailyStart = FindRandomStartNode(DialogueGroupType.Daily, npcId);
        if (dailyStart != null)
        {
            BeginFromStartNode(dailyStart);
            return;
        }
    }

    private void BeginFromStartNode(DSDialogueSO startNode)
    {
        currentStartNode = startNode;
        currentNode = startNode;

        // 그룹 메타 캐시
        currentGroupSO = null;
        if (nodeToGroupSO != null)
            nodeToGroupSO.TryGetValue(startNode, out currentGroupSO);

        Debug.Log($"[DialogueManager] BeginFromStartNode: {startNode.DialogueName}, " +
                  $"GroupType={(currentGroupSO != null ? currentGroupSO.GroupType : startNode.GroupType)}, " +
                  $"NpcId={(currentGroupSO != null ? currentGroupSO.NpcId : "(null)")}, StageId={startNode.StageId}");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentNode();
    }

    // =========================
    // FIND NEXT START NODE
    // =========================
    private DSDialogueSO FindNextStartNode(DialogueGroupType type, string npcId)
    {
        DSDialogueSO best = null;
        int bestStageId = int.MaxValue;

        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            var groupSO = pair.Key;
            var nodes = pair.Value;
            if (groupSO == null || nodes == null) continue;

            // 타입/NPC 필터
            if (groupSO.GroupType != type) continue;
            if (!string.Equals(groupSO.NpcId, npcId, StringComparison.Ordinal)) continue;

            // 저장된 진행도
            int saved = LoadProgress(type, groupSO.NpcId);

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null) continue;

                // ✅ 시작노드만 후보
                if (!node.IsStartingDialogue) continue;

                // ✅ start node만 StageId를 갖고, 나머지는 0인 정책
                if (node.StageId <= 0) continue;

                // ✅ 월드 스테이지 제한
                if (node.StageId > GameManager.Singleton.Stage) continue;

                // ✅ 이미 클리어한 stageId면 스킵
                if (node.StageId <= saved) continue;

                // ✅ 다음 후보 중 stageId 가장 작은 것 선택
                if (node.StageId < bestStageId)
                {
                    bestStageId = node.StageId;
                    best = node;
                }
            }
        }

        Debug.Log($"[DialogueManager] NextStart({type}) -> {(best != null ? best.DialogueName : "null")} (StageId={(best != null ? best.StageId : 0)})");
        return best;
    }

    private DSDialogueSO FindRandomStartNode(DialogueGroupType type, string npcId)
    {
        var list = new List<DSDialogueSO>();

        foreach (var pair in dialogueContainer.DialogueGroups)
        {
            var groupSO = pair.Key;
            var nodes = pair.Value;
            if (groupSO == null || nodes == null) continue;

            if (groupSO.GroupType != type) continue;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null) continue;

                if (!node.IsStartingDialogue) continue;

                // daily 제한 적용
                //if (node.StageId > 0 && node.StageId > STAGE) continue;

                list.Add(node);
            }
        }

        var pick = (list.Count == 0) ? null : list[UnityEngine.Random.Range(0, list.Count)];
        Debug.Log($"[DialogueManager] RandomStart({type}) -> {(pick != null ? pick.DialogueName : "null")}");
        return pick;
    }

    // =========================
    // UI SHOW / FLOW
    // =========================
    private void ShowCurrentNode()
    {
        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        if (speakerText != null) speakerText.text = currentNode.DialogueName;
        if (bodyText != null) bodyText.text = currentNode.Text;

        ClearChoices();

        ExecuteActions(currentNode, DSDialogueActionTrigger.OnEnter);

        var choices = currentNode.Choices ?? new List<DSDialogueChoiceData>();

        if (currentNode.DialogueType == DSDialogueType.SingleChoice)
        {
            CreateButton("다음", () =>
            {
                var next = GetAutoNextNode(currentNode);

                ExecuteActions(currentNode, DSDialogueActionTrigger.OnExit);

                if (next == null)
                {
                    EndDialogue(); // 여기서 저장
                    return;
                }

                currentNode = next;
                ShowCurrentNode();
            });
        }
        else
        {
            if (choices.Count == 0)
            {
                EndDialogue(); // 다음이 없으면 종료 + 저장
                return;
            }

            Debug.Log("=== chioce count: " + choices.Count);
            
            for (int i = 0; i < choices.Count; i++)
            {
                var localChoice = choices[i];
                var localNext = localChoice.NextDialogue;

                CreateButton(localChoice.Text, () =>
                {
                    ExecuteActions(currentNode, DSDialogueActionTrigger.OnExit);

                    if (localNext == null)
                    {
                        EndDialogue();
                        return;
                    }

                    currentNode = localNext;
                    ShowCurrentNode();
                });
            }
        }
    }

    private DSDialogueSO GetAutoNextNode(DSDialogueSO node)
    {
        if (node?.Choices == null || node.Choices.Count == 0) return null;
        return node.Choices[0].NextDialogue;
    }

    private void CreateButton(string text, Action onClick)
    {
        if (choiceButtonPrefab == null || choicesParent == null) return;

        var button = Instantiate(choiceButtonPrefab, choicesParent);
        var label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    private void ClearChoices()
    {
        if (choicesParent == null) return;

        for (int i = choicesParent.childCount - 1; i >= 0; i--)
            Destroy(choicesParent.GetChild(i).gameObject);
    }

    private void EndDialogue()
    {
        // 대화 끝날 때만 “시작 노드 stageId” 저장
        SaveProgress();

        ExecuteActions(currentNode, DSDialogueActionTrigger.OnDialogueEnd);

        UIManager.Hide<DialogueUI>(UIList.DialogueUI);

        currentNode = null;
        currentStartNode = null;
        currentGroupSO = null;

        ClearChoices();
    }

    // =========================
    // PROGRESS SAVE/LOAD
    // =========================
    private int LoadProgress(DialogueGroupType type, string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return 0;

        string key = type switch
        {
            DialogueGroupType.NpcStory => $"{npcId}_storyId",
            DialogueGroupType.Quest    => $"{npcId}_questId",
            _                          => $"{npcId}_{type}"
        };

        return PlayerPrefs.GetInt(key, 0);
    }

    private void SaveProgress(DialogueGroupType type, string npcId, int stageId)
    {
        if (string.IsNullOrEmpty(npcId) || stageId <= 0) return;

        string key = type switch
        {
            DialogueGroupType.NpcStory => $"{npcId}_storyId",
            DialogueGroupType.Quest    => $"{npcId}_questId",
            _                          => $"{npcId}_{type}"
        };

        int prev = PlayerPrefs.GetInt(key, 0);
        if (stageId > prev)
        {
            PlayerPrefs.SetInt(key, stageId);
            PlayerPrefs.Save();
            Debug.Log($"[DialogueManager] SaveProgress key={key}, {prev} -> {stageId}");
        }
    }

    /// <summary>
    /// “대화 끝날 때” 저장 규칙:
    /// - currentStartNode의 GroupType 기준으로 저장
    /// - 저장 값은 currentStartNode.StageId (start node만 유효)
    /// </summary>
    private void SaveProgress()
    {
        if (currentStartNode == null) return;

        // 그룹 타입/NPCId는 그룹SO에서 가져오는게 정석
        DialogueGroupType type = DialogueGroupType.Daily;
        string npcId = "";

        if (currentGroupSO != null)
        {
            type = currentGroupSO.GroupType;
            npcId = currentGroupSO.NpcId;
        }
        else
        {
            // fallback(ungrouped 등)
            type = currentStartNode.GroupType;
            // npcId는 그룹SO 없으면 저장 안 하는게 안전
            npcId = "";
        }

        if (type == DialogueGroupType.Daily) return; // daily는 저장 안 함

        int stageId = currentStartNode.StageId;
        SaveProgress(type, npcId, stageId);
    }

    // =========================
    // ACTIONS
    // =========================
    private void ExecuteActions(DSDialogueSO node, DSDialogueActionTrigger trigger)
    {
        if (node == null || node.Actions == null) return;

        var prog = PlayerDialogueProgress.Singleton;

        for (int i = 0; i < node.Actions.Count; i++)
        {
            var a = node.Actions[i];
            if (a == null || a.trigger != trigger) continue;

            switch (a.type)
            {
                case DSDialogueActionType.SetNpcStoryStage:
                    prog.SetNpcStoryStage(a.npcId, a.npcStoryStage);
                    break;

                case DSDialogueActionType.SetQuestState:
                    prog.SetQuestState(a.questId, a.questState);
                    break;

                case DSDialogueActionType.SetFlag:
                    prog.SetFlag(a.flag);
                    break;
                case DSDialogueActionType.CallMethod: 
                    InvokeActionMethod(a, node);
                    break;
            }
        }
    }
    
    private void InvokeActionMethod(DSDialogueActionData a, DSDialogueSO node)
    {
        // receiverType 비어있으면 DialogueManager(this)에서 찾기
        object target = this;

        // receiverType이 있으면 그 타입의 컴포넌트를 씬에서 찾아 호출
        if (!string.IsNullOrEmpty(a.receiverType))
        {
            var type = Type.GetType(a.receiverType);
            if (type == null)
                return;

            // find 말고 다른 방법은 없으려나 ?
            var comp = FindFirstObjectByType(type); 
            if (comp == null) return;

            target = comp;
        }

        var t = target.GetType();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        var mi =
            t.GetMethod(a.methodName, flags, null, Type.EmptyTypes, null)
            ?? t.GetMethod(a.methodName, flags, null, new[] { typeof(DSDialogueSO) }, null);

        if (mi == null) return;

        try
        {
            var p = mi.GetParameters();
            if (p.Length == 0) mi.Invoke(target, null);
            else mi.Invoke(target, new object[] { node });
        }
        catch (Exception e)
        {
            Debug.LogError($"[DialogueManager] Invoke failed: {t.FullName}.{a.methodName} :: {e}");
        }
    }
    
    /// <summary>
    /// story/quest marker
    /// </summary>
    public bool HasPlayableStoryOrQuest(string npcId, int worldStage)
    {
        if (dialogueContainer == null) return false;

        Build(); // 캐시 빌드

        // 1) Story 가능
        if (FindNextStartNode(DialogueGroupType.NpcStory, npcId) != null)
            return true;

        // 2) Quest 가능
        if (FindNextStartNode(DialogueGroupType.Quest, npcId) != null)
            return true;

        return false;
    }
}
