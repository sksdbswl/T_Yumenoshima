// using UnityEngine;
// using System;
// using Cysharp.Threading.Tasks;
//
// using Animancer;
// using Unity.Cinemachine;
// using System.Threading;
//
// using REIW.EventLock;
//
// namespace REIW.Animations
// {
//     [RequireComponent(typeof(AnimancerComponent))]
//     public partial class AnimationEventController : CacheMonoBehaviour, ICheckEventLockState
//     {
//         private CinemachineBasicMultiChannelPerlin CameraNoise => _ingameCameraSystemEvent.CameraNoise;
//         private CancellationTokenSource _cameraMoveCancellationTokenSource = null;
//         private CancellationTokenSource _cameraRotateCancellationTokenSource = null;
//         private CancellationTokenSource _cameraFovCancellationTokenSource = null;
//         private System.Action _camereMovefinishAction = null;
//         private System.Action _cameraRotatefinishAction = null;
//         private Quaternion _targetRotation = Quaternion.identity;
//
//         public void OnAnimationEvent_CameraFov(string parameter)
//         {
//             var param = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraFovData>(parameter);
//             ActionCameraFov(_ingameCameraSystem.TPSCamera.Lens.FieldOfView, param.fov, param.speed, param.animationcurve, (fov) => _ingameCameraSystem.TPSCamera.Lens.FieldOfView = fov);
//         }
//
//         public void OnAnimationEvent_CameraFovReset(string parameter)
//         {
//             var param = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraFovResetData>(parameter);
//             ActionCameraFov(_ingameCameraSystem.TPSCamera.Lens.FieldOfView, _oldFov, param.speed, param.animationcurve, (fov) => _ingameCameraSystem.TPSCamera.Lens.FieldOfView = fov);
//         }
//
//         private CancellationTokenSource CameraCancellationToken(ref CancellationTokenSource source)
//         {
//             if (source != null && source.IsCancellationRequested == false)
//             {
//                 source.Cancel();
//                 source.Dispose();
//             }
//
//             source = new CancellationTokenSource();
//             return source;
//         }
//         
//         
//         public void CameraFovResetInScript( float speed,AnimationCurve animationCurve)
//         {
//             ActionCameraFov(_ingameCameraSystem.TPSCamera.Lens.FieldOfView, _oldFov, speed, animationCurve, (fov) => _ingameCameraSystem.TPSCamera.Lens.FieldOfView = fov);
//         }
//         
//         public void CameraFovSetInScript( float fov ,float speed,AnimationCurve animationCurve)
//         {
//             ActionCameraFov(_ingameCameraSystem.TPSCamera.Lens.FieldOfView, fov, speed, animationCurve, (fov) => _ingameCameraSystem.TPSCamera.Lens.FieldOfView = fov);
//         }
//         public async UniTaskVoid ActionCameraFovReset(float speed, AnimationCurve speedCurve, System.Action<float> action, CancellationTokenSource source)
//         {
//             float fromfov = _ingameCameraSystem.TPSCamera.Lens.FieldOfView;
//             float tofov = _oldFov;
//             
//             CancellationTokenSource tokenSource = CameraCancellationToken(ref source);
//             
//             float time = 0;
//             float totaltime = speedCurve.length <= 0 ? 0 : speedCurve[speedCurve.length - 1].time;
//
//             while (true)
//             {
//                 if (tokenSource?.IsCancellationRequested ?? true)
//                     break;
//
//                 time += Time.deltaTime * speed;
//                 float evaluate = speedCurve.Evaluate(time);
//                 float lerp = Mathf.Lerp(fromfov, tofov, evaluate);
//                 action(lerp);
//
//                 if (time >= totaltime)
//                     break;
//
//                 await UniTask.WaitForEndOfFrame(tokenSource.Token);
//             }
//
//             if (source?.IsCancellationRequested == false)
//             {
//                 source.Cancel();
//                 source.Dispose();
//             }
//             source = null;
//         }
//
//         public void ActionCameraFov(float fromfov, float tofov, float speed, AnimationCurve speedCurve, System.Action<float> action)
//         {
//             CancellationTokenSource tokenSource = CameraCancellationToken(ref _cameraFovCancellationTokenSource);
//             ActionCameraFov(fromfov, tofov, speed, speedCurve, action, tokenSource.Token).Forget();
//             
//         }
//         public async UniTaskVoid ActionCameraFov(float fromfov, float tofov, float speed, AnimationCurve speedCurve, System.Action<float> action, CancellationToken token)
//         {
//             if (speedCurve == null)
//                 speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
//
//             float time = 0;
//             float totaltime = speedCurve.length <= 0 ? 0 : speedCurve[speedCurve.length - 1].time;
//
//             while (true)
//             {
//                 if (token.IsCancellationRequested)
//                     break;
//
//                 time += Time.deltaTime * speed;
//                 float evaluate = speedCurve.Evaluate(time);
//                 float lerp = Mathf.Lerp(fromfov, tofov, evaluate);
//                 action(lerp);
//
//                 if (time >= totaltime)
//                     break;
//
//                 await UniTask.WaitForEndOfFrame(token);
//             }
//         }
//
//         public void onAnimationEvnet_CameraShake(string parameter)
//         {
// //            Debug.LogError("onAnimationEvnet_CameraShake");
//
//             var data = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraShakeData>(parameter);
//
//             CameraNoise.NoiseProfile = data.noise;
//             CameraNoise.AmplitudeGain = data.amplitude;
//             CameraNoise.FrequencyGain = data.frequency;
//             
//             if (data.totalTime > 0)
//                 Invoke(nameof(onAnimationEvent_CameraShakeReset), data.totalTime);
//         }
//
//         public void onAnimationEvent_CameraShakeReset()
//         {
// //            Debug.LogError("onAnimationEvnet_CameraShakeReset");
//
//             CameraNoise.AmplitudeGain = 0;
//             CameraNoise.FrequencyGain = 0;
//         }
//
//         public void onAnimationEvent_CameraMove(string parameter)
//         {
//             var data = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraMove>(parameter);
//
//             _camereMovefinishAction?.Invoke();
//             
//             Vector3 oldvec = _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset;
//             onAnimationEvent_Camera(ref _cameraMoveCancellationTokenSource, ref _camereMovefinishAction,
//                 Vector3.zero, data.fromMove, data.toMove, data.speed, data.animationcurve,
//                 (from, to, time) =>
//                 {
//                     Vector3 veclerp = Vector3.Lerp(from, to, time);
//                     _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset = oldvec + veclerp;
//                 },
//                 () =>
//                 {
//                     _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset = oldvec;
//                     _camereMovefinishAction = null;
//                 });
//         }
//
//         public void onAnimationEvent_CameraMove_Grapple(string parameter)
//         {
//             var data = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraMove_Grapple>(parameter);
//
//             _camereMovefinishAction?.Invoke();
//
//             Vector3 oldvec = _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset;
//             onAnimationEvent_Camera(ref _cameraMoveCancellationTokenSource, ref _camereMovefinishAction,
//                 Vector3.zero, data.fromMove, data.toMove, data.speed, data.animationcurve,
//                 (from, to, time) =>
//                 {
//                     Vector3 veclerp = Vector3.Lerp(from, to, time);
//                     _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset = oldvec + veclerp;
//                 },
//                 () =>
//                 {
//                     _ingameCameraSystem.ThirdPersonFollow.ShoulderOffset = oldvec;
//                     _camereMovefinishAction = null;
//                 },
//                 () =>
//                 {
//                     if (data.EndGrappleState == CharacterMoveGrapple.GrappleState.None)
//                         return true;
//
//                     CharacterMoveGrapple grapple = _localCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGrapple>();
//                     return grapple.IsCurrentState(data.EndGrappleState);
//                 },
//                 data.EndOffsetTime
//             );
//         }
//
//         public void onAnimationEvent_CameraRotate(string parameter)
//         {
//             var data = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraRotate>(parameter);
//
//             _cameraRotatefinishAction?.Invoke();
//
//             Quaternion oldqut = _ingameCameraSystem.FollowTarget.rotation;
//             onAnimationEvent_Camera(ref _cameraRotateCancellationTokenSource, ref _cameraRotatefinishAction,
//                 Quaternion.identity, Quaternion.Euler(data.fromRotation), Quaternion.Euler(data.toRotation), data.speed, data.animationcurve,
//                 (from, to, time) =>
//                 {
//                     Quaternion qutlerp = Quaternion.Slerp(from, to, time);
//                     _targetRotation = qutlerp;
//                 },
//                 () =>
//                 {
//                     _ingameCameraSystem.FollowTarget.rotation = oldqut;
//                     _targetRotation = Quaternion.identity;
//                     _cameraRotatefinishAction = null;
//                 });
//         }
//
//         public void onAnimationEvent_CameraRotate_Grapple(string parameter)
//         {
//             var data = JsonUtility.FromJson<AnimationEventDataSO.DataInfo.EventCameraRotate_Grapple>(parameter);
//
//             _cameraRotatefinishAction?.Invoke();
//
//             Quaternion oldqut = _ingameCameraSystem.FollowTarget.rotation;
//             onAnimationEvent_Camera(ref _cameraRotateCancellationTokenSource, ref _cameraRotatefinishAction,
//                 Quaternion.identity, Quaternion.Euler(data.fromRotation), Quaternion.Euler(data.toRotation), data.speed, data.animationcurve,
//                 (from, to, time) =>
//                 {
//                     Quaternion qutlerp = Quaternion.Slerp(from, to, time);
//                     _targetRotation = qutlerp;
//                 },
//                 () =>
//                 {
//                     _ingameCameraSystem.FollowTarget.rotation = oldqut;
//                     _targetRotation = Quaternion.identity;
//                     _cameraRotatefinishAction = null;
//                 },
//                 () =>
//                 {
//                     if (data.EndGrappleState == CharacterMoveGrapple.GrappleState.None)
//                         return true;
//                     
//                     CharacterMoveGrapple grapple = _localCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGrapple>();
//                     return grapple.IsCurrentState(data.EndGrappleState);
//                 },
//                 data.EndOffsetTime);
//             
//         }
//
//         private void onAnimationEvent_Camera<T>( 
//             ref CancellationTokenSource tokenSource,
//             ref Action finishAction, 
//             T start, T from, T to, float speed, AnimationCurve speedCurve,
//             Action<T, T, float> moveAction, 
//             System.Action resetAction,
//             System.Func<bool> waitEndCondition = null,
//             float waitEndOffsetTime = 0f)
//         {
//             CancellationTokenSource newtokenSource = CameraCancellationToken(ref tokenSource);
//
//             finishAction = () =>
//             {
//                 resetAction();
//                 
//                 if (newtokenSource?.IsCancellationRequested == false)
//                 {
//                     newtokenSource?.Cancel();
//                     newtokenSource?.Dispose();
//                 }
//                 newtokenSource = null;
//             };
//
//             ActionCamera(start, from, to, speed, speedCurve, moveAction, finishAction, waitEndCondition, waitEndOffsetTime, newtokenSource.Token).Forget();
//         }
//
//         private async UniTaskVoid ActionCamera<T>(T start, T fromMove, T toMove, float speed, AnimationCurve speedCurve, System.Action<T, T, float> moveAction, Action finishAction, System.Func<bool> waitEndCondition, float waitEndOffsetTime, CancellationToken token)
//         {
//             if (speedCurve == null)
//                 speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
//
//             float totaltime = 1;
//
//             await PlayCameraMotion(start, fromMove, 5, totaltime, moveAction, token);
//             if (token.IsCancellationRequested)
//                 return;
//
//             totaltime = speedCurve.length <= 0 ? 0 : speedCurve[speedCurve.length - 1].time;
//             await PlayCameraMotion(fromMove, toMove, speed, totaltime, moveAction, token);
//             if (token.IsCancellationRequested)
//                 return;
//
//             await UniTask.WaitUntil(() => waitEndCondition?.Invoke() ?? true, PlayerLoopTiming.Update, token);
//             if (token.IsCancellationRequested)
//                 return;
//
//             await UniTask.WaitForSeconds(waitEndOffsetTime, true, PlayerLoopTiming.Update, token);
//             if (token.IsCancellationRequested)
//                 return;
//             
//             totaltime = 1;
//             await PlayCameraMotion(toMove, start, 5, totaltime, moveAction, token);
//             if (token.IsCancellationRequested)
//                 return;
//
//             finishAction();
//         }
//
//         private async UniTask<bool> PlayCameraMotion<T>(T fromMove, T toMove, float speed, float totaltime, System.Action<T, T, float> moveAction, CancellationToken token)
//         {
//             float time = 0;
//
//             while (true)
//             {
//                 if (token.IsCancellationRequested)
//                     break;
//
//                 time += Time.deltaTime * speed;
//                 time = Mathf.Clamp01(time);
//
//                 moveAction(fromMove, toMove, time);
//
//                 if (time >= totaltime)
//                     break;
//
//                 await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
//
//                 if (token.IsCancellationRequested)
//                     break;
//             }
//
//             return true;
//         }
//
//         public eEventLockType CurrentEventLockType
//         {
//             get
//             {
//                 if (_cameraMoveCancellationTokenSource != null && 
//                     _cameraMoveCancellationTokenSource.IsCancellationRequested == false)
//                 {
//                     int all = (int)eEventLockType.All;
//                     all ^= (int)eEventLockType.CharacterMove;
//                     all ^= (int)eEventLockType.CameraRotate;
//                     return (eEventLockType)all;
//                 }
//
//                 if (_cameraRotateCancellationTokenSource != null &&
//                     _cameraRotateCancellationTokenSource.IsCancellationRequested == false)
//                 {
//                     int all = (int)eEventLockType.All;
//                     all ^= (int)eEventLockType.CharacterMove;
//                     all ^= (int)eEventLockType.CameraRotate;
//                     return (eEventLockType)all;
//                 }
//
//                 return eEventLockType.None;
//             }
//         }
//         
//         public eEventLockType ReleaseEventLockType => eEventLockType.None;
//         
//         public void UpdateEventCameraRotate(ref Quaternion originalRotation)
//         {
//             originalRotation *= _targetRotation;
//         }
//     }
// }