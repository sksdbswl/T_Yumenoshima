using System;
using UnityEngine;

namespace DS.Data
{
    using ScriptableObjects;

    // 선택지가 1개인 경우의 Data
    [Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }
        
        // 다음 chapter로 진행하기 위한 필드
        [field: SerializeField] public int RequiredStageId { get; set; } 
        [field: SerializeField] public string RequiredChapterId { get; set; } // 예: "Chapter1"
    }
}