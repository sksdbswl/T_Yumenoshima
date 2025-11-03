using System;
using System.Collections.Generic;
using Animancer;
using Animancer.Units;
using RootMotion.FinalIK;
using UnityEngine;
using REIW.EventLock;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;
    using eMoveType = CharacterAnimationEnums.eMoveType;

    public class GrappleAnimationState : AirborneAnimationState
    {
        private enum eGrappleAnimationState
        {
            NONE,
            THROW,
            MOVE,
            ARRIVE,
            LAUNCH,
            FALL,
            LANDING
        }

        // public struct GrappleInformation
        // {
        //     internal GrapplePoint Target;
        //     internal Vector3 GrapplePosition;
        //     internal float GrappleDistance;
        //     internal float GrappleMoveTime;
        //     internal bool IsFar;
        //     internal Action<bool> StartGrappleCallback;
        //     internal Action<bool> StartLaunchCallback;
        //
        //     internal Transform TargetTransform => Target?.transform;
        //     internal bool IsValid => Target != null;
        //
        //     public static GrappleInformation Create(GrapplePoint InTarget, Vector3 InGrapplePosition,
        //         float InGrappleDistance, bool InFar, Action<bool> InStartGrappleCallback)
        //     {
        //         Debug.Log($"Is Far : {InFar}");
        //         return new GrappleInformation()
        //         {
        //             Target = InTarget,
        //             GrapplePosition = InGrapplePosition,
        //             GrappleDistance = InGrappleDistance,
        //             IsFar = InFar,
        //             StartGrappleCallback = InStartGrappleCallback
        //         };
        //     }
        //
        //     public float GetMoveAnimationSpeed(float InAnimationLength)
        //     {
        //         return Mathf.Clamp(InAnimationLength / GrappleMoveTime, 0.2f, 10f) * 0.95f;
        //     }
        //
        //     public void StartGrapple(bool InSuccess)
        //     {
        //         StartGrappleCallback?.Invoke(InSuccess);
        //         StartGrappleCallback = null;
        //     }
        //
        //     public void StartLaunch(bool InSuccess)
        //     {
        //         StartLaunchCallback?.Invoke(InSuccess);
        //         StartLaunchCallback = null;
        //     }
        // }

        public override eStateType StateType => eStateType.GRAPPLE;

        [SerializeField] private MixerTransition2D _throwMixer;
        [SerializeField] private MixerTransition2D _moveMixer;
        [SerializeField] private LinearMixerTransition _arriveMixer;
        [SerializeField] private LinearMixerTransition _launchMixer;

        [SerializeField, Meters(Rule = Validate.Value.IsNotNegative)]
        private float _moveShortAniMaxDistance = 7f;

        [SerializeField, Meters(Rule = Validate.Value.IsNotNegative)]
        private float _moveMediumAniMaxDistance = 15f;

        [SerializeField, Meters(Rule = Validate.Value.IsNotNegative)]
        private float _moveRegAniMaxDistance = 25f;

        [SerializeField, Meters(Rule = Validate.Value.IsNotNegative)]
        private float _moveSpinAniMaxDistance = 43f;

        [Tooltip("상단 Throw 모션의 적용 각도")] [SerializeField, Degrees]
        private float _throwUpAngle = 30f;

        [Tooltip("하단 Throw 모션의 적용 각도")] [SerializeField, Degrees]
        private float _throwDownAngle = -30f;

        [Tooltip("Throw 모션의 IK 적용 부위")] [SerializeField]
        private AvatarIKGoal[] _throwIKGoals;

        [Tooltip("Throw 모션의 IK 적용 Weight")] [SerializeField, Range(0, 1)]
        private float[] _throwIKPositionWeights;

        [Tooltip("Throw 모션의 IK 적용 Weight 변화 속도")] [SerializeField]
        private float _throwIKPositionWeightSpeed = 1f;

        //private GrappleInformation _grappleInfo;
        private eGrappleAnimationState _currentGrappleState;
        private eAnimationType _playingAnimationType;
        private eAnimationType _throwAnimationType;
        private eAnimationType _arriveAnimationType;
        private eAnimationType _launchAnimationType;

        // public override bool CanEnterState => _grappleInfo.IsValid;
        // public override bool CanExitState => (Movement.IsGrappleInput || _isLanding) && (IsArriveEnd || IsLandingEndByAniState || IsLandingEndByMovement);
        
        private bool IsArriveEnd => (_currentGrappleState == eGrappleAnimationState.ARRIVE && _playingAniState == null);
        private bool IsLandingEndByAniState => _currentGrappleState == eGrappleAnimationState.LANDING && _playingAniState == null;
        //private bool IsLandingEndByMovement => _currentGrappleState == eGrappleAnimationState.LANDING && Movement.MovementDirection != Vector3.zero &&
            //(_playingAniState != null && (Movement.GetMovementData<CharacterMoveGrapple>()?.AvailableLandingMove(_playingAniState.NormalizedTime) ?? false));

        public bool IsEnableThrowGrapple => _currentGrappleState == eGrappleAnimationState.LAUNCH ||
                                            _currentGrappleState == eGrappleAnimationState.FALL ||
                                            !enabled;

        public bool IsGrappling => enabled && eGrappleAnimationState.NONE < _currentGrappleState &&
                                   _currentGrappleState <= eGrappleAnimationState.ARRIVE;

        private FullBodyBipedIK BodyIK => Movement.BodyIK;
        private bool _oncameraeventGrapple = false;

        private LocalCharacter _localCharacter = null;

        protected override void Start()
        {
            base.Start();
            
            if (Character != null)
            {
                if (Character.IsLocalCharacter)
                {
                    _localCharacter =  Character as LocalCharacter;

                    if (_localCharacter == null)
                    {
                        Debug.LogError("_localCharacter == null!! ");
                    }
                }    
            }
            else
            {
                Debug.LogError("Unknown Error: Character == null!! ");
            }
        }

        protected override void OnEnable()
        {
            BaseOnEnable();
            PlayThrowAnimation();
            _oncameraeventGrapple = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Movement.IsJumpInput)
            {
                Movement.IsContinuousJump = true;

                // if (!_grappleInfo.IsFar)
                //     Movement.CurrentMoveType = eMoveType.SPRINT;
            }

            Movement.IsGrappleInput = false;
            _currentGrappleState = eGrappleAnimationState.NONE;
            _oncameraeventGrapple = false;
        }

        public override bool LateUpdateState()
        {
            if (!base.LateUpdateState())
                return false;

            UpdateCurrentState();
            UpdateThrowAnimationIK();

            return true;
        }

        protected override void UpdateLanding()
        {
            if (_currentGrappleState == eGrappleAnimationState.FALL)
                base.UpdateLanding();
        }

        private bool CanThrowGrapple => _currentGrappleState switch
        {
            eGrappleAnimationState.LAUNCH or eGrappleAnimationState.FALL => true,
            _                                                            => false,
        };
        
        private void UpdateCurrentState()
        {
            if (CanThrowGrapple == false)
                return;

            UpdateThrow();
        }

        private void UpdateThrow()
        {
            if (Movement.IsGrappleInput)
            {
                if (IsEnableThrowGrapple)
                    PlayThrowAnimation();
                // else
                //     _grappleInfo.StartGrapple(false);
            }
        }

        private void PlayThrowAnimation()
        {
            Reset();

            // _playingAniState = InternalPlayAnimation(GetThrowAnimationType());
            // SetAnimationEndEvent(_playingAniState, OnAnimation_ThrowEndEvent);

            Movement.SetAirbornStateGrounderIKWeight(true);
            Movement.IsLanding = false;
            Movement.IsGrappleInput = false;

            if (IsLocal)
            {
                // CharacterMoveGrapple grapple = _localCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGrapple>();
                // grapple?.StartThrow();    
            }
            

            //ChangeStaminaActionType(eStaminaActionType.Grapple);
        }

        private void PlayMoveAnimation()
        {
            // _playingAniState = InternalPlayAnimation(GetMoveAnimationType(),
            //     InCalculateSpeedFunc: (state) =>
            //     {
            //         if (state.IsValid() && _moveMixer.State != null)
            //         {
            //             _moveMixer.State.Parameter = GetMoveAnimationParameter();
            //             _moveMixer.State.RecalculateWeights();
            //         }
            //         return _grappleInfo.GetMoveAnimationSpeed(state.Length);
            //     });
            // SetThrowAnimationIK();
        }

        private void PlayArriveAnimation()
        {
            // _playingAniState = InternalPlayAnimation(GetArriveAnimationType());
            // SetAnimationEndEvent(_playingAniState, OnAnimation_ArriveEndEvent);

            Movement.SetAirbornStateGrounderIKWeight(false);
            Movement.IsLanding = false;
            _isLanding = true;
        }

        private void PlayLaunchAnimation()
        {
            // if (_grappleInfo.StartLaunchCallback == null)
            //     return;

            _enableAnyMovement = false;
            Movement.SetAirbornStateGrounderIKWeight(true);
            _playingAniState = InternalPlayAnimation(GetLaunchAnimationType());
            SetAnimationEndEvent(_playingAniState, OnAnimation_LaunchEndEvent);
        }

        private void PlayFallAnimation()
        {
            _playingAniState = InternalPlayAnimation(eAnimationType.GRAPPLE_FALL);
        }

        protected override bool PlayLandingAnimation()
        {
            if (!base.PlayLandingAnimation())
                return false;

            Movement.SetAirbornStateGrounderIKWeight(false);
            Movement.IsLanding = false;
            
            //Character.CharacterEffectSound.StopLoopingSfx((int)eKnownSfxSound.SE_Cynox_F_GrappleFall );
            
            return true;
        }

        public override bool IsPlayingAnimation(in float InNormalizedTime)
        {
            if (base.IsPlayingAnimation(InNormalizedTime))
                return true;
            if (Animancer.States.TryGet(_throwMixer, out var state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            if (Animancer.States.TryGet(_moveMixer, out state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            if (Animancer.States.TryGet(_arriveMixer, out state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            if (Animancer.States.TryGet(_launchMixer, out state) && state.IsPlaying &&
                state.NormalizedTime < InNormalizedTime)
                return true;
            return false;
        }

        private Dictionary<eAnimationType, eKnownSfxSound> _sounds = new Dictionary<eAnimationType, eKnownSfxSound>()
        {
            // { eAnimationType.GRAPPLE_TYPE_START,  eKnownSfxSound.   },
            { eAnimationType.GRAPPLE_THROW_UP, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            { eAnimationType.GRAPPLE_THROW, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            { eAnimationType.GRAPPLE_THROW_DOWN, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            { eAnimationType.GRAPPLE_THROW_AIR_UP, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            { eAnimationType.GRAPPLE_THROW_AIR, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            { eAnimationType.GRAPPLE_THROW_AIR_DOWN, eKnownSfxSound.SE_Cynox_F_GrappleThrow },
            
            { eAnimationType.GRAPPLE_MOVE_SHORT, eKnownSfxSound.SE_Cynox_F_GrappleMoveShortMid }, //once
            { eAnimationType.GRAPPLE_MOVE_MEDIUM, eKnownSfxSound.SE_Cynox_F_GrappleMoveMediumMid }, //once
            { eAnimationType.GRAPPLE_MOVE_REG, eKnownSfxSound.SE_Cynox_F_GrappleMoveReg },  // reg looping
            { eAnimationType.GRAPPLE_MOVE_SPIN, eKnownSfxSound.SE_Cynox_F_GrappleMoveSpin }, // once
            
            { eAnimationType.GRAPPLE_ARRIVE_SHORT, eKnownSfxSound.SE_Cynox_F_GrappleArriveShort },
            { eAnimationType.GRAPPLE_ARRIVE, eKnownSfxSound.SE_Cynox_F_GrappleArrive },
            { eAnimationType.GRAPPLE_ARRIVE_WOBBLE, eKnownSfxSound.SE_Cynox_F_GrappleArriveWobble },
            { eAnimationType.GRAPPLE_ARRIVE_SPIN_LANDING, eKnownSfxSound.SE_Cynox_F_GrappleArriveSpinLand },
            
            { eAnimationType.GRAPPLE_LAUNCH, eKnownSfxSound.SE_Cynox_F_GrappleLandJump },
            { eAnimationType.GRAPPLE_LAUNCH_SPIN, eKnownSfxSound.SE_Cynox_F_GrappleSpinLandJump },
            //
            
            { eAnimationType.GRAPPLE_LANDING, eKnownSfxSound.SE_Cynox_F_GrappleArriveLand}, //?
            { eAnimationType.GRAPPLE_FALL, eKnownSfxSound.SE_Cynox_F_GrappleFall }, // loop
            
            // { eAnimationType.GRAPPLE_TYPE_END, eKnownSfxSound. },

        };
        
        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            _playingAnimationType = InAnimationType;

            AnimancerState state = null;

            switch (InAnimationType)
            {
                case eAnimationType.GRAPPLE_THROW:
                case eAnimationType.GRAPPLE_THROW_UP:
                case eAnimationType.GRAPPLE_THROW_DOWN:
                case eAnimationType.GRAPPLE_THROW_AIR:
                case eAnimationType.GRAPPLE_THROW_AIR_UP:
                case eAnimationType.GRAPPLE_THROW_AIR_DOWN:
                {
                    _currentGrappleState = eGrappleAnimationState.THROW;
                    _throwAnimationType = InAnimationType;

                    state = Animation.PlayAnimation(InAnimationType, _throwMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if (state.IsValid() && _throwMixer.State != null)
                    {
                        if (_sounds.ContainsKey(InAnimationType))
                        {

                            // var ct = Character.GetCancellationTokenOnDestroy();
                            // if (Character.IsLocalCharacter)
                            // {
                            //     //Debug.LogWarning("InAnimationType:" +InAnimationType);
                            //     // Effect Wire Action
                            //     Character.CharacterEffectSound.WireTargetEvent(_grappleInfo.GrapplePosition, ct).Forget();
                            //     Character.CharacterEffectSound.WireActionTest(_grappleInfo.GrapplePosition, ct).Forget();
                            //     Character.CharacterEffectSound.AddSnapShot_GrapplePosition(_grappleInfo.GrapplePosition);
                            // }
                            // Character.CharacterEffectSound.PlayCharacterSfx((int)_sounds[InAnimationType]);
                        }

                    }

                    break;
                }
                case eAnimationType.GRAPPLE_MOVE_SHORT:
                case eAnimationType.GRAPPLE_MOVE_MEDIUM:
                case eAnimationType.GRAPPLE_MOVE_REG:
                case eAnimationType.GRAPPLE_MOVE_SPIN:
                {
                    _currentGrappleState = eGrappleAnimationState.MOVE;

                    //Debug.LogWarning("InAnimationType:" + InAnimationType);
                    
                    state = Animation.PlayAnimation(InAnimationType, _moveMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if (state.IsValid() && _moveMixer.State != null)
                    {
                        _moveMixer.State.Parameter = GetMoveAnimationParameter();
                        _moveMixer.State.RecalculateWeights();
                        
                        if (_sounds.ContainsKey(InAnimationType))
                        {
                            // if (InAnimationType != eAnimationType.GRAPPLE_MOVE_REG)
                            // {
                            //     Character.CharacterEffectSound.PlayCharacterSfx((int)_sounds[InAnimationType]);
                            // }
                            // // Debug.LogWarning("PlayLoopSfx");
                            // Character.CharacterEffectSound.PlayCharacterLoopSfx((int)eKnownSfxSound.SE_Cynox_F_GrappleMoveReg);           
                        }
                    }
                    break;
                }
                case eAnimationType.GRAPPLE_ARRIVE_SHORT:
                case eAnimationType.GRAPPLE_ARRIVE:
                case eAnimationType.GRAPPLE_ARRIVE_WOBBLE: //좁은땅
                case eAnimationType.GRAPPLE_ARRIVE_SPIN_LANDING:
                {
                    _currentGrappleState = eGrappleAnimationState.ARRIVE;
                    _arriveAnimationType = InAnimationType;

                    state = Animation.PlayAnimation(InAnimationType, _arriveMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if (state.IsValid() && _arriveMixer.State != null)
                    {
                        _arriveMixer.State.Parameter = GetArriveAnimationParameter();
                        _arriveMixer.State.RecalculateWeights();

                        if (_sounds.ContainsKey(InAnimationType))
                        {
                            //Character.CharacterEffectSound.PlayCharacterSfx((int)_sounds[InAnimationType]);
                        }
                    }

                    if (Character.IsLocalCharacter)
                    {
                        //Character.CharacterEffectSound.AddSnapShot_GrappleEnd();
                    }


                    //Character.CharacterEffectSound.StopWireAction();

                    try
                    {
                        // Debug.LogWarning("StopLoopSfx");
                        //Character.CharacterEffectSound.StopLoopingSfx((int)eKnownSfxSound.SE_Cynox_F_GrappleMoveReg);
                    }
                    catch (Exception e)
                    {
                        LogUtil.LogError("Unknown Try Catch :");
                    }
                    break;
                }
                case eAnimationType.GRAPPLE_LAUNCH:
                case eAnimationType.GRAPPLE_LAUNCH_SPIN:
                {
                    _currentGrappleState = eGrappleAnimationState.LAUNCH;
                    _launchAnimationType = InAnimationType;

                    state = Animation.PlayAnimation(InAnimationType, _launchMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if (state.IsValid() && _launchMixer.State != null)
                    {
                        _launchMixer.State.Parameter = GetLaunchAnimationParameter();
                        _launchMixer.State.RecalculateWeights();
                        if (_sounds.ContainsKey(InAnimationType))
                        {
                            //Character.CharacterEffectSound.PlayCharacterSfx((int)_sounds[InAnimationType]);
                        }
                    }
                    break;
                }
                case eAnimationType.GRAPPLE_LANDING:
                {
                    _currentGrappleState = eGrappleAnimationState.LANDING;

                    state = Animation.PlayAnimation(InAnimationType, _landingMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if (state.IsValid() && _landingMixer.State != null)
                    {
                        _landingMixer.State.Parameter = GetLandingAnimationParameter();
                        _landingMixer.State.RecalculateWeights();

                        if (_sounds.ContainsKey(InAnimationType))
                        {
                            //Character.CharacterEffectSound.PlayCharacterSfx((int)_sounds[InAnimationType]);
                        }
                    }
                    break;
                }
                case eAnimationType.GRAPPLE_FALL:
                    _currentGrappleState = eGrappleAnimationState.FALL;
                    
                    if (_sounds.ContainsKey(InAnimationType))
                    {
                       //Character.CharacterEffectSound.PlayCharacterLoopSfx((int)_sounds[InAnimationType]);
                    }
                    
                    state = Animation.PlayAnimation(InAnimationType, _fall, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
            }

            SetUseRootMotion(state);

            return state;
        }
        
        private void SetThrowAnimationIK(in Transform InTarget = null)
        {
            var bodyIK = BodyIK;
            if (!bodyIK)
                return;

            for (int i = 0; i < _throwIKGoals.Length; ++i)
            {
                switch (_throwIKGoals[i])
                {
                    case AvatarIKGoal.LeftHand:
                        bodyIK.solver.leftHandEffector.target = InTarget;
                        break;
                    case AvatarIKGoal.RightHand:
                        bodyIK.solver.rightHandEffector.target = InTarget;
                        break;
                }
            }
        }

        private void UpdateThrowAnimationIK()
        {
            if (_currentGrappleState == eGrappleAnimationState.NONE ||
                _currentGrappleState > eGrappleAnimationState.MOVE)
                return;

            var bodyIK = BodyIK;
            if (!bodyIK)
                return;

            for (int i = 0; i < _throwIKGoals.Length; ++i)
            {
                IKEffector hand = null;
                switch (_throwIKGoals[i])
                {
                    case AvatarIKGoal.LeftHand:
                        hand = bodyIK.solver.leftHandEffector;
                        break;
                    case AvatarIKGoal.RightHand:
                        hand = bodyIK.solver.rightHandEffector;
                        break;
                }

                if (hand != null)
                {
                    hand.positionWeight = Mathf.Clamp(hand.positionWeight + _throwIKPositionWeightSpeed * Time.deltaTime *
                        (hand.target == null ? -1f : 1f), 0, _throwIKPositionWeights[i]);
                }
            }
        }

        // public void SetGrappleInfo(GrappleInformation InGrappleInfo)
        // {
        //     _grappleInfo = InGrappleInfo;
        // }
        //
        // public void StartGrapple(in GrapplePoint InTarget, in float InGrappleMoveTime)
        // {
        //     _grappleInfo.Target = InTarget;
        //     _grappleInfo.GrappleMoveTime = InGrappleMoveTime;
        //
        //     PlayMoveAnimation();
        // }

        public void ArriveGrapple()
        {
            if (!enabled)
                return;

            PlayArriveAnimation();
        }

        public void LaunchRequested(in Action<bool> InStartLaunchCallback)
        {
            // if (!enabled || !_grappleInfo.IsFar)
            // {
            //     InStartLaunchCallback?.Invoke(false);
            //     return;
            // }
            //
            // _grappleInfo.StartLaunchCallback = InStartLaunchCallback;
            Movement.IsJumpInput = false;
            Movement.UseHorizontalRootMotionPosition = CharacterRootMotionMode.Ignore;
        }

        public void LandingLaunch()
        {
            if (!enabled)
                return;

            PlayLandingAnimation();
        }

        private float GetMoveAnimationMaxDistance(in eAnimationType InAnimationType)
        {
            return InAnimationType switch
            {
                eAnimationType.GRAPPLE_MOVE_SHORT => _moveShortAniMaxDistance,
                eAnimationType.GRAPPLE_MOVE_MEDIUM => _moveMediumAniMaxDistance,
                eAnimationType.GRAPPLE_MOVE_REG => _moveRegAniMaxDistance,
                eAnimationType.GRAPPLE_MOVE_SPIN => _moveSpinAniMaxDistance,
                _ => _moveShortAniMaxDistance
            };
        }

        // private eAnimationType GetThrowAnimationType()
        // {
        //     float angle =
        //         Movement.GetVerticalAngleToTarget(Character.CharacterTransform, _grappleInfo.TargetTransform);
        //     if (Movement.IsGrounded)
        //     {
        //         if (angle > _throwUpAngle)
        //             return eAnimationType.GRAPPLE_THROW_UP;
        //         if (angle < _throwDownAngle)
        //             return eAnimationType.GRAPPLE_THROW_DOWN;
        //         return eAnimationType.GRAPPLE_THROW;
        //     }
        //     else
        //     {
        //         if (angle > _throwUpAngle)
        //             return eAnimationType.GRAPPLE_THROW_AIR_UP;
        //         if (angle < _throwDownAngle)
        //             return eAnimationType.GRAPPLE_THROW_AIR_DOWN;
        //         return eAnimationType.GRAPPLE_THROW_AIR;
        //     }
        // }
        //
        // private eAnimationType GetMoveAnimationType()
        // {
        //     return _grappleInfo.GrappleDistance switch
        //     {
        //         var distance when distance < _moveShortAniMaxDistance => eAnimationType.GRAPPLE_MOVE_SHORT,
        //         var distance when distance < _moveMediumAniMaxDistance => eAnimationType.GRAPPLE_MOVE_MEDIUM,
        //         var distance when distance < _moveRegAniMaxDistance => eAnimationType.GRAPPLE_MOVE_REG,
        //         _ => eAnimationType.GRAPPLE_MOVE_SPIN
        //     };
        // }

        // private eAnimationType GetArriveAnimationType()
        // {
        //     return _grappleInfo.Target != null
        //         ? _grappleInfo.Target.GetArriveAnimationType(_playingAnimationType)
        //         : eAnimationType.GRAPPLE_ARRIVE;
        // }

        private eAnimationType GetLaunchAnimationType()
        {
            switch (_arriveAnimationType)
            {
                case eAnimationType.GRAPPLE_ARRIVE_SPIN_LANDING:
                    return eAnimationType.GRAPPLE_LAUNCH_SPIN;
                default:
                    return eAnimationType.GRAPPLE_LAUNCH;
            }
        }

        protected override eAnimationType GetLandingAnimationType()
        {
            return eAnimationType.GRAPPLE_LANDING;
        }

        private Vector2 GetThrowAnimationParameter()
        {
            switch (_playingAnimationType)
            {
                default:
                case eAnimationType.GRAPPLE_THROW_UP:
                    return new Vector2(1, 1);
                case eAnimationType.GRAPPLE_THROW:
                    return new Vector2(1, 2);
                case eAnimationType.GRAPPLE_THROW_DOWN:
                    return new Vector2(1, 3);
                case eAnimationType.GRAPPLE_THROW_AIR_UP:
                    return new Vector2(2, 1);
                case eAnimationType.GRAPPLE_THROW_AIR:
                    return new Vector2(2, 2);
                case eAnimationType.GRAPPLE_THROW_AIR_DOWN:
                    return new Vector2(2, 3);
            }
        }

        private Vector2 GetMoveAnimationParameter()
        {
            var x = _playingAnimationType switch
            {
                eAnimationType.GRAPPLE_MOVE_SHORT => 1,
                eAnimationType.GRAPPLE_MOVE_MEDIUM => 2,
                eAnimationType.GRAPPLE_MOVE_REG => 3,
                eAnimationType.GRAPPLE_MOVE_SPIN => 4,
                _ => 1
            };

            var y = 1;
            switch (_playingAnimationType)
            {
                case eAnimationType.GRAPPLE_MOVE_SHORT:
                case eAnimationType.GRAPPLE_MOVE_MEDIUM:
                    y = _throwAnimationType switch
                    {
                        eAnimationType.GRAPPLE_THROW_UP => 1,
                        eAnimationType.GRAPPLE_THROW => 2,
                        eAnimationType.GRAPPLE_THROW_DOWN => 3,
                        eAnimationType.GRAPPLE_THROW_AIR_UP => 1,
                        eAnimationType.GRAPPLE_THROW_AIR => 2,
                        eAnimationType.GRAPPLE_THROW_AIR_DOWN => 3,
                        _ => 1
                    };
                    break;
            }

            return new Vector2(x, y);
        }

        private int GetArriveAnimationParameter()
        {
            return _arriveAnimationType switch
            {
                eAnimationType.GRAPPLE_ARRIVE_SHORT => 1,
                eAnimationType.GRAPPLE_ARRIVE => 2,
                eAnimationType.GRAPPLE_ARRIVE_WOBBLE => 3,
                eAnimationType.GRAPPLE_ARRIVE_SPIN_LANDING => 4,
                _ => 1
            };
        }

        private int GetLaunchAnimationParameter()
        {
            return _launchAnimationType switch
            {
                eAnimationType.GRAPPLE_LAUNCH => 1,
                eAnimationType.GRAPPLE_LAUNCH_SPIN => 2,
                _ => 1
            };
        }

        protected override Vector2 GetLandingAnimationParameter()
        {
            return new Vector2(_playingAnimationType switch
            {
                eAnimationType.GRAPPLE_LANDING => 1,
                _ => 1
            }, 0);
        }

        // private void OnAnimation_ThrowEndEvent()
        // {
        //     _grappleInfo.StartGrapple(true);
        // }
        //
        // private void OnAnimation_ArriveEndEvent()
        // {
        //     _playingAniState = null;
        //     _grappleInfo.StartLaunch(false);
        // }

        private void OnAnimation_LaunchEndEvent()
        {
            if (_currentGrappleState != eGrappleAnimationState.LAUNCH)
                return;

            PlayFallAnimation();
        }

        // public void OnAnimation_EnableAnyMovementArriveEvent(int InArriveType)
        // {
        //     if (GetArriveAnimationParameter() != InArriveType)
        //         return;
        //
        //     _enableAnyMovement = true;
        //     _grappleInfo.StartLaunch(false);
        // }
        //
        // public void OnAnimation_PlayLaunchAnimationEvent(int InArriveType)
        // {
        //     if (GetArriveAnimationParameter() != InArriveType)
        //         return;
        //
        //     _oncameraeventGrapple = false;
        //     PlayLaunchAnimation();
        // }
        //
        // public void OnAnimation_StartLaunchEvent(int InLaunchType)
        // {
        //     if (GetLaunchAnimationParameter() != InLaunchType)
        //         return;
        //
        //     _grappleInfo.StartLaunch(true);
        // }

//        private bool IsLandingAirBone => _isLanding && Movement.CurrentMoveType == eMoveType.AIRBORNE;
        public override eEventLockType CurrentEventLockType
        {
            get
            {
                int eventlock = (int)(CanThrowGrapple ? base.CurrentEventLockType : eEventLockType.CharacterGlide);
                return (eEventLockType)eventlock;
            }
        }
        
        public override eEventLockType ReleaseEventLockType
        {
            get
            {
                int release = (int)eEventLockType.CharacterMove;
                release |= (int)eEventLockType.CharacterInputLock;
                release |= (int)eEventLockType.CameraRotate;
                release |= (int)eEventLockType.CharacterJump;
                return (eEventLockType)release;
            }
        }

        // public override IngameCameraSystem_Event.CameraEventType CameraEventType
        // {
        //     get
        //     {
        //         if (_oncameraeventGrapple)
        //             return IngameCameraSystem_Event.CameraEventType.Grapple;
        //
        //         return base.CameraEventType;
        //     }
        // }

// #if UNITY_EDITOR
//         private void OnGUI()
//         {
//             if (_currentGrappleState == eGrappleAnimationState.THROW && PlayerController.Instance.IsStandalone)
//             {
//                 Vector3 screenPos = Camera.main.WorldToScreenPoint(_grappleInfo.GrapplePosition);
//                 GUI.DrawTexture(new Rect(screenPos.x - 15, Screen.height - screenPos.y - 15, 30, 30), Texture2D.normalTexture);
//             }
//         }
// #endif
    }
}