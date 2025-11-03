// using System;
// using System.Collections.Generic;
// using Unity.Collections;
// using UnityEngine;
//
// namespace REIW
// {
//     public class AnimationClipRootMotionSettingsSO : SerializeIfChangedSO
//     {
//         [SerializeField] private List<RootMotionSettings> _settings = new();
//
//         private Dictionary<string, RootMotionSettings> datas = new();
//
//         //public static readonly string ROOT_MOTION_SETTINGS_PATH = $"{AssetConstant.AddressAnimationData}{AssetConstant.AddressPrefixRootMotionData}";
//         private static readonly string ROOT_MOTION_SETTINGS_SO_FILE_NAME = $"{nameof(AnimationClipRootMotionSettingsSO)}_{{0}}";
//         private static readonly string ROOT_MOTION_SETTINGS_SO_CHARACTER_FILE_NAME = $"{nameof(AnimationClipRootMotionSettingsSO)}_{{0}}_{{1}}";
//         private const int ROOT_MOTION_SETTINGS_PATH_DEPTH = 2;
//         private const int ROOT_MOTION_SETTINGS_CHARACTER_PATH_DEPTH = 3;
//
//         public static string GetRootMotionSettingsSOFileNameFormat(eObjectType type)
//         {
//             return type switch
//             {
//                 eObjectType.Character => ROOT_MOTION_SETTINGS_SO_CHARACTER_FILE_NAME,
//                 _ => ROOT_MOTION_SETTINGS_SO_FILE_NAME
//             };
//         }
//
//         public static int GetRootMotionSettingsPathDepth(eObjectType type)
//         {
//             return type switch
//             {
//                 eObjectType.Character => ROOT_MOTION_SETTINGS_CHARACTER_PATH_DEPTH,
//                 _ => ROOT_MOTION_SETTINGS_PATH_DEPTH
//             };
//         }
//
//         public void Set(RootMotionSettings value)
//         {
//             if (!string.IsNullOrEmpty(value.clipName))
//                 datas[value.clipName] = value;
//
// #if UNITY_EDITOR
//             SetDirty();
// #endif
//         }
//
//         public void Clear()
//         {
//             datas.Clear();
//
// #if UNITY_EDITOR
//             SetDirty();
// #endif
//         }
//
//         protected override void BeforeSerialize()
//         {
//             _settings.Clear();
//
//             foreach (var rs in datas)
//             {
//                 _settings.Add(rs.Value);
//             }
//
//             _settings.Sort((l, r) =>
//                 string.Compare(l.clipName, r.clipName, StringComparison.OrdinalIgnoreCase));
//         }
//
//         protected override void AfterDeserialize()
//         {
//             datas = new();
//             for (int i = 0; i < _settings.Count; ++i)
//             {
//                 var setting = _settings[i];
//                 if (!string.IsNullOrEmpty(setting.clipName))
//                     datas[setting.clipName] = setting;
//             }
//         }
//
//         public RootMotionSettings GetRootMotionSettings(AnimationClip clip)
//         {
//             if (clip == null)
//                 return default;
//
//             return datas.GetValueOrDefault(clip.name);
//         }
//     }
//
//     [Serializable]
//     public struct RootMotionSettings
//     {
//         [ReadOnly] public string clipName;
//         public bool rotation;
//         public bool posY;
//         public bool posXZ;
//     }
// }
