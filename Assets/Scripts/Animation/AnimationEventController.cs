using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

using Animancer;
using System.Threading;
using System.Reflection;
using REIW.EventLock;

namespace REIW.Animations
{
    [RequireComponent(typeof(AnimancerComponent))]
    public partial class AnimationEventController : CacheMonoBehaviour
    {
        private AnimationEventDataSO.DataInfo[] _animationEvents;

        private IngameCameraSystem _ingameCameraSystem;
        // private PlayerStandaloneController _playerstandaloneController;
        private AnimancerComponent _animancer;
        private float _oldFov = float.MinValue;
        private CancellationTokenSource _cancellationTokenSource = null;
        private LocalCharacter _localCharacter;
        private IngameCameraSystem_Event _ingameCameraSystemEvent;

        public void Initialize(LocalCharacter localCharacter)
        {
            _animationEvents ??= GameDataModel.Singleton.AnimationEventData.Array;
            _cancellationTokenSource = new CancellationTokenSource();

            _ = InitializeAnimationEvents(_cancellationTokenSource.Token);
         
            // _playerstandaloneController = FindAnyObjectByType<PlayerStandaloneController>();
            _ingameCameraSystem = IngameCameraSystem.Instance;// FindAnyObjectByType<IngameCameraSystem>();
            
            Debug.LogWarning("Animation events initialized:" + _ingameCameraSystem);
            
            _oldFov = _ingameCameraSystem.TPSCamera.Lens.FieldOfView;

            _localCharacter = localCharacter;
            _localCharacter.CharacterEventLockController.AddEventLockState(this);

            _ingameCameraSystemEvent = _ingameCameraSystem.gameObject.GetorAddComponent<IngameCameraSystem_Event>();
            _ingameCameraSystemEvent.LocalCharacter = _localCharacter;
//            _localCharacter.MyAnimationEventController = this;

            PlayerController.Instance.EventCameraRotateAction -= UpdateEventCameraRotate;
            PlayerController.Instance.EventCameraRotateAction += UpdateEventCameraRotate;
        }

