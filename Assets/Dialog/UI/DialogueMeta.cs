using System;
using System.Collections.Generic;
using UnityEngine;
using DS.ScriptableObjects;

[Serializable]
public class DialogueMeta
{
    public string npcId;                    // Shin, Chief, ...
    public int minStage = 0;
    public int maxStage = 999;

    public DSDialogueContainerSO container; // 이 조건에서 쓸 그래프
}