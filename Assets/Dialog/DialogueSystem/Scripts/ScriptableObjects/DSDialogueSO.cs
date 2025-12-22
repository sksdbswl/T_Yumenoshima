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
        [field: SerializeField] public string NpcId { get; private set; }   // ✅ StoryId -> NpcId
        [field: SerializeField] [field: TextArea] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public AnimationClip NpcAnimationClip { get; set; }
        [field: SerializeField] public List<DSDialogueActionData> Actions { get; set; } = new();
        [field: SerializeField] public int StageId { get; set; }

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices,
            DSDialogueType dialogueType, bool isStartingDialogue, AnimationClip npcAnimationClip = null, int stageId = 0)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            NpcAnimationClip = npcAnimationClip;
            StageId = stageId;
        }

        public void SetGroupMeta(DialogueGroupType groupType, string npcId)
        {
            GroupType = groupType;
            NpcId = npcId;
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

        public string receiverType;
        public string methodName;
    }
}
