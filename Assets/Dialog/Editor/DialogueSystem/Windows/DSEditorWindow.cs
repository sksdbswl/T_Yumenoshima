using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DS.Windows
{
    using System;
    using Utilities;

    public class DSEditorWindow : EditorWindow
    {
        private DSGraphView graphView;

        // 기본 파일 이름
        private readonly string defaultFileName = "DialoguesName";

        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;

        [MenuItem("Window/DS/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
            AddStyles();
        }

        private void AddGraphView()
        {
            graphView = new DSGraphView(this);
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            // 파일 이름 입력 필드
            fileNameTextField = DSElementUtility.CreateTextField(
                defaultFileName,
                "File Name:",
                callback =>
                {
                    fileNameTextField.value = callback.newValue
                        .RemoveWhitespaces()
                        .RemoveSpecialCharacters();
                });

            // 버튼들
            saveButton  = DSElementUtility.CreateButton("Save",  () => Save());
            Button loadButton   = DSElementUtility.CreateButton("Load",   () => Load());
            Button clearButton  = DSElementUtility.CreateButton("Clear",  () => Clear());
            Button resetButton  = DSElementUtility.CreateButton("Reset",  () => ResetGraph());
            miniMapButton       = DSElementUtility.CreateButton("Minimap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            // Toolbar 스타일 – 실제 경로
            toolbar.AddStyleSheets(
                "Assets/Dialog/Editor Default Resources/DialogueSystem/DSToolbarStyles.uss"
            );

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            // 전역 변수 / 색상 등 스타일
            rootVisualElement.AddStyleSheets(
                "Assets/Dialog/Editor Default Resources/DialogueSystem/DSVariables.uss"
            );
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure the file name you've typed in is valid.",
                    "Roger!"
                );

                return;
            }

            // Graph 이름 기준으로 IO 유틸 초기화 후 저장
            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }

        private void Load()
        {
            // 🔹 실제 Graph 저장 위치와 동일한 폴더 사용
            string filePath = EditorUtility.OpenFilePanel(
                "Dialogue Graphs",
                "Assets/Dialog/Editor/DialogueSystem/Graphs",
                "asset"
            );

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DSIOUtility.Initialize(graphView, fileName);
            DSIOUtility.Load();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();
            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
    }
}
