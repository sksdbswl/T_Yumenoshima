using UnityEngine;
using REIW.EventLock;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class CharacterAnimationState : AnimationState<eAnimationType, eStateType, CharacterAnimationState, CharacterAnimationStateMachine, CharacterAnimation>, ICheckEventLockState, ICameraEventType
    {
        public override eStateType StateType => eStateType.NONE;

        protected CharacterAnimationMovement Movement => Animation?.Movement;
        protected CharacterAnimationParameters AnimationParameters => Animation?.Parameters;

        protected CharacterBase _character;
        protected CharacterBase Character
        {
            get
            {
                if (_character == null)
                {
                    _character = Animation?.Character;

                    if (_character != null)
                    {
                        IsLocal = _character.IsLocalCharacter;
                    }
                }
                return _character;
            }
        }
        protected bool IsLocal = false;
        
        
        protected CharacterAnimationStateMachine CharacterStateMacnine => (CharacterAnimationStateMachine)OwnerStateMachine;
        protected CharacterAnimationState CurrentState => OwnerStateMachine.CurrentState;
        protected eStateType CurrentStateType => Animation.CurrentState;
        

        public override (bool isChange, eStateType nextType) NextStateType
        {
            get
            {
                var immediateNext = CharacterStateMacnine.CheckImmediateNextStateType();
                if (immediateNext.hasNext)
                    return immediateNext;

                var stateType = StateType;
                if (!CanExitState)
                    return (true, stateType);

                eCharacterActionInputType inputype = Movement.CurrentActionInputType; 
                if (inputype != eCharacterActionInputType.NONE)
                {
                    eStateType sType = inputype.ConvertStateType();
                    if (CanChangeNextState(sType))
                        return (true, sType);
                }

                if (CanChangeNextAirborneState)
                    return (true, eStateType.AIRBORNE);

                return (false, DefaultStateType);
            }
        }

        protected virtual bool CanChangeNextDashState => StateType != eStateType.DASH;
        protected virtual bool CanChangeNextJumpState => StateType != eStateType.JUMP;
        protected virtual bool CanChangeNextParkourState => StateType != eStateType.PARKOUR;
        protected virtual bool CanChangeNextGrappleState => StateType != eStateType.GRAPPLE;
        protected virtual bool CanChangeNextAirborneState => StateType != eStateType.AIRBORNE && Movement.IsAirborne;
        protected virtual bool CanChangeNextMountState => StateType != eStateType.MOUNT && (Movement.IsGrounded || !Movement.IsAirborne); // Mount 입력에 따른 전환/사용 가능 조건 (ex: 공중 사용 불가)
        protected virtual bool CanChangeNextInteractionState => StateType != eStateType.INTERACTION;
        protected virtual bool CanChangeNextGatheringState => StateType != eStateType.GATHERING;
        protected virtual bool CanChangeNextFishingState => StateType != eStateType.FISHING;

        public override bool CanExitState => ExitState;

        protected virtual void Start()
        {
            base.Start();
        }

        protected override void BaseOnEnable()
        {
            base.BaseOnEnable();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            Animation.ResetEventData();
        }

        protected override void UpdateAnimationParameters()
        {
            Movement.UpdateForwardSpeedParameter();
            Movement.UpdateVerticalSpeedParameter();
        }

        protected override void SetUseRootMotion(in AnimationClip InAnimationClip)
        {
            var motionSettings = Animation.GetRootMotionSettings(InAnimationClip);
            Movement.UseHorizontalRootMotionPosition = motionSettings.posXZ ? CharacterRootMotionMode.Override : CharacterRootMotionMode.Ignore;
            Movement.UseVerticalRootMotionPosition = motionSettings.posY ? CharacterRootMotionMode.Override : CharacterRootMotionMode.Ignore;
            Movement.UseRootMotionRotation = motionSettings.rotation ? CharacterRootMotionMode.Override : CharacterRootMotionMode.Ignore;
        }

        protected bool CanChangeNextState(in eCharacterActionInputType InInputType)
        {
            return CanChangeNextState(InInputType.ConvertStateType());
        }

        protected bool CanChangeNextState(in eStateType InNextState)
        {
            switch (InNextState)
            {
                case eStateType.DASH:
                    return CanChangeNextDashState;
                case eStateType.JUMP:
                    return CanChangeNextJumpState;
                case eStateType.PARKOUR:
                    return CanChangeNextParkourState;
                case eStateType.GRAPPLE:
                    return CanChangeNextGrappleState;
                case eStateType.MOUNT:
                    return CanChangeNextMountState;
                case eStateType.INTERACTION:
                    return CanChangeNextInteractionState;
                case eStateType.GATHERING:
                    return CanChangeNextGatheringState;
                case eStateType.FISHING:
                    return CanChangeNextFishingState;
                case eStateType.AIRBORNE:
                    return CanChangeNextAirborneState;
            }

            return true;
        }

        protected void ChangeStaminaActionType(eStaminaActionType InStaminaActionType)
        {
            if (IsLocal)
                Character.EventBus.Post<ICharacterStateEventListener>(_ => _.OnChangeStaminaActionType(InStaminaActionType));
        }

        public virtual eEventLockType CurrentEventLockType => eEventLockType.None;
        public virtual eEventLockType ReleaseEventLockType => eEventLockType.None;
        public virtual IngameCameraSystem_Event.CameraEventType CameraEventType =>IngameCameraSystem_Event.CameraEventType.Default;

        public virtual Vector3 CameraEventOffset
        {
            get;
            set;
        }
    }
}