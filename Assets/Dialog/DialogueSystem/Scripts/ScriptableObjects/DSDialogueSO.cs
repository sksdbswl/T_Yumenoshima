using System;
using System.Collections.Generic;
using UnityEngine;

namespace DS.ScriptableObjects
{
    using Data;
    using Enumerations;

    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] public DialogueGroupType GroupType { get; private set; }
        [field: SerializeField] public string NpcId { get; private set; }
        [field: SerializeField] public string QuestId { get; private set; }
        [field: SerializeField] [field: TextArea] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public AnimationClip NpcAnimationClip { get; set; }
        [field: SerializeField] public List<DSDialogueActionData> Actions { get; set; } = new();

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType,
            bool isStartingDialogue, AnimationClip npcAnimationClip = null)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            NpcAnimationClip = npcAnimationClip;
        }

        // ✅ 추가: 그룹 메타 세팅(툴에서만 세팅)
        public void SetGroupMeta(DialogueGroupType groupType, string npcId, string questId)
        {
            GroupType = groupType;
            NpcId = npcId;
            QuestId = questId;
        }
    }

    public enum DSDialogueActionTrigger { OnEnter, OnExit, OnDialogueEnd }
    public enum DSDialogueActionType { SetNpcStoryStage, SetQuestState, SetFlag }

    [Serializable] 
    public class DSDialogueActionData
    {
        public DSDialogueActionTrigger trigger = DSDialogueActionTrigger.OnExit;
        public DSDialogueActionType type;
        
        public string npcId;
        public int npcStoryStage;

        public string questId;
        public QuestState questState;

        public string flag;
        
        // ✅ 커스텀 함수 호출용
        public string receiverType;   // AssemblyQualifiedName (빌드에서도 안전)
        public string methodName;     // 실행할 메서드명

    }
}
