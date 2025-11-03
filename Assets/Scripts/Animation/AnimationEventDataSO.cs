// using UnityEngine;
// using System;
// using Unity.Cinemachine;
// using Animancer;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace REIW
// {
//     [AssetPathAttribute("ANIMATION_EVENT/AnimationEventDataSO")]
//     [CreateAssetMenu(fileName = "AnimationEventDataSO", menuName = "ScriptableObject/AnimationEvent/AnimationEventDataSO")]
//     public class AnimationEventDataSO : CustomScriptObject_Key<AnimationEventDataSO.DataInfo, ulong>
//     {
//         private const string AnimationEvent_CameraFov = "OnAnimationEvent_CameraFov";
//         private const string AnimationEvent_CameraFovReset = "OnAnimationEvent_CameraFovReset";
//         private const string AnimationEvent_CameraShake = "onAnimationEvnet_CameraShake";
//         private const string AnimationEvent_CameraShakeReset = "onAnimationEvent_CameraShakeReset";
//         private const string AnimationEvent_CameraMove = "onAnimationEvent_CameraMove";
//         private const string AnimationEvent_CameraRotate = "onAnimationEvent_CameraRotate";
//         private const string AnimationEvent_CameraMove_Grapple = "onAnimationEvent_CameraMove_Grapple";
//         private const string AnimationEvent_CameraRotate_Grapple = "onAnimationEvent_CameraRotate_Grapple";
//         
//         public const string DIVIDE_FRAME = ":";
//
//         public enum eAnimationEventType : uint
//         {
//             None = 0,
//             Custom,
//             Camera_FOV,
//             Camera_FOV_RESET,
//             Camera_Shake,
//             Camera_Shake_Reset,
//             Camera_Move,
//             Camera_Move_Grapple,
//             Camera_Rotate,
//             Camera_Rotate_Grapple,
//         }
//
//         [Serializable]
//         public class DataInfo : CustomScriptData_Key<ulong>
//         {
//             public DataInfo()
//             {
//                 _animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
//             }
//
//             public override ulong GetKey => ID;
//             [SerializeField] private ulong ID;
//             [SerializeField] private AnimationClip _eventClip;
//             [SerializeField] private eAnimationEventType _eventType;
//             public eAnimationEventType EventType => _eventType;
//             
//             [SerializeField, Range(0f, 1f)] private float _eventNormalTime;
//             [SerializeField] private AnimationCurve _animationCurve;
//
//             /// <summary>
//             /// //////////////////////////
//             /// </summary>
//
//             [SerializeField] private string _customEventName;
//             [SerializeField] private string _customEventParameters;
//
//             [SerializeField] private float _cameraFov = 20.0f;
//             [SerializeField, Range(0.1f, 10.0f)] private float _cameraSpeed = 2.0f;
//
//             [SerializeField] private ScriptableObject _noiseProfile;
//
//             [Tooltip("진폭")] [SerializeField] private float _amplitude = 2.0f;
//             [Tooltip("떨림 속도")] [SerializeField] private float _frequency = 2.0f;
//             [Tooltip("전체 시간(0 일시 계속)")] [SerializeField, Range(0, 10)] private float _time = 0.0f;
//
//             [SerializeField] private Vector3 _cameramoveFrom  = Vector3.zero;
//             [SerializeField] private Vector3 _cameramoveTo = Vector3.zero;
//             [SerializeField] private Vector3 _camerarotFrom = Vector3.zero;
//             [SerializeField] private Vector3 _camerarotTo = Vector3.zero;
//
//             [SerializeField] private CharacterMoveGrapple.GrappleState _endGrappleState = CharacterMoveGrapple.GrappleState.None;
//             [SerializeField, Range(0.0f, 10.0f)] private float _endGrappleOffsetTime = 0.0f;
//             
//             #if UNITY_EDITOR
//             public bool ResetInfos()
//             {
//                 bool dirty = false;
//
//                 if (_noiseProfile == null)
//                 {
//                     _noiseProfile = UnityEditor.AssetDatabase.LoadAssetAtPath<NoiseSettings>("Packages/com.unity.cinemachine/Presets/Noise/6D Shake.asset");
//                     _amplitude = 2;
//                     _frequency = 2;
//                     _time = 0;
//                     dirty = true;
//                 }
//
//                 if (_cameraSpeed == 0.0f)
//                 {
//                     _cameraSpeed = 1.0f;
//                     dirty = true;
//                 }
//
//                 if ((_animationCurve?.keys?.Length ?? 0) == 0)
//                 {
//                     _animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
//                     dirty = true;
//                 }
//
//                 return dirty;
//             }
// #endif
//             public bool Initialize(AnimancerStateDictionary states)
//             {
//                 AnimationClip findclip = null;
//                 AnimancerState findstate = states.ToList().Find(x => FindAnimancerState(x, _eventClip, out findclip));
//                 if (findstate == null)
//                     return false;
//
//                 if (_eventType == eAnimationEventType.None)
//                 {
//                     Debug.LogError("animation event controller - event type is None");
//                     return true;
//                 }
//
//                 string functionname = EventFunctionName;
//                 string parameters = EventParameters;
// /*                Debugging.LogError($"set target : {target}, functionname : {functionname}, EventParameters : {parameters}");
//                 AnimancerEvent.Sequence events = findstate.OwnedEvents;
//                 events.Add(_eventNormalTime * findclip.length, () =>
//                 {
//                     Debugging.LogError($"call target : {target}, functionname : {functionname}, EventParameters : {parameters}");
//                     target.SendMessage(functionname, parameters, SendMessageOptions.DontRequireReceiver);
//                 });*/
//
//                 AnimationEvent animevent = new AnimationEvent();
//                 animevent.time = _eventNormalTime * findclip.length;
//                 animevent.functionName = functionname;
//                 animevent.stringParameter = parameters;
//                 animevent.messageOptions = SendMessageOptions.DontRequireReceiver;
//                 findclip.AddEvent(animevent);
//
//                 return true;
//             }
//
//             private bool FindAnimancerState(AnimancerState state, AnimationClip eventclip, out AnimationClip findclip)
//             {
//                 if (state is ManualMixerState mixer)
//                 {
//                     foreach (var mixstate in mixer)
//                     {
//                         if (mixstate.Clip == eventclip)
//                         {
//                             findclip = mixstate.Clip;
//                             return true;
//                         }
//                     }
//                 }
//                 else if (state is ClipState clipState)
//                 {
//                     if (clipState.Clip == eventclip)
//                     {
//                         findclip = clipState.Clip;
//                         return true;
//                     }
//                 }
//
//                 findclip = null;
//                 return false;
//             }
//
//             private string EventFunctionName
//             {
//                 get => _eventType switch
//                 {
//                     eAnimationEventType.Camera_FOV            => AnimationEvent_CameraFov,
//                     eAnimationEventType.Camera_FOV_RESET      => AnimationEvent_CameraFovReset,
//                     eAnimationEventType.Camera_Shake          => AnimationEvent_CameraShake,
//                     eAnimationEventType.Camera_Shake_Reset    => AnimationEvent_CameraShakeReset,
//                     eAnimationEventType.Custom                => _customEventName,
//                     eAnimationEventType.Camera_Move           => AnimationEvent_CameraMove,
//                     eAnimationEventType.Camera_Move_Grapple   => AnimationEvent_CameraMove_Grapple,
//                     eAnimationEventType.Camera_Rotate         => AnimationEvent_CameraRotate,
//                     eAnimationEventType.Camera_Rotate_Grapple => AnimationEvent_CameraRotate_Grapple,
//                     _                                         => string.Empty,
//                 };
//             }
//
//             private string EventParameters
//             {
//                 get => _eventType switch
//                 {
//                     eAnimationEventType.Camera_FOV => JsonUtility.ToJson(new EventCameraFovData
//                     {
//                         speed = _cameraSpeed,
//                         fov = _cameraFov,
//                         animationcurve = _animationCurve,
//                     }),
//                     eAnimationEventType.Camera_FOV_RESET => JsonUtility.ToJson(new EventCameraFovResetData
//                     {
//                         speed = _cameraSpeed,
//                         animationcurve = _animationCurve,
//                     }),
//                     eAnimationEventType.Camera_Shake => JsonUtility.ToJson(new EventCameraShakeData
//                     {
//                         amplitude = _amplitude,
//                         frequency = _frequency,
//                         noise = _noiseProfile as NoiseSettings,
//                         totalTime = _time,
//                     }),
//                     eAnimationEventType.Custom => _customEventParameters,
//                     eAnimationEventType.Camera_Move => JsonUtility.ToJson(new EventCameraMove
//                     {
//                         animationcurve = _animationCurve,
//                         speed = _cameraSpeed,
//                         toMove = _cameramoveTo,
//                         fromMove = _cameramoveFrom,
//                     }),
//                     eAnimationEventType.Camera_Move_Grapple => JsonUtility.ToJson(new EventCameraMove_Grapple
//                     {
//                         animationcurve = _animationCurve,
//                         speed = _cameraSpeed,
//                         toMove = _cameramoveTo,
//                         fromMove = _cameramoveFrom,
//                         EndGrappleState = _endGrappleState,
//                         EndOffsetTime = _endGrappleOffsetTime,
//                     }),
//                     eAnimationEventType.Camera_Rotate => JsonUtility.ToJson(new EventCameraRotate
//                     {
//                         animationcurve = _animationCurve,
//                         speed = _cameraSpeed,
//                         toRotation = _camerarotTo,
//                         fromRotation = _camerarotFrom,
//                     }),
//                     eAnimationEventType.Camera_Rotate_Grapple => JsonUtility.ToJson(new EventCameraRotate_Grapple
//                     {
//                         animationcurve = _animationCurve,
//                         speed = _cameraSpeed,
//                         toRotation = _camerarotTo,
//                         fromRotation = _camerarotFrom,
//                         EndGrappleState = _endGrappleState,
//                         EndOffsetTime = _endGrappleOffsetTime,
//                     }),
//
//                     _ => string.Empty,
//                 };
//             }
//
//             [Serializable]
//             public class EventBaseData
//             {
// //                public ulong id;
//             }
//
//             [Serializable]
//             public class EventCameraFovData : EventBaseData
//             {
//                 public AnimationCurve animationcurve;
//                 public float speed;
//                 public float fov;
//             }
//
//             [Serializable]
//             public class EventCameraFovResetData : EventBaseData
//             {
//                 public AnimationCurve animationcurve;
//                 public float speed;
//             }
//
//             [Serializable]
//             public class EventCameraShakeData : EventBaseData
//             {
//                 public NoiseSettings noise;
//                 public float amplitude;
//                 public float frequency;
//                 public float totalTime;
//             }
//
//             [Serializable]
//             public abstract class EventCamera : EventBaseData
//             {
//                 public AnimationCurve animationcurve;
//                 public float speed;
//             }
//             [Serializable]
//             public class EventCameraMove : EventCamera
//             {
//                 public Vector3 toMove;
//                 public Vector3 fromMove;
//             }
//             [Serializable]
//             public class EventCameraMove_Grapple : EventCameraMove
//             {
//                 public CharacterMoveGrapple.GrappleState EndGrappleState;
//                 public float EndOffsetTime;
//             }
//             [Serializable]
//             public class EventCameraRotate : EventCamera
//             {
//                 public Vector3 toRotation;
//                 public Vector3 fromRotation;
//             }
//             [Serializable]
//             public class EventCameraRotate_Grapple : EventCameraRotate
//             {
//                 public CharacterMoveGrapple.GrappleState EndGrappleState;
//                 public float EndOffsetTime;
//             }
//         }
//         
// #if UNITY_EDITOR
//         [ContextMenu("ClearNoneEvent")]
//         private void OnClearNoneEvent()
//         {
//             DataList.ToList().RemoveAll(x => x.EventType == eAnimationEventType.None);
//             UnityEditor.EditorUtility.SetDirty(this);
//         }
//         
//         private void OnValidate()
//         {
//             if (Application.isPlaying)
//                 return;
//             
//             if (DataList?.Any(x => x.ResetInfos()) ?? false)
//                 UnityEditor.EditorUtility.SetDirty(this);
//         }
// #endif
//
//     }
// }
