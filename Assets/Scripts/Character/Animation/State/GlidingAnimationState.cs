using UnityEngine;
using Animancer;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using REIW.EventLock;
using GlidingPool = UnityEngine.Pool.ObjectPool<REIW.GlidingAttachObject>;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class GlidingAnimationState : CharacterAnimationState, IPlayModeState
    {
        public override eStateType StateType => eStateType.GLIDING;
        public CharacterMovePlayMode MovePlayMode => CharacterMovePlayMode.Gliding;

        [SerializeField]
        private ClipTransition _glideStart;
        [SerializeField]
        private ClipTransition _gliding;
        [SerializeField]
        private ClipTransition _glideJump;

        private eAnimationType _currentAnimationType = eAnimationType.NONE;

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var nextState = base.NextStateType;
                if (nextState.isChange)
                    return nextState;

                if (_playingAniState != null)
                    return (true, eStateType.GLIDING);

                return base.NextStateType;
            }
        }

        public override bool CanExitState => true;
        protected override bool CanChangeNextAirborneState => false;

        public enum GlidingStateType
        {
            None,
            Start,
            Playing,
            Jump_Waiting,
            Jumping,
            Landing_Waiting,
            Landing,
//            Landing_End,
        }

        private GlidingStateType _glidingState = GlidingStateType.None;
        public GlidingStateType GlidingState
        {
            set
            {
                if (_glidingState >= GlidingStateType.Landing_Waiting)
                    return;

                _glidingState = value;
            }
            get => _glidingState;
        }

        private DeltaPoseSolver _deltaPoseSolver = null;
        private DeltaPoseSolver deltaPoseSolver
        {
            get
            {
                if (_deltaPoseSolver == null)
                {
                    Transform trans = Movement.BodyIK.references.root.FindAllChild("com");
                    _deltaPoseSolver = trans.GetComponent<DeltaPoseSolver>();
                    if (_deltaPoseSolver == null)
                        _deltaPoseSolver = trans.gameObject.AddComponent<DeltaPoseSolver>();
                }

                return _deltaPoseSolver;
            }
        }

        public Vector3 DeltaPosition => deltaPoseSolver.DeltaPosition;

        private CharacterMoveGlidingData _data = null;
        private CharacterMoveGlidingData MovementData
        {
            get
            {
                if (_data == null)
                    _data = AssetManager.Singleton.GetCharacterMovementDataSO<CharacterMoveGlidingData>(true);
                return _data;
            }
        }
        private GlidingAttachObject _attachObject = null;
        private Animator _attachAnimator = null;

        public override Vector3 CameraEventOffset { get; set; }
        
        public override eEventLockType CurrentEventLockType => eEventLockType.CharacterDash | eEventLockType.CharacterGraple | eEventLockType.CharacterMount;

        private LocalCharacter _localCharacter = null;
        protected override void Start()
        {
            base.Start();

            if (Character != null)
            {
                if (Character.IsLocalCharacter)
                {
                    _localCharacter = Character as LocalCharacter;
                    if (_localCharacter != null)
                    {
                        CameraEventOffset = _localCharacter.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveGliding>().CameraEventOffset;
                    }
                    else
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

        private void OnDestroy()
        {
            ReleaseAsset(ref _data);
        }

        private void ReleaseAsset<T>(ref T asset) where T : ScriptableObject
        {
            if (asset == null)
                return;

            if (AssetManager.IsCreated)
                AssetManager.Singleton.ReleaseAsset(asset);

            asset = null;
        }

        public override void OnEnterState()
        {
            base.OnEnterState();
            Movement.IsAirborne = true;
            _glidingState = GlidingStateType.Start;
        }

        public override void OnExitState()
        {
            base.OnExitState();

            _glidingState = GlidingStateType.None;
            _currentAnimationType = eAnimationType.NONE;

            _attachObject?.OnFinish();
            _attachObject = null;
        }

        // network 적용?
        public override void DisableStateNetwork()
        {
            base.DisableStateNetwork();

            _attachObject?.OnFinish();
            _attachObject = null;
        }

        public override bool UpdateState()
        {
            if (base.UpdateState() == false)
                return false;

            switch (_glidingState)
            {
                case GlidingStateType.None:
                    return true;

                case GlidingStateType.Start:
                {
                    var state = InternalPlayAnimation(eAnimationType.GLIDING_START);
                    SetAnimationEndEvent(state, OnAnimation_EndEvent);
                    AttachObject();
                    ChangeStaminaActionType(eStaminaActionType.Glide);
                }
                    break;

                case GlidingStateType.Playing:
                {
                    InternalPlayAnimation(eAnimationType.GLIDING_PLAYING);
                }
                    break;

                case GlidingStateType.Jump_Waiting:
                {
                    _glidingState = GlidingStateType.Jumping;
                    var state = InternalPlayAnimation(eAnimationType.GLIDING_JUMP);
                    SetAnimationEndEvent(state, OnAnimation_EndEvent);
                }
                    break;

                case GlidingStateType.Landing_Waiting:
                    _glidingState = GlidingStateType.Landing;
                    SetGlidingLand();
                    DetachObject();
                    break;

                default:
                    return true;
            }

            return true;
        }

        public override bool LateUpdateState()
        {
            if (base.LateUpdateState() == false)
                return false;

            Movement.VerticalSpeedParameter = -10.0f;
            return true;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            if (_currentAnimationType == InAnimationType)
                return null;

            _currentAnimationType = InAnimationType;

            ClipTransition clip;
            switch (InAnimationType)
            {
                case eAnimationType.GLIDING_START:
                    clip = _glideStart;
                    break;

                case eAnimationType.GLIDING_PLAYING:
                    clip = _gliding;
                    break;

                case eAnimationType.GLIDING_JUMP:
                    clip = _glideJump;
                    
                    // 
                    Character.AnimancerEvents.OnFxObjectEvent((int)_attachObject.Trail_Jump, _attachObject.Fx_Trail_JumpTransform);
                    //>
                    
                    break;

                default:
                    clip = _gliding;
                    break;
            }

            InitAttachObjectForNetworkCharacter();
            _playingAniState = Animation.PlayAnimancerFade(InAnimationType, clip, 0.2f);
            _playingAniState.NormalizedTime = 0;
            SetUseRootMotion(_playingAniState);
            deltaPoseSolver.Sample();
            PlayAnimationAttachObject(InAnimationType);

            return _playingAniState;
        }


        private int _loopingSound = 0;
        private void PlayCharacterLoopSfx(int loopSfxIndex)
        {
            if (Character.IsLocalCharacter)
            {
                _loopingSound = loopSfxIndex;
                Character.CharacterEffectSound.LocalPlayerLoopSfx(loopSfxIndex );
            }
        }

        private void PlayCharacterStopLoopSfx()
        {
            if (Character.IsLocalCharacter)
            {
                Character.CharacterEffectSound.StopLoopingSfx(_loopingSound );
            }
        }
            


        public void OnAnimation_EndEvent()
        {
            _glidingState = GlidingStateType.Playing;
        }

        private void SetGlidingLand()
        {
            if (this.CurrentState != this)
                return;

            CharacterStateMacnine.SetImmediateNextStateType(CharacterAnimationEnums.eStateType.AIRBORNE);
        }


        public override IngameCameraSystem_Event.CameraEventType CameraEventType
        {
            get => IngameCameraSystem_Event.CameraEventType.Custom;
        }

        private void AttachObject()
        {
            AttachObject(Character.GetPlayerNetObject().GlideSerialID);
        }

        private void AttachObject(uint serialID)
        {
            if (_attachObject != null)
                return;
            
            // 글라이더 정보에서 붙러와야함...
            Debug.LogError("Gliding ");
            
            GlideObjectDataSO.DataInfo info = GameDataModel.Singleton.GlideObjectDatas.GetDataInfo(0);
            if (info == null)
                return;

            GameObject obj = GameObject.Instantiate(info.AttachObject);
            _attachObject = obj.GetComponent<GlidingAttachObject>();
            
            if (_attachObject == null)
            {
                Debug.LogError("GlidingAttachObject: AttachObject: AttachObject is null!");
                _attachObject = obj.AddComponent<GlidingAttachObject>();
            }
            
            // _attachObject.AttachAction = () => Character?.VisualAttachment?.Attach(info.AttachBones, ref obj, info.LocalPositionOffset, Quaternion.Euler(info.LocalRotationOffset), info.LocalScaleOffset);
            
            _attachObject.AttachAction = () =>
            {
                if (Character == null || Character.VisualAttachment == null)
                    return;

                Character.VisualAttachment.Attach(info.AttachBones, ref obj, info.LocalPositionOffset, Quaternion.Euler(info.LocalRotationOffset), info.LocalScaleOffset);
                Character.AnimancerEvents.OnFxObjectEvent((int)_attachObject.Mount_Summon, _attachObject.Fx_Mount_SummonTransform);
                PlayCharacterLoopSfx((int)_attachObject.Trail_Loop_Sound);
                OnFxGlidingTrailEvent((int)_attachObject.Trail_Loop, _attachObject.Fx_Trail_1, _attachObject.Fx_Trail_2);
            };


            _attachObject.DetachAction = () =>
            {
                if (Character == null || Character.VisualAttachment == null)
                    return;

                PlayCharacterStopLoopSfx();
                
                Character.CharacterEffectSound.LocalPlayerSfx((int)eKnownSfxSound.SE_GlideEnd_Common);
                
                Character.VisualAttachment.Detach(info.AttachBones);
                OnFxGlidingTrailEventStop();
            };
            _attachAnimator = _attachObject.GetComponent<Animator>();            
            PlayAnimationAttachObject(eAnimationType.GLIDING_START);
        }

        private readonly string[] AttackAnimationTriggers = new string[]
        {
            "Summon", "Idle", "Jump",
        };
        
        private void PlayAnimationAttachObject(eAnimationType type)
        {
            if (_attachAnimator == null)
                return;

            string trigger = type switch
            {
                eAnimationType.GLIDING_START   => AttackAnimationTriggers[0],
                eAnimationType.GLIDING_PLAYING => AttackAnimationTriggers[1],
                eAnimationType.GLIDING_JUMP    => AttackAnimationTriggers[2],
                _                              => AttackAnimationTriggers[1],
            };

            _attachAnimator.SetTrigger(trigger);
        }

        private void DetachObject()
        {
            _attachObject.DetachObject();
            _attachObject = null;
        }

        private void InitAttachObjectForNetworkCharacter()
        {
            if (_attachObject != null)
                return;

            if (Character is not NetworkCharacter networkCharacter)
                return;

            // 추후 정보에 따라서...
            uint id  = networkCharacter.GetPlayerNetObject().GlideSerialID;
            AttachObject(id);
        }
        
        //Effect
        
        private LoopingPooledEffect fx1;
        private LoopingPooledEffect fx2;
        public async void OnFxGlidingTrailEvent(int type, Transform glidingLeft, Transform glidingRight)
        {
            if(glidingLeft == null)
                Debug.LogError("glidingLeft is null!");
            if(glidingRight == null)
                Debug.LogError("glidingRight is null!");
            
            EffectDatabaseSO.EffectEntry entry = DataTable.Singleton.GetEffectEntry(type);
            if (entry != null)
            {
                var ct = Character.GetCancellationTokenOnDestroy(); 
                    
                var pooled1 = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
                fx1 = pooled1 as LoopingPooledEffect;
                
                var pooled2 = await IndexedEffectPoolManager.Singleton.GetAsync(entry, ct);
                fx2 = pooled2 as LoopingPooledEffect;
                
                if (fx1 != null && glidingLeft)
                {
                    fx1.SpawnEffectAtSocket( glidingLeft, _attachObject.transform.forward);
                }
                else
                {
                    Debug.Log("FX Is NULL ! OnFxEventInt:" + type);
                }
                
                if (fx2 != null && glidingRight)
                {
                    fx2.SpawnEffectAtSocket( glidingRight, _attachObject.transform.forward);
                }
                else
                {
                    Debug.Log("FX Is NULL ! OnFxEventInt:" + type);
                }
            }
            else
            {
                Debug.Log("EffectEntry is null type=" + type);
            }
        }

        public async void OnFxGlidingTrailEventStop()
        {
            //Trail End
            if (fx1 != null)
            {
                fx1.ForceStop();
            }
            if (fx2 != null)
            {
                fx2.ForceStop();
            }
        }
        //>
        
        
    }

    public class DeltaPoseSolver : CacheMonoBehaviour
    {
        private Vector3 _prevRootPos;
        public Vector3 DeltaPosition => MyTransform.position - _prevRootPos;
        private Quaternion _prevRootRot;
        public Quaternion DeltaRotate => MyTransform.rotation * Quaternion.Inverse(_prevRootRot);

        private void Awake()
        {
            Sample();
        }

        private void FixedUpdate()
        {
            Sample();
        }

        public void Sample()
        {
            _prevRootPos = MyTransform.position;
            _prevRootRot = MyTransform.rotation;
        }
    }
}