using System.Linq;
using UnityEditor;
using UnityEngine;

namespace REIW
{
    public class AnimationRootMotionDataExtractor : EditorWindow
    {
        private GameObject _fbx;
        private AnimationClip _clip;

        private bool _isExtract;
        private float _rootMotionSpeed;
        private CharacterAnimationRotationData _characterAnimationRotationData;

        private string _samplingResultText;

        [MenuItem("PROJECT REIW/Animation/Root Motion Data Extractor")]
        private static void ShowWindow()
        {
            GetWindow<AnimationRootMotionDataExtractor>("Animation Root Motion Data Extractor");
        }

        private void OnGUI()
        {
            GameObject fbx = _fbx;
            _fbx = (GameObject)EditorGUILayout.ObjectField("FBX", _fbx, typeof(GameObject), false);
            if (fbx != _fbx)
                _clip = FbxClipExtractorTool.GetAnimationClipsInFBX(AssetDatabase.GetAssetPath(_fbx)).FirstOrDefault();
            if (!_clip)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.HelpBox("FBX 파일이 아니거나 Animation Clip이 포함돼 있지 않습니다.", MessageType.Error);
                return;
            }

            AnimationClip clip = _clip;
            _clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", _clip, typeof(AnimationClip), false);
            if (!_clip)
                return;

            if (clip != _clip)
                Reset();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Extract Root Motion Data", GUILayout.Height(30)))
                ExtractRootMotionData();

            EditorGUILayout.Space(5);

            if (!_isExtract)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space(1);
                EditorGUILayout.PrefixLabel("Result", EditorStyles.largeLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("Root Motion Speed", _rootMotionSpeed);
                EditorGUI.EndDisabledGroup();

                if (_characterAnimationRotationData != null &&
                    GUILayout.Button("Create Animation Rotation Data", GUILayout.Height(30)))
                {
                    string path = EditorUtility.OpenFolderPanel("생성 위치 선택", Application.dataPath, "");
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = AssetDatabase.GenerateUniqueAssetPath("Assets" +
                                                                     path.Substring(Application.dataPath.Length) +
                                                                     $"/AnimationRotationData_{_clip.name}.asset");
                        AssetDatabase.CreateAsset(_characterAnimationRotationData, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = _characterAnimationRotationData;
                    }
                }

                if (!string.IsNullOrEmpty(_samplingResultText))
                {
                    EditorGUILayout.Space(3);
                    EditorGUILayout.HelpBox(_samplingResultText, MessageType.Info);
                }

                EditorGUILayout.Space(1);
            }
            EditorGUILayout.EndVertical();
        }

        private void Reset()
        {
            _isExtract = false;
            _rootMotionSpeed = 0;
            _samplingResultText = string.Empty;
        }

        private void ExtractRootMotionData()
        {
            if (!_clip)
                return;

            var path = AssetDatabase.GetAssetPath(_clip);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError("fbx 파일 내의 클립을 Animation Clip에 연결 해야 합니다.");
                _isExtract = false;
                return;
            }

            _isExtract = true;
            var positionRootPath = string.Empty;
            var rotationRootPath = string.Empty;
            var positionPropertyName = "RootT";
            var rootType = importer.animationType != ModelImporterAnimationType.Human
                ? typeof(Transform)
                : typeof(Animator);

            if (importer.animationType != ModelImporterAnimationType.Human)
                positionPropertyName = "m_LocalPosition";

            var bindings = AnimationUtility.GetCurveBindings(_clip);

            positionRootPath = bindings.FirstOrDefault(b =>
                b.type == rootType && b.propertyName.StartsWith(positionPropertyName)).path;

            if (importer.animationType == ModelImporterAnimationType.Human)
            {
                rotationRootPath = bindings.FirstOrDefault(b =>
                    b.type == rootType && b.propertyName.StartsWith("RootQ")).path;
            }
            else
            {
                rotationRootPath = bindings.FirstOrDefault(b =>
                    b.type == rootType && (b.propertyName.StartsWith("m_LocalRotation") ||
                                           b.propertyName.StartsWith("localEulerAnglesRaw"))).path;
            }

            var rootPosX = AnimationUtility.GetEditorCurve(_clip,
                EditorCurveBinding.FloatCurve(positionRootPath, rootType, $"{positionPropertyName}.x"));
            var rootPosY = AnimationUtility.GetEditorCurve(_clip,
                EditorCurveBinding.FloatCurve(positionRootPath, rootType, $"{positionPropertyName}.y"));
            var rootPosZ = AnimationUtility.GetEditorCurve(_clip,
                EditorCurveBinding.FloatCurve(positionRootPath, rootType, $"{positionPropertyName}.z"));

            _characterAnimationRotationData = CreateInstance<CharacterAnimationRotationData>();
            if (importer.animationType == ModelImporterAnimationType.Human)
            {
                _characterAnimationRotationData.RotationCurveX = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "RootQ.x"));
                _characterAnimationRotationData.RotationCurveY = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "RootQ.y"));
                _characterAnimationRotationData.RotationCurveZ = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "RootQ.z"));
                _characterAnimationRotationData.RotationCurveW = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "RootQ.w"));
            }
            else
            {
                _characterAnimationRotationData.RotationCurveX = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "m_LocalRotation.x")) ??
                                                                 AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "localEulerAnglesRaw.x"));
                _characterAnimationRotationData.RotationCurveY = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "m_LocalRotation.y")) ??
                                                                 AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "localEulerAnglesRaw.y"));
                _characterAnimationRotationData.RotationCurveZ = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "m_LocalRotation.z")) ??
                                                                 AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "localEulerAnglesRaw.z"));
                _characterAnimationRotationData.RotationCurveW = AnimationUtility.GetEditorCurve(_clip, EditorCurveBinding.FloatCurve(rotationRootPath, rootType, "m_LocalRotation.w"));
            }

            var sampleCount = Mathf.CeilToInt(_clip.length * _clip.frameRate);
            var rootMotionSpeedQueue = new LocalCharacter.FixedSizeQueue<float>(sampleCount);
            var prevRootMotionPos = Vector3.zero;

            _samplingResultText = $"Sampling Root Motion for '{_clip.name}' over {sampleCount} frames:";

            for (var i = 0; i <= sampleCount; ++i)
            {
                var time = (i / _clip.frameRate);
                var x = rootPosX?.Evaluate(time) ?? 0f;
                var y = rootPosY?.Evaluate(time) ?? 0f;
                var z = rootPosZ?.Evaluate(time) ?? 0f;
                var rootPos = new Vector3(x, y, z);

                if (i > 0)
                    rootMotionSpeedQueue.Enqueue(GetRootMotionSpeed(rootPos - prevRootMotionPos));

                prevRootMotionPos = rootPos;
            }

            _rootMotionSpeed = (float)rootMotionSpeedQueue.GetAverage();
        }

        private float GetRootMotionSpeed(Vector3 InRootMotionDeltaPosition)
        {
            var rootMotionVelocity = InRootMotionDeltaPosition / Time.deltaTime;
            // var vRootMotionVelocity = Vector3.Project(rootMotionVelocity, Vector3.up);
            // var hRootMotionVelocity = rootMotionVelocity - vRootMotionVelocity;
            return rootMotionVelocity.magnitude * 10f;
        }
    }
}