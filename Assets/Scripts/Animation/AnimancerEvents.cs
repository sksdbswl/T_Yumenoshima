// using System.Collections.Generic;
// using Animancer;
// using Cysharp.Threading.Tasks;
// using REIW.Animations.Character;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.Serialization;
//
// namespace REIW.Animations
// {
//     public class AnimancerEvents : MonoBehaviour
//     {
//         // 캐릭터 별로
//         // so 관리 해야 함.
//         // Addressable에도 폴더 관리 필요
//         [SerializeField] Animancer.AnimancerComponent animancer;
//         [SerializeField] CharacterEffectSound characterEffectSound;
//         
//         // Sound
//         [SerializeField] Animancer.StringAsset Footstep; 
//         [SerializeField] Animancer.StringAsset SfxSound;
//         
//         //Effect
//         [SerializeField] Animancer.StringAsset CharacterEffect;
//         
//         [SerializeField] Animancer.StringAsset GatheringEvent;
//         
//         [SerializeField] Animancer.StringAsset GatheringAttackEvent;
//         
//         [SerializeField] Animancer.StringAsset PostProcessingEvent;
//
//         private bool _isLocalCharacter = false;
//         private CharacterBase _character;
//         private CharacterAnimation _characterAnimation = null;
//         private CharacterEffectSound _characterEffectSound = null;
//         
//
//         public void Initialize(ClientCharacter clientCharacter)
//         {
//             _isLocalCharacter = clientCharacter.LogicalCharacter != null && clientCharacter.LogicalCharacter.IsLocalCharacter;
//             _character = clientCharacter.LogicalCharacter;
//             _characterAnimation = clientCharacter.CharacterAnimation;
//             _characterEffectSound = clientCharacter._characterEffectSound;
//             
//             if (_characterAnimation != null)
//             {
//                 if (_isLocalCharacter)
//                     _characterAnimation.FootStepEvent += CharacterAnimationOnFootStepEvent;
//             }
//         }
//         
//         void Awake()
//         {
//             if(animancer == null)
//                 animancer = GetComponent<Animancer.AnimancerComponent>();
//             // 이름이 Footstep인 모든 이벤트를 int 파라미터로 받는 콜백에 연결
//             // if(Footstep != null)
//             //     animancer.Events.AddTo<int>(Footstep, OnFootstep);
//             
//             if(SfxSound != null)
//                 animancer.Events.AddTo<int>(SfxSound, OnSfxSound);
//             
//             if(CharacterEffect != null)
//                 animancer.Events.AddTo<int>(CharacterEffect , OnFxEventInt);
//             
//             if(GatheringEvent != null)
//                 animancer.Events.AddTo<int>(GatheringEvent , OnGatheringAnimationEvent);
//             
//             if(GatheringAttackEvent != null)
//                 animancer.Events.AddTo<int>(GatheringAttackEvent , OnGatheringAttackEvent);
//             
//             if(PostProcessingEvent != null)
//                 animancer.Events.AddTo<int>(PostProcessingEvent , OnPostProcessingEvent);
//             // Debug.LogWarning("_character  Animation:" + Character );
//         }
//
//         private void CharacterAnimationOnFootStepEvent((AvatarIKGoal footType, float footPower, eKnownSfxSound groundTag) obj)
//         {
//             _characterEffectSound.PlayCharacterSfx( (int)obj.groundTag);
//         }
//
//         public void OnDestroy()
//         {
//             if (_characterAnimation != null)
//             {
//                 _characterAnimation.FootStepEvent -= CharacterAnimationOnFootStepEvent;
//             }
//         }
//
//         /// <summary>
//         /// Effect 관련 이벤트
//         /// </summary>
//         /// <param name="type"></param>
//         public async void OnFxEventInt(int type)
//         {
//             var ct = _characterEffectSound.GetCancellationTokenOnDestroy(); 
//             
//             EffectDatabaseSO.EffectEntry entry = DataTable.Singleton.GetEffectEntry(type);
//             if (entry != null)
//             {
//                 var fx = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
//
//                 if (fx == null || ct.IsCancellationRequested)
//                 {
//                     Debug.Log($"FX Is NULL ! OnFxEventInt:{type}");
//                     return;
//                 }
//
//                 // Unity API는 메인 스레드에서
//                 await UniTask.SwitchToMainThread(ct);
//                 
//                 if (fx._socket != ReiwHumanBodyBones.None)
//                 {
//                     characterEffectSound.NormalEffect(fx);
//                 }
//                 else
//                 {
//                     Debug.LogError("FX Socket Is None ! OnFxEventInt:" + fx.gameObject.name);
//                 }
//
//                 // sound 가 있으면 sfx 사운드 실행
//                 if (entry.SoundIndex != 0)
//                 {
//                     _characterEffectSound.PlayCharacterSfx(entry.SoundIndex);
//                 }
//             }
//             else
//             {
//                 Debug.Log("EffectEntry is null type=" + type);
//             }
//         }
//         
//         
//         /// <summary>
//         /// Effect 관련 이벤트
//         /// </summary>
//         /// <param name="type"></param>
//         public async void OnFxObjectEvent(int type, Transform glidingObject)
//         {
//             if(glidingObject == null)
//                 Debug.LogError("glidingObject is null!");
//             
//             var ct = _characterEffectSound.GetCancellationTokenOnDestroy(); 
//
//             EffectDatabaseSO.EffectEntry entry = DataTable.Singleton.GetEffectEntry(type);
//             if (entry != null)
//             {
//                 var fx = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
//                 
//                 if (fx == null || ct.IsCancellationRequested)
//                 {
//                     Debug.Log($"FX Is NULL ! OnFxEventInt:{type}");
//                     return;
//                 }
//
//                 // Unity API는 메인 스레드에서
//                 await UniTask.SwitchToMainThread(ct);
//
//                 if (fx != null)
//                 {
//                     characterEffectSound.NormalEffect(fx, glidingObject);
//
//                     // sound 가 있으면 sfx 사운드 실행
//                     if (entry.SoundIndex != 0)
//                     {
//                         _characterEffectSound.PlayCharacterSfx(entry.SoundIndex);
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log("FX Is NULL ! OnFxEventInt:" + type);
//                 }
//             }
//             else
//             {
//                 Debug.Log("EffectEntry is null type=" + type);
//             }
//         }
//
//         
//         
//         void OnSfxSound(int index)
//         {
//
//             // 사운드 디버깅용
//             // var state = Animancer.AnimancerEvent.Current.State;               // 이벤트를 쏜 State
//             // var clip = (state as Animancer.ClipState)?.Clip;                 // 가능하면 원본 Clip
//             // var evt  = Animancer.AnimancerEvent.Current;                     // 현재 이벤트(시간 등)
//             // Debug.Log($"SfxSound value={index},  " +
//             //           $"clip={clip?.name}, time={evt.Event.normalizedTime}");
//             _characterEffectSound.PlayCharacterSfx(index);
//         }
//         
//         void OnGatheringAnimationEvent(int type)
//         {
//             // Debug.LogError("OnGatheringAnimationEvent :" +type);
//             if (_isLocalCharacter)
//             {
//                 if (IngameFieldSubjectSystem.GetLocalPlayer()?.CurrentGatheringObject != null)
//                 {
//                     IngameFieldSubjectSystem.GetLocalPlayer().CurrentGatheringObject.OnGatheringAnimationEvent(type);
//                 }    
//             }
//             else
//             {
//                 if (IngameFieldSubjectSystem.GetOtherPlayer(_character.DatabaseID)?.CurrentGatheringObject != null)
//                 {
//                     IngameFieldSubjectSystem.GetOtherPlayer(_character.DatabaseID).CurrentGatheringObject.OnGatheringAnimationEvent(type);
//                 }
//             }
//
// #if UNITY_EDITOR
//             // 애니메이션 테스트 씬
//             if (AnimationInteractionTest.Instance != null)
//             {
//                 AnimationInteractionTest.Instance.CurrentInteractionPoint?.OnGatheringAnimationEvent(type);
//             }
// #endif
//         }
//
//         GatherPooledEffect _gatherPooledEffect;
//
//         public async void OnGatheringAttackEvent(int index)
//         {
//             // Debug.LogError("___OnGatheringAttackEvent :" + index);
//             var ct = _character.GetCancellationTokenOnDestroy();
//
//             EffectDatabaseSO.EffectEntry entry = DataTable.Singleton.GetEffectEntry(index);
//
//             if (entry == null)
//             {
//                 Debug.LogError("___OnGatheringAttackEvent entry Is Null:" + index);
//                 return;
//             }
//
//             var pooled = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
//             _gatherPooledEffect = pooled as GatherPooledEffect;
//
//             if (_gatherPooledEffect == null || ct.IsCancellationRequested)
//             {
//                 Debug.Log($"_gatherPooledEffect Is NULL ! OnFxEventInt:{index}");
//                 return;
//             }
//
//             if (_gatherPooledEffect != null)
//             {
//                 if (_gatherPooledEffect.IsAttachCharacter)
//                 {
//                     if (_gatherPooledEffect._socket != ReiwHumanBodyBones.None)
//                     {
//                         characterEffectSound.EffectAtGatheringCharacter(_gatherPooledEffect);
//                     }
//                     else
//                     {
//                         Debug.LogError("FX Socket Is None ! OnFxEventInt:" + _gatherPooledEffect.gameObject.name);
//                     }
//
//                 }
//                 else if (_gatherPooledEffect.IsGatherObject)
//                 {
// #if UNITY_EDITOR
//                     // 애니메이션 테스트 씬
//                     if (AnimationInteractionTest.Instance != null)
//                     {
//                         if (AnimationInteractionTest.Instance.CurrentInteractionPoint != null)
//                             _gatherPooledEffect.SpawnEffectAtPosition(
//                                 AnimationInteractionTest.Instance.CurrentInteractionPoint.transform.position,
//                                 _character.Forward);
//                     }
// #endif
//
//                     //해당 오브젝트의 Get position
//
//                     if (_character.GetPlayerNetObject().CurrentGatheringObject != null)
//                     {
//                         characterEffectSound.EffectAtGatheringPoint(_gatherPooledEffect, _character.Forward);
//                     }
//                 }
//                 else if (_gatherPooledEffect.IsToolObjectAttach)
//                 {
//
//                     AttachObject obj = _character.VisualAttachment.GetAttachObject(ReiwHumanBodyBones.RightHandWeapon);
//
//                     if (obj != null)
//                     {
//                         characterEffectSound.EffectAtAttachObject(_gatherPooledEffect, obj.FxTransform, true);
//                     }
//                     else
//                     {
//                         Debug.LogError("___AttachObject is NULL");
//                     }
//                 }
//                 else if (_gatherPooledEffect.IsToolObjectPosition)
//                 {
//                     AttachObject obj = _character.VisualAttachment.GetAttachObject(ReiwHumanBodyBones.RightHandWeapon);
//
//                     if (obj != null)
//                     {
//                         characterEffectSound.EffectAtAttachObject(_gatherPooledEffect, obj.FxTransform, false);
//                     }
//                     else
//                     {
//                         Debug.LogError("___AttachObject is NULL");
//                     }
//                 }
//
//                 // sound 가 있으면 sfx 사운드 실행
//                 if (entry.SoundIndex != 0)
//                 {
//                     _characterEffectSound.PlayCharacterSfx(entry.SoundIndex);
//                 }
//             }
//         }
//
//         void OnPostProcessingEvent(int index)
//         {
//             Debug.LogError("OnPostProcessingEvent :" +index);
//         }
//     }
// }