        private void OnDestroy()
        {
            Dispose(_cancellationTokenSource);
            _cancellationTokenSource = null;

            Dispose(_cameraMoveCancellationTokenSource);
            _cameraMoveCancellationTokenSource = null;

            Dispose(_cameraRotateCancellationTokenSource);
            _cameraRotateCancellationTokenSource = null;

            Dispose(_cameraFovCancellationTokenSource);
            _cameraFovCancellationTokenSource = null;
            
            _localCharacter?.CharacterEventLockController.RemoveEventLockState(this);

            if (PlayerController.Instance != null)
                PlayerController.Instance.EventCameraRotateAction -= UpdateEventCameraRotate;
            

            void Dispose(CancellationTokenSource tokenSource)
            {
                if (tokenSource == null)
                    return;
                if (tokenSource.IsCancellationRequested)
                    return;
                
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }

        protected virtual async UniTaskVoid InitializeAnimationEvents(CancellationToken token)
        {
            _animancer = GetComponent<AnimancerComponent>();

            List<AnimationEventDataSO.DataInfo> list = _animationEvents.ToList();
            List<AnimationEventDataSO.DataInfo> workinglist = new List<AnimationEventDataSO.DataInfo>();

            while (list.Count > 0)
            {
                if (token.IsCancellationRequested)
                    return;

                workinglist.Clear();
                foreach (AnimationEventDataSO.DataInfo info in list)
                {
                    if (info.Initialize(_animancer.States))
                        workinglist.Add(info);
                }

                list.RemoveAll(x => workinglist.Contains(x));
                if (list.Count == 0)
                    return;

                await UniTask.WaitForEndOfFrame(token);
            }
        }

        private AnimancerState _oldAnimancerState = null;
        private (AnimationClip, float) _oldanimationInfo;

        private void Update()
        {
            CheckAnimationState();
        }
        
        private void CheckAnimationState()
        {
            if (_animancer.States == null)
                return;

            if (_oldAnimancerState == null)
            {
                _oldAnimancerState = _animancer.States.Current;
                return;
            }

            if (_oldAnimancerState == _animancer.States.Current)
            {
                _oldanimationInfo = GetCurrentClipInfo();
                return;
            }
            
            _oldAnimancerState = _animancer.States.Current;
            UpdateRemainEvent(_oldanimationInfo);
            

            (AnimationClip, float) GetCurrentClipInfo()
            {
                if (_animancer.States.Current is ManualMixerState mixer)
                {
                    foreach (var state in mixer)
                    {
                        if (state.IsCurrent)
                            return (state.Clip, state.Time);
                    }
                }

                if (_animancer.States.Current is ClipState clipstate)
                {
                    if (clipstate.IsCurrent)
                        return (clipstate.Clip, clipstate.Time);

                }

                return (null, 0);
            }
        }

        private void UpdateRemainEvent((AnimationClip, float) info)
        {
            AnimationClip clip = info.Item1;
            if (!clip)
                return;

            float time = info.Item2;

            List<AnimationEvent> animevents = clip.events.Where(x => (x?.time ?? 0) > time).ToList();
            foreach (AnimationEvent evt in animevents)
            {
                MethodInfo method = this.GetType().GetMethod(evt.functionName, BindingFlags.Instance | BindingFlags.Public);
                if (method == null)
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    method.Invoke(this, null);
                }
                else
                {
                    object[] args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                        args[i] = evt.stringParameter;

                    method.Invoke(this, args);
                }
            }
        }

#if JIN_TEST
        private void LateUpdate()
        {
            // if (Input.GetKeyUp(KeyCode.F5))
            // {
            //     _localCharacter.EventCameraController.MoveDir = Vector3.zero;
            //     Debugging.LogRed($"_localCharacter.EventCameraController.MoveDir : {_localCharacter.EventCameraController.MoveDir}");
            // }
            // if (Input.GetKeyUp(KeyCode.F6))
            // {
            //     _localCharacter.EventCameraController.MoveDir = _localCharacter.transform.forward;
            //     Debugging.LogRed($"_localCharacter.EventCameraController.MoveDir : {_localCharacter.EventCameraController.MoveDir}");
            // }
            // if (Input.GetKeyUp(KeyCode.F7))
            // {
            //     _localCharacter.EventCameraController.MoveDir = -_localCharacter.transform.forward;
            //     Debugging.LogRed($"_localCharacter.EventCameraController.MoveDir : {_localCharacter.EventCameraController.MoveDir}");
            // }
            
            
            if (Input.GetKeyUp(KeyCode.F1))
            {
                AnimationEventDataSO.DataInfo.EventCameraRotate_Grapple info = new AnimationEventDataSO.DataInfo.EventCameraRotate_Grapple();
                info.animationcurve = AnimationCurve.Linear(0, 0, 1, 1);
                info.speed = 1;
                info.toRotation = new Vector3(40, 40, 40);
                info.EndGrappleState = CharacterMoveGrapple.GrappleState.Landing;
                onAnimationEvent_CameraRotate_Grapple(JsonUtility.ToJson(info));
            }
            if (Input.GetKeyUp(KeyCode.F2))
            {
                AnimationEventDataSO.DataInfo.EventCameraMove info = new AnimationEventDataSO.DataInfo.EventCameraMove();
                info.animationcurve = AnimationCurve.Linear(0, 0, 1, 1);
                info.speed = 1;
                info.toMove = new Vector3(40, 40, 40);
                onAnimationEvent_CameraMove(JsonUtility.ToJson(info));
            }

            if (Input.GetKeyUp(KeyCode.F8))
            {
                if (_localCharacter.CharacterEventLockController.AnyExternalLockEvent())
                    _localCharacter.CharacterEventLockController.ResetExternalEventLockType();
                else
                    _localCharacter.CharacterEventLockController.SetExternalEventLockType(eEventLockType.All);
            }
        }
#endif
    }
}
