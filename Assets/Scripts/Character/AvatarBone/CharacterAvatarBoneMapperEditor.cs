#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEditor;

namespace REIW
{
    [CustomEditor(typeof(CharacterAvatarBoneMapper))]
    public class CharacterAvatarBoneMapperEditor : UnityEditor.Editor
    {
        // ==== 기존 상수/필드 ====
        private const string PROP_ROOT            = "<RootTransform>k__BackingField";
        private const string PROP_CONTAINER       = "<AvatarBoneContainer>k__BackingField";
        private const string PROP_BONE_NAME       = "<BoneName>k__BackingField";
        private const string PROP_BONE_TYPE       = "<BoneType>k__BackingField";
        private const string PROP_BONE_TRANSFORM  = "<BoneTransform>k__BackingField";

        private SerializedProperty _rootProp;
        private SerializedProperty _containerProp;

        // ==== 추가: Config 탐색 관련 ====
        private const string DEFAULT_CONFIG_FOLDER = "Assets/01_REIW/Anothers/Bone Configs";
        private const string PREF_CONFIG_FOLDER    = "REIW.Mapper.ConfigSearchFolder";

        private string _configFolder;
        private List<CharacterAvatarBoneConfig> _configs = new();
        private int _cfgIndex = -1;
        private Editor _cfgEditor;

        private void OnEnable()
        {
            _configFolder = EditorPrefs.GetString(PREF_CONFIG_FOLDER, DEFAULT_CONFIG_FOLDER);
            RefreshConfigs();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _rootProp      ??= serializedObject.FindProperty(PROP_ROOT);
            _containerProp ??= serializedObject.FindProperty(PROP_CONTAINER);

            // RootTransform 경고
            if (_rootProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠ RootTransform 이 비어 있습니다. 컴포넌트가 붙은 GameObject 자체를 루트로 사용합니다.",
                    MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_rootProp);
            EditorGUILayout.PropertyField(_containerProp, includeChildren: true);

            EditorGUILayout.Space(8);

            // ====== Config 섹션 (AssetDatabase 기반) ======
            DrawConfigSelectorAndPreview();

            EditorGUILayout.Space(8);

            // ====== Auto-Mapping 유틸 ======
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Avatar Bone Auto-Mapping", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("이름으로 전체 자동 매핑"))
                        AutoMapAllByName();

                    if (GUILayout.Button("미지정 항목만 매핑"))
                        AutoMapAllByName(onlyUnassigned: true);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("캐시 재생성 (OnValidate)"))
                    {
                        foreach (UnityEngine.Object t in serializedObject.targetObjects)
                            EditorUtility.SetDirty(t);
                    }

                    var disabled = _containerProp == null || _containerProp.arraySize == 0;
                    using (new EditorGUI.DisabledGroupScope(disabled))
                    {
                        if (GUILayout.Button("모든 BoneTransform 리셋"))
                            ResetAllBoneTransforms();
                    }
                }

