using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REIW.Animations;
using REIW.Animations.Character;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace REIW
{
    public enum CharacterRootMotionMode
    {
        Ignore,     // 루트모션 값을 무시
        Additive,   // 기본 이동 제어에 루트모션 값을 추가
        Override,   // 기본 이동 제어를 무시하고 루트모션 값만 사용
    }
    
    [Serializable]
    public class PlayerCharacterInputs
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;
        public bool JumpHold;
        public bool Parkour;
        public bool Grapple;
        public bool Walk;
        public bool Sprint;
        public bool Dash;
        public bool Mount;
        public bool WallClimb;
        public MouseClickChecker MouseClickChecker;
    }

    [Serializable]
    public struct CharacterSurfaceContactReport
    {
        public bool IsStableOnSurface;
        public Collider SurfaceCollider;
        public Vector3 SurfaceNormal;
        public Vector3 SurfacePoint;
    }
    
    public class CharacterBase : CacheMonoBehaviour, ICharacterMoveController
    {
        [Space(5)][Header("Network Character")]
        //[SerializeField] protected BasePlayerNetObject _playerNetObject;
        
        [Space(5)][Header("Collider")]
        [SerializeField] protected new CapsuleCollider collider;
        [SerializeField] protected TransformLinker colliderTransformLinker;
        
        [Space(5)][Header("Stable Movement")]
        [SerializeField] protected float maxMoveSpeed = 8f;             // 이동 최대 속도(루트모션 사용 안하는 경우)
        //[SerializeField] protected float maxSprintSpeed = 12f;          // 스프린트 최대 속도(루트모션 사용 안하는 경우)
        [SerializeField] protected float maxSlopeSpeed = 4f;            // 경사면에서 최대 속도(루트모션 사용 안하는 경우)
        [SerializeField] protected float movementSharpness = 15f;       // 이동 가속 보간 비율
        [SerializeField] protected float orientationSharpness = 15f;    // 회전 보간 비율
        
        [Space(5)][Header("Air Movement")]
        [SerializeField] protected float maxAirMoveSpeed = 8f;          // 공중 이동 최대 속도
        [SerializeField] protected float airAccelerationSpeed = 5f;     // 입력에 대한 공중 가속도
        [SerializeField] protected float airOrientationSharpness = 15f; // 공중 회전 보간 비율
        [SerializeField] protected float airMoveDrag = .1f;             // 기본 공중 감속도
        
        [Space(5)][Header("Misc")]
        [SerializeField] protected Vector3 gravityDir = Vector3.down;   // 중력 방향(인게임에서 이동에 따라 변경되는 변수)
        [SerializeField] protected float gravityMagnitude = 30;         // 중력 가속도
        [SerializeField] protected LayerMask groundLayer;               // 지면 판정 레이어
        [SerializeField] protected LayerMask stableLayer;               // 위에 서 있을 수 있는 오브젝트 판정 레이어
        
        [Space(5)][Header("Client Character")]
        [SerializeField] protected ClientCharacter clientCharacter;

        // public Animator CharacterAnimator => clientCharacter.CharacterAnimator;
        // public CharacterAnimationState CurrentState => CharacterAnimation.StateMachine.CurrentState;
        // public CharacterAnimation CharacterAnimation => clientCharacter.CharacterAnimation;
        // public CharacterVisualAttachment VisualAttachment => clientCharacter.VisualAttachment;
        // public CharacterCustomizer CharacterCustomizer => clientCharacter.CharacterCustomizer;
        // public AnimancerEvents AnimancerEvents => clientCharacter.AnimancerEvents;
        // public CharacterAvatarBoneMapper AvatarBoneMapper => clientCharacter.AvatarBoneMapper;
        // public CharacterActionEffect CharacterActionEffect => clientCharacter._characterActionEffect;
        
        [HideInInspector]
        public Animator CharacterAnimator;
        public CharacterAnimationState CurrentState => CharacterAnimation.StateMachine.CurrentState;
        
        [HideInInspector]
        public CharacterAnimation CharacterAnimation;
        [HideInInspector]
        public CharacterVisualAttachment VisualAttachment;
        //[HideInInspector]
        //public CharacterCustomizer CharacterCustomizer;
        [HideInInspector]
        public AnimancerEvents AnimancerEvents;
        [HideInInspector]
        public CharacterAvatarBoneMapper AvatarBoneMapper;
        [HideInInspector]
        public CharacterEffectSound CharacterEffectSound;
        
        public CharacterBaseEventBus EventBus { get; protected set; } = new();
       
        private ulong databaseID;
        public ulong DatabaseID
        {
            get => databaseID;
            set => databaseID = value;
        }
        
        public Transform CharacterTransform => transform;
        public Vector3 Up => transform.up;
        public Vector3 Forward => transform.forward;
        public Vector3 Right => transform.right;
        public float Height => collider.height;
        public float Radius => collider.radius;
        
        public Vector3 Gravity => gravityDir.normalized;
        public float GravityMagnitude => gravityMagnitude;
        
        // 고정 벡터 캐싱
        protected Vector3 Vector3Up = Vector3.up;
        protected Vector3 Vector3Forward = Vector3.forward;
        protected Vector3 Vector3Right = Vector3.right;
        protected Vector3 Vector3Zero = Vector3.zero;

        // 캐릭터가 보아야 하는 시선 방향
        private Vector3 _characterLookDir;
        public virtual Vector3 CharacterLookDir
        {
            get => _characterLookDir;
            set => _characterLookDir = value;
        }
        
        // 이동 입력값에 카메라 회전, 중력 방향을 고려하여 변환한 실제 이동 방향
        protected Vector3 characterMoveDir;
        public Vector3 CharacterMoveDir => characterMoveDir;
        
        // 현재 이동 속도
        protected Vector3 currentMoveVelocity;
        public virtual Vector3 CurrentMoveVelocity => currentMoveVelocity;

        // 현재 지면 회전 속도(외부에서 제어하기 위해 추가)
        protected float curOrientationSharpness;
        public virtual float CurrentOrientationSharpness
        {
            get => curOrientationSharpness;
            set => curOrientationSharpness = value;
        }
        
        // 현재 공중 최대 속도(루트모션 적용 시, 현재 속도를 유지하기 위해서 설정)
        protected float rootMotionAirMoveSpeed;
        protected float curMaxAirMoveSpeed;
        public virtual float CurMaxAirMoveSpeed
        {
            get => curMaxAirMoveSpeed;
            set => curMaxAirMoveSpeed = value;
        }

        // 지면 검사 상태값
        private CharacterSurfaceContactReport surfaceStatus;
        public CharacterSurfaceContactReport SurfaceStatus => surfaceStatus;
        public bool IsStableOnCollider => surfaceStatus.IsStableOnSurface;
        public Collider GroundCollider => surfaceStatus.SurfaceCollider;

        // 루트모션 적용 모드 
        public virtual CharacterRootMotionMode ModeRootMotionHorizontalPos { get; set; }
        public virtual CharacterRootMotionMode ModeRootMotionVerticalPos { get; set; }
        public virtual CharacterRootMotionMode ModeRootMotionRotation { get; set; }
        public virtual bool LockMoveInput { get; set; }
        public virtual PlayerCharacterInputs CurrentInputs => null;

        protected bool jumpRequested = false;
        protected bool jumpPrevFrame = false;
        protected float jumpUpVelocity;
        protected bool onJump;
        protected bool dashRequested = false;
        
        public event Action OnInitialized;

        public virtual bool IsLocalCharacter { get; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            curOrientationSharpness = orientationSharpness;
            curMaxAirMoveSpeed = maxAirMoveSpeed;
        }
#endif
        private bool isInitialized = false;
        
        // ────────────────────────────── Cancellation (lifecycle) ──────────────────────────────
        private CancellationTokenSource _lifecycleCts;

        protected virtual void OnEnable()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = new CancellationTokenSource();
        }

        protected virtual void OnDisable()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;
            
            CharacterEffectSound.StopWireAction();
            // 추가로 루핑 FX/SFX 정리 필요 시 여기에
        }
        

        // public BasePlayerNetObject GetPlayerNetObject()
        // {
        //     return _playerNetObject;
        // }
        
        public virtual void Initialize()
        {
            LogUtil.LogWarning("CharacterBase Initialize");
            
            //_playerNetObject = GetComponent<BasePlayerNetObject>();
            
            curOrientationSharpness = orientationSharpness;
            curMaxAirMoveSpeed = maxAirMoveSpeed;
            colliderTransformLinker?.RestoreParent();

            if (this.clientCharacter != null)
            {
                Destroy(clientCharacter.gameObject);
                clientCharacter = null;
            }

            //var newInstClientCharacter = AssetManager.Singleton.GetClientCharacter(_playerNetObject.Gender, _playerNetObject.Race);
            //newInstClientCharacter.transform.SetParent(transform);
            //newInstClientCharacter.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            clientCharacter = new ClientCharacter();
            //clientCharacter.Initialize(this, _playerNetObject.Race, _playerNetObject.Gender);
            
            CharacterAnimator = clientCharacter.CharacterAnimator;
            CharacterAnimation = clientCharacter.CharacterAnimation;
            VisualAttachment = clientCharacter.VisualAttachment;
            //CharacterCustomizer = clientCharacter.CharacterCustomizer;
            AnimancerEvents = clientCharacter.AnimancerEvents;
            AvatarBoneMapper = clientCharacter.AvatarBoneMapper;
            CharacterEffectSound = clientCharacter._characterEffectSound;
            
            
            isInitialized = true;
            OnInitialized?.Invoke();
        }

        //public void RefreshAppearance(AvatarData appearance) => clientCharacter.RefreshAppearance(appearance);

        // public void EquipClothes(PlayerEquipDto.PlayerEquipData topEquipData, PlayerEquipDto.PlayerEquipData bottomEquipData, PlayerEquipDto.PlayerEquipData shoesEquipData)
        // {
        //     clientCharacter.EquipClothes(topEquipData, bottomEquipData, shoesEquipData);
        // }
        
        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
            GroundedCheck(ref surfaceStatus);
        }

        protected virtual void LateUpdate()
        {
        }

        public virtual void StartJump(bool directly)
        {
            //Debug.LogError("StartJump");
            //Effect
            AnimancerEvents.OnFxEventInt( (int)eKnownEffect.JUMP_ACTION );
            //>
        }
        
        public virtual void SetGravity(Vector3 gravity)
        {
            gravityDir = gravity;
        }
        
        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool directly = true)
        {
            transform.SetPositionAndRotation(position, rotation);
        }
        
        public virtual void AddRootMotionPosition(Vector3 deltaPos)
        {
        }

        public virtual void AddRootMotionRotation(Quaternion deltaRot)
        {
        }
        
        public virtual void GroundedCheck(ref CharacterSurfaceContactReport report)
        {
        }
        
        
      
        
        [Tooltip("나와의 다른 유저간의 거리..")]
        public float SqrMagnitude =100;
       

        protected virtual void OnDestroy()
        {
            CharacterEffectSound.StopWireAction();
        }
        //>
        //>
    }
}