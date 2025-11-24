using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogSO", menuName = "Dialog/DialogSO")]
public class DialogSO : ScriptableObject
{
    public List<DialogRow> Values = new();
}

[System.Serializable]
public class DialogRow
{
    public string Category;
    public int Id;
    public string Key;
    public string Kor;
    public string NPC;
    public bool IsStory;
    public int WorldStageMin;
    public int WorldStageMax;
    public int NpcStoryStage;
    public int Order;
    public string Speaker;
}