                EditorGUILayout.HelpBox(
                    "- RootTransform 이하의 모든 자식 Transform에서 BoneName을 찾아 BoneTransform에 설정합니다.\n" +
                    "- 매칭 규칙: 정확 일치(대/소문자 구분) → 정확 일치(대/소문자 무시) → 접미 일치(예: 'mixamorig:RightHand' vs 'RightHand')",
                    MessageType.Info);
            }

            // 항상 미매핑 경고 표시
            var warn = BuildUnassignedWarning();
            if (!string.IsNullOrEmpty(warn))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(warn, MessageType.Warning);
            }

            EditorGUILayout.Space(8);

            // ====== FBBIK의 References bone 연결 메뉴 ======
            DrawFBBIKBoneReferencesMappingMenu();

            serializedObject.ApplyModifiedProperties();
        }

        // ====================================================================
        // Config 섹션
        // ====================================================================
        private void DrawConfigSelectorAndPreview()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("CharacterAvatarBoneConfig (AssetDatabase)", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _configFolder = EditorGUILayout.TextField("Search Folder", _configFolder);
                    if (GUILayout.Button("...", GUILayout.Width(28)))
                    {
                        var abs = EditorUtility.OpenFolderPanel("Select folder under Assets", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(abs))
                        {
                            if (abs.StartsWith(Application.dataPath))
                            {
                                _configFolder = "Assets" + abs.Substring(Application.dataPath.Length);
                                EditorPrefs.SetString(PREF_CONFIG_FOLDER, _configFolder);
                                RefreshConfigs();
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("경고", "Assets 폴더 내부만 선택할 수 있습니다.", "확인");
                            }
                        }
                    }
                    if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                    {
                        EditorPrefs.SetString(PREF_CONFIG_FOLDER, _configFolder);
                        RefreshConfigs();
                    }
                }

                if (_configs.Count == 0)
                {
                    EditorGUILayout.HelpBox($"'{_configFolder}' 경로에서 CharacterAvatarBoneConfig 자산을 찾지 못했습니다.", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("◀", GUILayout.Width(28))) PrevConfig();
                    GUILayout.Label($"{_cfgIndex + 1}/{_configs.Count}  {_configs[_cfgIndex].name}", EditorStyles.boldLabel);
                    if (GUILayout.Button("▶", GUILayout.Width(28))) NextConfig();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(CurrentConfig);
                    if (GUILayout.Button("Open", GUILayout.Width(48))) Selection.activeObject = CurrentConfig;
                }

                // 선택된 Config 미리보기 (그대로 Inspector UI 렌더)
                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if (_cfgEditor == null || _cfgEditor.target != CurrentConfig)
                        _cfgEditor = CreateEditor(CurrentConfig);

                    _cfgEditor.OnInspectorGUI();
                }

                // 적용 버튼들
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply (Replace + AutoMap)"))
                        ApplyConfigToMapper(replaceAll: true, autoMapAll: true);

                    if (GUILayout.Button("Merge (Fill Empty + AutoMap Missing)"))
                        ApplyConfigToMapper(replaceAll: false, autoMapAll: false);
                }
            }
        }

        private void RefreshConfigs()
        {
            _configs.Clear();
            _cfgIndex = -1;
            _cfgEditor = null;

            try
            {
                var guids = AssetDatabase.FindAssets($"t:{nameof(CharacterAvatarBoneConfig)}", new[] { _configFolder });
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    var cfg = AssetDatabase.LoadAssetAtPath<CharacterAvatarBoneConfig>(path);
                    if (cfg != null) _configs.Add(cfg);
                }
            }
            catch { /* 폴더가 없을 수 있음 */ }

            if (_configs.Count > 0) _cfgIndex = 0;
        }

        private CharacterAvatarBoneConfig CurrentConfig =>
            (_cfgIndex >= 0 && _cfgIndex < _configs.Count) ? _configs[_cfgIndex] : null;

        private void PrevConfig()
        {
            if (_configs.Count == 0) return;
            _cfgIndex = (_cfgIndex - 1 + _configs.Count) % _configs.Count;
            _cfgEditor = null;
        }

        private void NextConfig()
        {
            if (_configs.Count == 0) return;
            _cfgIndex = (_cfgIndex + 1) % _configs.Count;
            _cfgEditor = null;
        }

        private static bool ShouldSkip(ReiwHumanBodyBones bone)
            => bone == ReiwHumanBodyBones.None || bone == ReiwHumanBodyBones.LastBone;

        // 핵심: Config → Mapper 컨테이너 적용
        private void ApplyConfigToMapper(bool replaceAll, bool autoMapAll)
        {
            var cfg = CurrentConfig;
            if (cfg == null)
            {
                EditorUtility.DisplayDialog("적용 불가", "선택된 CharacterAvatarBoneConfig가 없습니다.", "확인");
                return;
            }

            var mapper = (CharacterAvatarBoneMapper)target;
            var so = serializedObject; // mapper용
            so.Update();

            var cont = _containerProp; // AvatarBoneContainer
            if (replaceAll) cont.arraySize = 0;

            // 기존 인덱스 맵(마지막 항목 우선)
            var indexByType = new Dictionary<ReiwHumanBodyBones, int>();
            for (int i = 0; i < cont.arraySize; i++)
            {
                var elem  = cont.GetArrayElementAtIndex(i);
                var typeP = elem.FindPropertyRelative(PROP_BONE_TYPE);
                var bone  = (ReiwHumanBodyBones)typeP.intValue;
                if (ShouldSkip(bone)) continue;
                indexByType[bone] = i;
            }

            // SO의 맵을 순회하여 컨테이너 반영
            foreach (var kv in cfg.AvatarBoneKeyValueMap)
            {
                var bone = kv.Key;
                if (ShouldSkip(bone)) continue;

                int idx;
                if (!indexByType.TryGetValue(bone, out idx))
                {
                    idx = cont.arraySize;
                    cont.InsertArrayElementAtIndex(idx);
                    indexByType[bone] = idx;
                }

                var elem   = cont.GetArrayElementAtIndex(idx);
                var nameP  = elem.FindPropertyRelative(PROP_BONE_NAME);
                var typeP  = elem.FindPropertyRelative(PROP_BONE_TYPE);
                var trP    = elem.FindPropertyRelative(PROP_BONE_TRANSFORM);

                // enum은 값 기반으로 설정
                typeP.intValue = (int)bone;

                // Replace 모드: 무조건 덮어쓰기 / Merge 모드: 비어있을 때만 채움
                if (replaceAll || string.IsNullOrEmpty(nameP.stringValue))
                    nameP.stringValue = kv.Value ?? string.Empty;

                // Replace 모드면 Transform 초기화
                if (replaceAll)
                    trP.objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(mapper);

            // 자동 매핑
            if (autoMapAll)
                AutoMapAllByName(onlyUnassigned: true); // 이름 방금 채운 항목 위주로 매핑

            ShowStatus($"Config 적용 완료 ({(replaceAll ? "Replace" : "Merge")})");
        }

        // ====================================================================
        // 기존 Auto Map Routines / Helpers (필요 부분만 인라인 유지)
        // ====================================================================
        private void AutoMapAllByName(bool onlyUnassigned = false)
        {
            var rootTr = GetRootOrTargetTransform(out var mapperObj);
            if (rootTr == null)
            {
                EditorUtility.DisplayDialog("자동 매핑",
                    "RootTransform이 비어 있습니다. 컴포넌트가 붙은 GameObject 자체를 루트로 사용합니다.",
                    "확인");
            }

            var index = BuildNameIndex(rootTr);
            int count = _containerProp.arraySize;
            Undo.RegisterCompleteObjectUndo(mapperObj, "AvatarBones Auto-Map All");

            int mapped = 0;
            for (int i = 0; i < count; i++)
            {
                var elemProp  = _containerProp.GetArrayElementAtIndex(i);
                var nameProp  = elemProp.FindPropertyRelative(PROP_BONE_NAME);
                var transProp = elemProp.FindPropertyRelative(PROP_BONE_TRANSFORM);

                string boneName = nameProp.stringValue;
                if (string.IsNullOrEmpty(boneName)) continue;
                if (onlyUnassigned && transProp.objectReferenceValue != null) continue;

                var found = FindTransformByName(boneName, index);
                if (found != null)
                {
                    transProp.objectReferenceValue = found;
                    mapped++;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(mapperObj);
            ShowStatus($"자동 매핑 완료: {mapped}개 매핑");
        }

        private string BuildUnassignedWarning()
        {
            if (_containerProp == null) return null;

            int count = _containerProp.arraySize;
            if (count == 0) return null;

            var missing = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var elemProp = _containerProp.GetArrayElementAtIndex(i);
                var nameProp = elemProp.FindPropertyRelative(PROP_BONE_NAME);
                var trProp   = elemProp.FindPropertyRelative(PROP_BONE_TRANSFORM);

                if (trProp.objectReferenceValue == null)
                {
                    string n = string.IsNullOrEmpty(nameProp.stringValue) ? "(빈 이름)" : nameProp.stringValue;
                    missing.Add($"{i}: {n}");
                }
            }

            if (missing.Count == 0) return null;

            const int maxLines = 30;
            var listText = string.Join("\n", missing.Take(maxLines));
            if (missing.Count > maxLines)
                listText += $"\n... (외 {missing.Count - maxLines}개)";

            return $"미지정 매핑 BoneTransform 항목 {missing.Count}개:\n{listText}";
        }

        private void ResetAllBoneTransforms()
        {
            var mapperObj = target;

            if (!EditorUtility.DisplayDialog("확인", "모든 항목의 BoneTransform 값을 null로 리셋하시겠습니까?", "예", "아니오"))
                return;

            Undo.RegisterCompleteObjectUndo(mapperObj, "Reset All BoneTransforms");

            int count = _containerProp.arraySize;
            int reset = 0;

            for (int i = 0; i < count; i++)
            {
                var elemProp = _containerProp.GetArrayElementAtIndex(i);
                var trProp   = elemProp.FindPropertyRelative(PROP_BONE_TRANSFORM);

                if (trProp.objectReferenceValue != null)
                {
                    trProp.objectReferenceValue = null;
                    reset++;
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(mapperObj);
            ShowStatus($"BoneTransform 리셋 완료: {reset}개 초기화");
        }

        private Transform GetRootOrTargetTransform(out UnityEngine.Object mapperObj)
        {
            mapperObj = target;
            _rootProp ??= serializedObject.FindProperty(PROP_ROOT);

            var rootRef = _rootProp.objectReferenceValue as Transform;
            if (rootRef != null) return rootRef;

            var comp = (CharacterAvatarBoneMapper)target;
            return comp != null ? comp.transform : null;
        }

        private struct NameIndex
        {
            public Dictionary<string, Transform> Exact;      // Ordinal
            public Dictionary<string, Transform> IgnoreCase; // OrdinalIgnoreCase
            public List<(string name, Transform tr)> All;    // EndsWith 스캔용
        }

        private NameIndex BuildNameIndex(Transform root)
        {
            var exact = new Dictionary<string, Transform>(StringComparer.Ordinal);
            var icase = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
            var all   = new List<(string, Transform)>();

            if (root == null) return new NameIndex { Exact = exact, IgnoreCase = icase, All = all };

            var trs = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in trs)
            {
                string n = t.name;
                if (string.IsNullOrEmpty(n)) continue;

                if (!exact.ContainsKey(n)) exact[n] = t;
                if (!icase.ContainsKey(n)) icase[n] = t;

                all.Add((n, t));
            }

            return new NameIndex { Exact = exact, IgnoreCase = icase, All = all };
        }

        private Transform FindTransformByName(string boneName, NameIndex index)
        {
            if (index.Exact.TryGetValue(boneName, out var tExact))
                return tExact;

            if (index.IgnoreCase.TryGetValue(boneName, out var tIgnore))
                return tIgnore;

            var tail = index.All.FirstOrDefault(p =>
                p.name.EndsWith(boneName, StringComparison.OrdinalIgnoreCase)).tr;
            if (tail != null) return tail;

            int colonIdx = boneName.LastIndexOf(':');
            if (colonIdx >= 0 && colonIdx < boneName.Length - 1)
            {
                var tailToken = boneName[(colonIdx + 1)..];
                if (index.IgnoreCase.TryGetValue(tailToken, out var tColon))
                    return tColon;
            }
            return null;
        }

        private static void ShowStatus(string msg)
        {
            EditorUtility.DisplayDialog("CharacterAvatarBoneMapper", msg, "확인");
            Debug.Log($"[CharacterAvatarBoneMapperEditor] {msg}");
        }

        // ====================================================================
        // FBBIK의 References bone 연결 메뉴
        // ====================================================================
        private FullBodyBipedIK _fullBodyIK;

        private void DrawFBBIKBoneReferencesMappingMenu()
        {
            // ====== Auto-Mapping 유틸 ======
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("FBBIK Bone References Bone Auto-Mapping", EditorStyles.boldLabel);

                using (new EditorGUILayout.VerticalScope())
                {
                    _fullBodyIK = (FullBodyBipedIK)EditorGUILayout.ObjectField(_fullBodyIK, typeof(FullBodyBipedIK), true);
                    if (_fullBodyIK != null)
                    {
                        if (GUILayout.Button("Bone 매핑 정보로 자동 매핑"))
                            FBBIKBoneReferencesAutoMapping(_fullBodyIK);
                    }
                }

                EditorGUILayout.HelpBox(
                    "- Bone 매핑 정보로 FBBIK의 Bone References를 연결합니다.\n",
                    MessageType.Info);
            }
        }

        private void FBBIKBoneReferencesAutoMapping(FullBodyBipedIK fullBodyIK)
        {
            if (!fullBodyIK)
                return;

            var cont = _containerProp; // AvatarBoneContainer
            int count = _containerProp.arraySize;
            if (count == 0)
                return;

            Undo.RegisterCompleteObjectUndo(fullBodyIK, "FBBIK Auto-Map All");

            for (int i = 0; i < cont.arraySize; i++)
            {
                var elem = cont.GetArrayElementAtIndex(i);
                var typeP = elem.FindPropertyRelative(PROP_BONE_TYPE);
                var bone = (ReiwHumanBodyBones)typeP.intValue;
                if (ShouldSkip(bone)) continue;

                var trP = elem.FindPropertyRelative(PROP_BONE_TRANSFORM);
                if (trP == null) continue;

                var tf = trP.objectReferenceValue as Transform;

                switch (bone)
                {
                    case ReiwHumanBodyBones.Root:
                        fullBodyIK.references.root = tf;
                        break;
                    case ReiwHumanBodyBones.Hips:
                        fullBodyIK.references.pelvis = tf;
                        break;
                    case ReiwHumanBodyBones.LeftUpperLeg:
                        fullBodyIK.references.leftThigh = tf;
                        break;
                    case ReiwHumanBodyBones.LeftLowerLeg:
                        fullBodyIK.references.leftCalf = tf;
                        break;
                    case ReiwHumanBodyBones.LeftFoot:
                        fullBodyIK.references.leftFoot = tf;
                        break;
                    case ReiwHumanBodyBones.RightUpperLeg:
                        fullBodyIK.references.rightThigh = tf;
                        break;
                    case ReiwHumanBodyBones.RightLowerLeg:
                        fullBodyIK.references.rightCalf = tf;
                        break;
                    case ReiwHumanBodyBones.RightFoot:
                        fullBodyIK.references.rightFoot = tf;
                        break;
                    case ReiwHumanBodyBones.LeftUpperArm:
                        fullBodyIK.references.leftUpperArm = tf;
                        break;
                    case ReiwHumanBodyBones.LeftLowerArm:
                        fullBodyIK.references.leftForearm = tf;
                        break;
                    case ReiwHumanBodyBones.LeftHand:
                        fullBodyIK.references.leftHand = tf;
                        break;
                    case ReiwHumanBodyBones.RightUpperArm:
                        fullBodyIK.references.rightUpperArm = tf;
                        break;
                    case ReiwHumanBodyBones.RightLowerArm:
                        fullBodyIK.references.rightForearm = tf;
                        break;
                    case ReiwHumanBodyBones.RightHand:
                        fullBodyIK.references.rightHand = tf;
                        break;
                    case ReiwHumanBodyBones.Head:
                        fullBodyIK.references.head = tf;
                        break;
                    case ReiwHumanBodyBones.Spine1:
                        fullBodyIK.references.spine = new[] { tf };
                        fullBodyIK.solver.rootNode = tf;
                        break;
                    case ReiwHumanBodyBones.Spine2:
                    case ReiwHumanBodyBones.Spine3:
                        fullBodyIK.references.spine = fullBodyIK.references.spine.Append(tf).ToArray();
                        break;
                    case ReiwHumanBodyBones.LeftEye:
                        fullBodyIK.references.eyes = new[] { tf };
                        break;
                    case ReiwHumanBodyBones.RightEye:
                        fullBodyIK.references.eyes = fullBodyIK.references.eyes.Append(tf).ToArray();
                        break;
                }
            }
        }
    }
}
#endif
