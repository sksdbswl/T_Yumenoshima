using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace REIW
{
    public enum CharacterType
    {
        InGame,
        OutGame,
    }
    
    public class CharacterEffectSound : MonoBehaviour
    {
        private CharacterBase _character;
        private ClientCharacter _clientCharacter;
        private bool IsLocalCharacter = false;
        private CharacterType _characterType = CharacterType.InGame;
        private OwnerPlayerNetObject _ownerPlayerNetObject =null;
        public virtual void Initialize(CharacterBase character, ClientCharacter clientCharacter, CharacterType type = CharacterType.InGame)
        {
            _characterType = type;
            if (_characterType == CharacterType.InGame)
            {
                _character = character;
                IsLocalCharacter = _character.IsLocalCharacter;
            }
                
            _clientCharacter = clientCharacter;
            

            if (IsLocalCharacter)
            {
                LocalCharacter _localCharacter = character as LocalCharacter;
                if (_localCharacter != null)
                {
                    _ownerPlayerNetObject = _localCharacter.GetComponent<OwnerPlayerNetObject>();

                    if (_ownerPlayerNetObject == null)
                    {
                        Debug.LogError("CharacterActionEffect: LocalCharacter is not a LocalCharacter");
                    }
                }
                else
                {
                    Debug.LogError("This character is not a local character");
                }
            }
        }
        
       // ────────────────────────────── Wire Action ──────────────────────────────
        private WirePooledEffect _wireFx = null;
        
        public async UniTask<bool> WireActionTest(Vector3 grapplePosition, CancellationToken ct)
        {
            EffectDatabaseSO.EffectEntry  entry = DataTable.Singleton.GetEffectEntry((int)eKnownEffect.Wire_Action);
            if (entry != null)
            {
                var pooled = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
                _wireFx= pooled as WirePooledEffect;

                if (_wireFx == null)
                {
                    Debug.LogError("_wireFx is null");
                    return false;
                }

                if (ct.IsCancellationRequested) return false; 

                await UniTask.SwitchToMainThread(ct);
                
                _wireFx.WireAction(_clientCharacter.AvatarBoneMapper.GetBoneTransform( ReiwHumanBodyBones.RightHand), grapplePosition);
                return true;
            }
            else
            {
                Debug.Log("EffectEntry is null type= 1000" );
            }
            return false;
        }

        public virtual void AddSnapShot_GrapplePosition(Vector3 grapplePosition)
        {
            if (IsLocalCharacter)
            {
                _ownerPlayerNetObject.AddSnapShot_GrapplePosition(grapplePosition);
            }
        }
      
        public async UniTask WireTargetEvent(Vector3 position, CancellationToken ct)
        {
            // Debug.LogWarning("OnFxWireTargetEvent:" +position.ToString());
            
            EffectDatabaseSO.EffectEntry entry = DataTable.Singleton.GetEffectEntry((int)eKnownEffect.Wire_Target);
            
            if (entry == null)
            {
                Debug.LogError("EffectEntry Is NULL ! OnFxWire_TargetEvent:");
                return;
            }
            
            
            var pooled = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
            if (pooled == null || ct.IsCancellationRequested)
            {
                Debug.Log("FX Is NULL ! OnFxWire_TargetEvent:");
                return;
            }
            await UniTask.SwitchToMainThread(ct);
            if (ct.IsCancellationRequested || this == null || !gameObject) return;

            pooled.SpawnEffectAtPosition(position, Vector3.forward);
        }
        
        public void StopWireAction()
        {
            if (_wireFx != null && _wireFx.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("Test Code 그래플 StopWireAction");
                //>
                _wireFx.StopWireAction();
            }
        }
        
        public virtual void AddSnapShot_GrappleEnd()
        {
            _ownerPlayerNetObject.AddSnapShot_GrapplePosition(Vector3.zero);
        }
        
        // ────────────────────────────── Looping FX ──────────────────────────────
        Dictionary<eKnownEffect, LoopingPooledEffect> _loopingPooledEffects = new Dictionary<eKnownEffect, LoopingPooledEffect>();
        
        public async UniTask LoopingFxUniTask(REIW.eKnownEffect fxType, CancellationToken ct)
        {
            if (_loopingPooledEffects.ContainsKey(fxType))
            {
                Debug.LogError("LoopingFx Already Exists");
                return;
            }

            try
            {
                var entry = DataTable.Singleton.GetEffectEntry((int)fxType);
                if (entry == null || ct.IsCancellationRequested) return;

                var pooled = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
                var fx = pooled as LoopingPooledEffect;

                if (fx == null || ct.IsCancellationRequested) return;

                // 메인 스레드 보장
                await UniTask.SwitchToMainThread(ct);

                if (ct.IsCancellationRequested) return;

                var socket = _clientCharacter.AvatarBoneMapper.GetBoneTransform(fx._socket);

                fx.SpawnEffectAtSocket(socket, transform.forward);

                _loopingPooledEffects.Add(fxType, fx);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }



        public void StopLoopingFx(REIW.eKnownEffect fxType)
        {
            if (_loopingPooledEffects.ContainsKey(fxType))
            {
                LoopingPooledEffect fx = _loopingPooledEffects[fxType];
                if(fx != null)
                    fx.ForceStop();
                _loopingPooledEffects.Remove(fxType);
            }
        }
        
         // //Effect 실행
        public void NormalEffect(PooledEffect effect, Transform trf)
        {
            if (effect == null || trf == null) return;
            effect.SpawnEffectAtSocket(trf, transform.forward);
        }

        //
        public void NormalEffect(PooledEffect effect)
        {
            Transform socket = _clientCharacter.AvatarBoneMapper.GetBoneTransform(effect._socket);
            effect.SpawnEffectAtSocket(socket, transform.forward);
        }

        // 비동기 경로에서 사용하고 싶을 때 (안전 버전)
        public async UniTask NormalEffectAsync(PooledEffect effect, Transform trf, CancellationToken ct)
        {
            if (effect == null || trf == null) return;
            await UniTask.SwitchToMainThread(ct);
            if (ct.IsCancellationRequested || this == null || !gameObject) return;
            effect.SpawnEffectAtSocket(trf, transform.forward);
        }

        public async UniTask NormalEffectAsync(PooledEffect effect, CancellationToken ct)
        {
            if (effect == null) return;
            await UniTask.SwitchToMainThread(ct);
            if (ct.IsCancellationRequested || this == null || !gameObject) return;
            var socket = _clientCharacter.AvatarBoneMapper.GetBoneTransform(effect._socket);
            if (socket == null) return;
            effect.SpawnEffectAtSocket(socket, transform.forward);
        }
        
        
        //OnGatheringAttackEvent
        PooledEffect _gatherEffect = null;
        public void EffectAtGatheringCharacter(PooledEffect effect)
        {
            Transform socket =_clientCharacter.AvatarBoneMapper.GetBoneTransform(effect._socket);
            effect.SpawnEffectAtSocket(socket, transform.forward);
            
            _gatherEffect = effect;
        }

        public void EffectAtGatheringPoint(PooledEffect effect, Vector3 direction)
        {
            ////거리 체크 해야 함.
            effect.SpawnEffectAtPosition( transform.position, direction);
            
            _gatherEffect = effect;
        }
        
        public void EffectAtAttachObject(PooledEffect effect, Transform attachBone, bool isAttach)
        {
            if (isAttach)
            {
                effect.trackMode = FxTrackMode.AttachToSocket;
                ////거리 체크 해야 함.
                effect.SpawnEffectAtSocket(attachBone, attachBone.forward);
            }
            else
            {
                ////거리 체크 해야 함.
                effect.SpawnEffectAtPosition(attachBone.position, attachBone.forward);    
            }
            
            
            _gatherEffect = effect;
        }

        public void GatherEffectCancel()
        {
            if (_gatherEffect != null)
            {
                _gatherEffect.ForceStop();
            }
        }
        
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// SFX Sound Play
        /// </summary>
        /// <param name="index"></param>
        public void PlayCharacterSfx(int index)
        {
            SfxDatabaseSO.SfxEntry entry = DataTable.Singleton.GetEntry(index);

            if (entry == null)
            {
                Debug.LogError("SFX entry == null --" + index);
                return;
            }

            if (IsLocalCharacter || _characterType == CharacterType.OutGame)
            {
                SoundManager.Singleton.PlayerSfx(entry);
                return;
            }
            else if(entry._categoryData.soundType == SfxDatabaseSO.Category.eSoundType.Shared) // OtherPlayer
            {
                if (_character.SqrMagnitude < entry._categoryData.hearingDistance )
                {
                    // 거리 체크 후 
                    SoundManager.Singleton.OtherPlayerSfx(entry, this.transform.position);
                }
            }
        }
        
        // Local Player만 호출 해야 함.
        public void LocalPlayerSfx(int index)
        {
#if UNITY_EDITOR
            if (IsLocalCharacter == false)
            {
                Debug.LogError(" 이 메소드는 로컬 플레이만 호출 해야 함 !!!! - Name :" + this.gameObject.name);
            }
#endif
            
            SfxDatabaseSO.SfxEntry entry = DataTable.Singleton.GetEntry(index);

            if (entry == null)
            {
                Debug.LogError("SFX entry == null --" + index);
                return;
            }
            SoundManager.Singleton.PlayerSfx(entry);
            return;
        }


        private Dictionary<int, SfxManager.SfxHandle> _loopingSound = new Dictionary<int, SfxManager.SfxHandle>();
        
        /// <summary>
        /// Sfx Loop Sound Play ( 나와 다른 유저 모두 호출)
        /// </summary>
        /// <param name="index"></param>
        public void PlayCharacterLoopSfx(int index)
        {
            if (_loopingSound.ContainsKey(index))
            {
                Debug.LogWarning("PlayCharacterLoopSfx called more than once!");
            }
            else
            {
                SfxDatabaseSO.SfxEntry entry = DataTable.Singleton.GetEntry(index);
                if (entry == null)
                    return;

                if (IsLocalCharacter || _characterType == CharacterType.OutGame)
                {
                    SfxManager.SfxHandle soundHandle = SoundManager.Singleton.PlayLoopSfx(entry, this.transform);
                    _loopingSound.Add(index, soundHandle);
                }
                else if(entry._categoryData.soundType == SfxDatabaseSO.Category.eSoundType.Shared) // OtherPlayer
                {
                    if (_character.SqrMagnitude < entry._categoryData.hearingDistance)
                    {
                        // 거리 체크 후 
                        SfxManager.SfxHandle soundHandle = SoundManager.Singleton.OtherPlayLoopSfx(entry, this.transform);
                        _loopingSound.Add(index, soundHandle);
                    }
                }
            }
        }

        // Local Player만 호출 해야 함.
        public void LocalPlayerLoopSfx(int index)
        {
#if UNITY_EDITOR
            if (IsLocalCharacter == false)
            {
                Debug.LogError(" 이 메소드는 로컬 플레이만 호출 해야 함 !!!! - Name :" + this.gameObject.name);
            }
#endif
            
            if (_loopingSound.ContainsKey(index))
            {
                Debug.LogWarning("PlayCharacterLoopSfx called more than once!");
            }
            else
            {
                SfxDatabaseSO.SfxEntry entry = DataTable.Singleton.GetEntry(index);
                if (entry == null)
                    return;

                SfxManager.SfxHandle soundHandle = SoundManager.Singleton.PlayLoopSfx(entry, this.transform);
                _loopingSound.Add(index, soundHandle);
            }
        }

        public void StopLoopingSfx(int index)
        {
            if (_loopingSound.ContainsKey(index))
            {
               _loopingSound[index].LoopStop();
               _loopingSound.Remove(index);
            }
        }
        
    }
}
