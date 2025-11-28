using System.Collections.Generic;
using UnityEngine;
using DS.ScriptableObjects;

[System.Serializable]
public class NPCDialogueEntry
{
    public string id;                        // 디버그용 키
    public DSDialogueContainerSO container;  // 이 대사 그래프
    public DSDialogueSO startNode;           // 시작 노드

    [Header("NPC Story 조건")]
    public int minNpcStoryStage = 0;
    public int maxNpcStoryStage = 999;

    [Header("퀘스트 조건")]
    public string questId;
    public QuestState requiredQuestState = QuestState.NotStarted;

    [Header("Flag 조건")]
    public List<string> requiredFlags;
}