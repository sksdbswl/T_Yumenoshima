// using UnityEngine;
// using UnityEditor;
//
// public class MySimpleTool : EditorWindow
// {
//     private string objectName = "New Name";
//
//     // 윈도우 열기 메뉴
//     [MenuItem("Tools/My Simple Tool")]
//     public static void ShowWindow()
//     {
//         // 윈도우 생성/열기
//         GetWindow<MySimpleTool>("My Simple Tool");
//     }
//
//     private void OnGUI()
//     {
//         GUILayout.Label("선택된 오브젝트 리네임 툴", EditorStyles.boldLabel);
//
//         // 텍스트 필드
//         objectName = EditorGUILayout.TextField("새 이름", objectName);
//
//         // 버튼
//         if (GUILayout.Button("선택된 오브젝트 이름 바꾸기"))
//         {
//             RenameSelectedObjects();
//         }
//     }
//
//     private void RenameSelectedObjects()
//     {
//         // 현재 선택된 오브젝트들
//         var selected = Selection.gameObjects;
//
//         if (selected.Length == 0)
//         {
//             EditorUtility.DisplayDialog("알림", "선택된 GameObject가 없습니다.", "확인");
//             return;
//         }
//
//         Undo.RecordObjects(selected, "Rename Objects");
//
//         for (int i = 0; i < selected.Length; i++)
//         {
//             selected[i].name = objectName + "_" + i;
//         }
//
//         EditorUtility.DisplayDialog("완료", $"{selected.Length}개의 오브젝트 이름을 변경했습니다.", "좋아요");
//     }
// }