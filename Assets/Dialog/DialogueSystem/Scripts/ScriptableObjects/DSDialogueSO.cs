using System.Collections.Generic;
using UnityEngine;

namespace DS.ScriptableObjects
{
    using Data;
    using Enumerations;

    // 그래프 상의 노드 하나 (한 줄 대사 + 선택지들)
    public class DSDialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<DSDialogueChoiceData> Choices { get; set; } // 선택지 텍스트 리스트
        [field: SerializeField] public DSDialogueType DialogueType { get; set; } // SingleChoice, MultipleChoice
        [field: SerializeField] public bool IsStartingDialogue { get; set; }
        [field: SerializeField] public AnimationClip NpcAnimationClip { get; set; } // 재생할 애니메이션

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType, bool isStartingDialogue, AnimationClip npcAnimationClip = null)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
            NpcAnimationClip = npcAnimationClip;
        }
    }
}