using System;
using UnityEngine;

namespace DS.Data
{
    using ScriptableObjects;

    //선택지 텍스트
    [Serializable]
    public class DSDialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DSDialogueSO NextDialogue { get; set; }
    }
}