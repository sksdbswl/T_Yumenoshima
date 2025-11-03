using System.IO;
using UnityEditor;
using UnityEngine;

namespace REIW
{
    public class CharacterGenerateEditorWindow : EditorWindow
    {
        private Object fbxAsset;
        private const string prefabBasePath = "Assets/01_REIW/Anothers/CharacterGenerateEditor/";
        private Editor fbxEditor; // Inspector Preview 전용

        private const string configPath = "Assets/01_REIW/Anothers/CharacterGenerateEditor/GenerateConfig.asset";
        private GenerateConfig config;
        
        
        [MenuItem("PROJECT REIW/Character Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CharacterGenerateEditorWindow>("Character Generator");
            window.minSize = new Vector2(500, 500);
        }
        
        private void OnEnable()
        {
            config = AssetDatabase.LoadAssetAtPath<GenerateConfig>(configPath);
        }
        
        private void OnGUI()
        {
            if (config != null)
            {
                EditorGUILayout.LabelField("Root Scripts:");
                EditorGUI.indentLevel++;
                foreach (var script in config.rootScripts)
                {
                    EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Character Scripts:");
                EditorGUI.indentLevel++;
                foreach (var script in config.characterScripts)
                {
                    EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
                }
                EditorGUI.indentLevel--;
            }
            
            GUILayout.Label("FBX Drag & Drop", EditorStyles.boldLabel);
            fbxAsset = EditorGUILayout.ObjectField("FBX", fbxAsset, typeof(GameObject), false);

            if (fbxAsset != null)
            {
                if (fbxEditor == null || fbxEditor.target != fbxAsset)
                {
                    fbxEditor = Editor.CreateEditor(fbxAsset);
                }

                Rect previewRect = GUILayoutUtility.GetRect(512, 400);
                fbxEditor.OnPreviewGUI(previewRect, EditorStyles.helpBox);
            }

            GUILayout.Space(10);
            GUI.enabled = fbxAsset != null;

            if (GUILayout.Button("Generate Character Prefab"))
            {
                GenerateCharacterPrefab();
                fbxEditor = null; // 변경 시 새로 로딩
            }

            GUI.enabled = true;
        }

        private void GenerateCharacterPrefab()
        {
            if (fbxAsset == null) return;

            string path = AssetDatabase.GetAssetPath(fbxAsset);
            GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);

            // ✅ 새로운 Root 오브젝트 생성
            GameObject rootObject = new GameObject(fbxPrefab.name + "_Root");
            fbxInstance.transform.SetParent(rootObject.transform, false);
            fbxInstance.transform.localPosition = Vector3.zero;

            // ✅ 컴포넌트 추가
            AddRootComponents(rootObject);
            AddCharacterComponents(fbxInstance);

            // 프리팹 저장
            string baseName = fbxPrefab.name + "_Generated";
            string fullPath = GetUniquePrefabPath(baseName);

            Directory.CreateDirectory(prefabBasePath);
            PrefabUtility.SaveAsPrefabAsset(rootObject, fullPath);
            DestroyImmediate(rootObject);

            Debug.Log($"✅ Prefab saved at: {fullPath}");
        }

        private void AddRootComponents(GameObject root)
        {
            if (config == null) return;

            foreach (var script in config.rootScripts)
            {
                AddScriptComponentToObject(root, script);
            }
        }

        private void AddCharacterComponents(GameObject character)
        {
            if (config == null) return;

            if (!character.TryGetComponent<Animator>(out _))
                character.AddComponent<Animator>();

            if (!character.TryGetComponent<CharacterController>(out _))
                character.AddComponent<CharacterController>();

            foreach (var script in config.characterScripts)
            {
                AddScriptComponentToObject(character, script);
            }
        }

        private void AddScriptComponentToObject(GameObject obj, MonoScript script)
        {
            if (script == null) return;

            var type = script.GetClass();
            if (type != null && type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                if (obj.GetComponent(type) == null)
                    obj.AddComponent(type);
            }
        }

        private string GetUniquePrefabPath(string baseName)
        {
            int index = 0;
            string path;

            do
            {
                string suffix = index == 0 ? "" : $" ({index})";
                path = $"{prefabBasePath}{baseName}{suffix}.prefab";
                index++;
            }
            while (File.Exists(path));

            return path;
        }
    }
}
