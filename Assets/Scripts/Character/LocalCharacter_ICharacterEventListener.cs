using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using KinematicCharacterController;
using REIW.Animations.Character;
using UnityEditor;
using UnityEngine;

namespace REIW
{
    public partial class LocalCharacter : IMoveGrappleEventListener, IMoveWallClimbEventListener, IGatheringEventListener, ICharacterStateEventListener
    {
        public void OnGrappleRequested(GrapplePoint target, Vector3 grapplePosition, float grappleDistance, bool isFar, Action<bool> funcStartGrapple)
        {
            if (!target)
                return;

            var lookDir = (target.Position - transform.position).normalized;
            lookDir.y = CharacterLookDir.y;
            CharacterLookDir = lookDir;

            var climbComp = CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveWallClimb>();
            climbComp.IsActivateWallClimb = false;
            
            LockMoveInput = true;
        }

        public void OnGrappleStarted(GrapplePoint target, float grappleMoveTime)
        {
            Debug.Log("OnGrappleStarted");
            
            onJump = false;
            motor.ForceUnground();
            colliderTransformLinker?.ChangeParent();
        }
        
     
        public void OnGrappleArrival()
        {
            Debug.Log("OnGrappleArrival");
            
            LockMoveInput = false;
        }

        public void OnGrappleLaunchRequested(Action<bool> funcStartLaunch)
        {
            motor.ForceUnground();
        }

        public void OnGrappleLaunchStarted()
        {
            motor.ForceUnground();
        }

        public void OnGrappleLaunchLanding()
        {
            colliderTransformLinker?.RestoreParent();
            CharacterLookDir = Vector3.zero;
        }

        public void OnGrapplePointTargeted(GrapplePoint prev, GrapplePoint target, Vector3 grapplePosition)
        {
            CharacterMoveGrapple moveGrapple  = CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGrapple>();
            bool isview = moveGrapple?.ShowGrapplePoint ?? true;
            // for debug
            if(prev != null)
                prev.GetComponent<Renderer>().enabled = false;
            if (target != null)
                target.GetComponent<Renderer>().enabled = isview;
            //>
            
            // Debug.LogWarning("OnGrapplePointTargeted:" + target);
            
            if (target != null)
                _grapplePointInstant.position = grapplePosition;
            else
                _grapplePointInstant.position = Vector3.zero;
            
            
            //>
            
        }

        public void OnGravityChangeStarted(bool isDownSnapping)
        {
            LogUtil.Log($"OnGravityChangeStarted. isDownSnapping:{isDownSnapping}".Color(Color.green));
            if (isDownSnapping)
            {
                motor.ForceUnground();
                CharacterAnimation.Movement.IsAirborne = true;
                CharacterAnimation.Movement.EnableGrounderIK(false);
            }
        }

        public void OnGravityChangeFinished(bool worldGravity)
        {
        }

        public async void OnWallClimbStarted()
        {
            if (_wallclimbEffect == null)
            {
                EffectDatabaseSO.EffectEntry entry =
                    DataTable.Singleton.GetEffectEntry((int)eKnownEffect.FX_WallClimb_Common_Loop);
                    
                if (entry != null)
                {
                    var ct = this.GetCancellationTokenOnDestroy(); 
                    
                    var pooled = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
                    _wallclimbEffect = pooled as CharacterAttachedLoopVfx;
                    
                    if (_wallclimbEffect != null)
                    {
                        Transform socket = AvatarBoneMapper.GetBoneTransform(_wallclimbEffect._socket);
            
                        Debug.Log("_wallclimbEffect._socket:" + _wallclimbEffect._socket);
                        Debug.Log("socket:" + socket);
            
                        _wallclimbEffect.SpawnEffectAtSocket(socket, transform.forward);
                    }
                    else
                    {
                        Debug.Log("FX Is NULL ! WireActionTest");
                    }
                }
                else
                {
                    Debug.Log("EffectEntry is null type= 1000");
                }
            }
            _wallclimbEffect.ActiveOn();
            
          
            AnimancerEvents.OnFxEventInt( (int)eKnownEffect.FX_WallClimb_Common_Start );
        }

        public void OnWallClimbFinished()
        {
            if (_wallclimbEffect != null)
            {
                _wallclimbEffect.ActiveOff();
            }
        }

        CharacterAttachedLoopVfx _wallclimbEffect;
        
        public void OnStartGathering(EnumGathering gatheringType, float gatheringSpeed = 1f)
        {
            prevMoveInput = Vector3.zero;
        }

        public void OnStopGathering()
        {
        }

        public void OnStartGatheringSuccess()
        {
        }

        public void OnChangeStaminaActionType(eStaminaActionType staminaActionType)
        {
            PlayerController.Instance.CurrentExecuteActionTypeStateType = staminaActionType;
        }
    }
}