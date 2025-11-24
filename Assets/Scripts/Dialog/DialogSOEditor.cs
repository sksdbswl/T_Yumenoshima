#if UNITY_EDITOR
using System;
using UnityEditor;

[CustomEditor(typeof(DialogSO))]
public class DialogSOEditor : JsonImporterSOEditor<DialogSO>
{
    protected override string ListName => "Values";    // DialogSO.Values
    protected override string JsonArrayKey => "Items"; // 내가 만들어준 JSON의 루트 배열 키
    protected override string JsonPath =>
        "Assets/Scripts/JsonConvert/DialogTable.json";

    // DTO = DialogRow 그대로 사용
    protected override Type ImportDtoElementType => typeof(DialogRow);
}
#endif