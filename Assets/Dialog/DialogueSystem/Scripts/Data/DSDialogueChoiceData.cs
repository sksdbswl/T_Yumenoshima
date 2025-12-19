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
    }
}