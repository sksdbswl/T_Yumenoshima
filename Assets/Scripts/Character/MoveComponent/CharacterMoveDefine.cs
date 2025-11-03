using UnityEngine;
using REIW.EventLock;

namespace REIW
{
    
    public enum CharacterMoveType
    {
        Grapple = 1 << 0,
        WallClimb = 1 << 1,
        Gliding = 1 << 2,

        Max = 3,
    }

    public enum CharacterMovePlayMode
    {
        None = -1,
        Normal = CharacterMoveType.Grapple | CharacterMoveType.WallClimb,
        Gliding = CharacterMoveType.Gliding,
    }

    public interface ICharacterMoveComponent
    {
        CharacterMoveType  MoveType { get; }
        
        void Initialize(ICharacterMoveController controller);
        void EnterComponent();
        void ExitComponent();
        void FixedUpdateComponent();
        void LateUpdateComponent();
        void UpdateInput(PlayerCharacterInputs inputs);
        bool UpdateVelocity(ref Vector3 velocity, float deltaTime);
        bool UpdateRotation(ref Quaternion rotation, float deltaTime);
        void DestroyComponent();
        void EnterFromPreviousComponentType(CharacterMovePlayMode prevmode);
    }

    public interface ICharacterMoveComponentGizmo
    {
        void OnDrawGizmos();
    }
    
    public interface ICharacterMoveController
    {
        CharacterBaseEventBus EventBus { get; }
        
        Transform CharacterTransform { get; }
        Vector3 Up => CharacterTransform.up;
        Vector3 Forward => CharacterTransform.forward;
        Vector3 Right => CharacterTransform.right;
        
        float Height { get; }
        float Radius { get; }
        
        // 현재 딛고 있는 오브젝트
        Collider GroundCollider { get; }
        // (Ground 상관 없이)오브젝트 위에 서있는지 여부
        bool IsStableOnCollider { get; }
        
        Vector3 Gravity { get; }
        float GravityMagnitude { get; }
        
        CharacterRootMotionMode ModeRootMotionHorizontalPos { get; }
        CharacterRootMotionMode ModeRootMotionVerticalPos { get; }
        CharacterRootMotionMode ModeRootMotionRotation { get; }

        void SetGravity(Vector3 gravity);
        void StartJump(bool directly);
        
        PlayerCharacterInputs CurrentInputs { get; }
    }

    public abstract class CharacterMoveComponentBase<T> : ICharacterMoveComponent, ICheckEventLockState
        where T : ScriptableObject
    {
        private T _data = null;
        protected T MovementData
        {
            get
            {
                if (_data == null)
                    _data = AssetManager.Singleton.GetCharacterMovementDataSO<T>(true);
                return _data;
            }
        }
        
        public ICharacterMoveController Controller { get; private set; }
        public LocalCharacter CurrentLocalCharacter => Controller as LocalCharacter;
        protected Transform CharacterTransform => Controller.CharacterTransform;
        
        public abstract CharacterMoveType MoveType { get; }

        public virtual void Initialize(ICharacterMoveController controller)
        {
            Controller = controller;
            CurrentLocalCharacter?.CharacterEventLockController.AddEventLockState(this);
        }

        public virtual void DestroyComponent()
        {
            if (_data != null)
            {
                if (AssetManager.IsCreated)
                    AssetManager.Singleton.ReleaseAsset(_data);
                _data = null;
            }
            
            CurrentLocalCharacter?.CharacterEventLockController.RemoveEventLockState(this);
        }

        public virtual void EnterComponent()
        {
            
        }

        public virtual void ExitComponent()
        {
            
        }

        public virtual void FixedUpdateComponent()
        {
            
        }

        public virtual void LateUpdateComponent()
        {
            
        }

        public virtual void UpdateInput(PlayerCharacterInputs inputs)
        {
            
        }

        public virtual bool UpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            return false;
        }

        public virtual bool UpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            return false;
        }

        public virtual void EnterFromPreviousComponentType(CharacterMovePlayMode prevmode)
        {
            
        }

        public virtual eEventLockType CurrentEventLockType => eEventLockType.None;
        public virtual eEventLockType ReleaseEventLockType => eEventLockType.None;
    }
    
    public interface IPlayModeState
    {
        CharacterMovePlayMode MovePlayMode { get; }
    }
    
    public interface IMoveComponentStateApplier
    {
        bool MoveComponentStateApply(PlayerCharacterInputs inputs);
    }
}