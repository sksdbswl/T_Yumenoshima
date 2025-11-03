using System;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    [Serializable]
    public class CharacterAnimationStateMachine : AnimationStateMachine<eAnimationType, eStateType, CharacterAnimationState, CharacterAnimationStateMachine, CharacterAnimation>
    {
        public NetworkAnimationState Network => GetAnimationState<NetworkAnimationState>(eStateType.NETWORK);
        public IdleAnimationState Idle => GetAnimationState<IdleAnimationState>(eStateType.IDLE);
        public RunAnimationState Run => GetAnimationState<RunAnimationState>(eStateType.RUN);
        public DashAnimationState Dash => GetAnimationState<DashAnimationState>(eStateType.DASH);
        public SprintAnimationState Sprint => GetAnimationState<SprintAnimationState>(eStateType.SPRINT);
        public WalkAnimationState Walk => GetAnimationState<WalkAnimationState>(eStateType.WALK);
        public AirborneAnimationState Airborne => GetAnimationState<AirborneAnimationState>(eStateType.AIRBORNE);
        public JumpAnimationState Jump => GetAnimationState<JumpAnimationState>(eStateType.JUMP);
        public GrappleAnimationState Parkour => GetAnimationState<GrappleAnimationState>(eStateType.PARKOUR);
        public GrappleAnimationState Grapple => GetAnimationState<GrappleAnimationState>(eStateType.GRAPPLE);
        public MountAnimationState Mount => GetAnimationState<MountAnimationState>(eStateType.MOUNT);
        public InteractionAnimationState Interaction => GetAnimationState<InteractionAnimationState>(eStateType.INTERACTION);
        public GatheringAnimationState Gathering => GetAnimationState<GatheringAnimationState>(eStateType.GATHERING);
        public FishingAnimationState Fishing => GetAnimationState<FishingAnimationState>(eStateType.FISHING);
        public GlidingAnimationState Gliding => GetAnimationState<GlidingAnimationState>(eStateType.GLIDING);

        protected override void SetSecondaryCheckStates()
        {
            if (!_statesRoot)
                return;

            _secondaryCheckNextStateList = _statesRoot.GetComponentsInChildren<PlayTargetAnimationState>();
        }

        private eStateType _immediate_nextstateType = eStateType.NONE;

        public bool IsImmediateNextStateType(CharacterAnimationState characterState)
        {
            if (_immediate_nextstateType == eStateType.NONE)
                return false;
            if (_immediate_nextstateType != characterState.StateType)
                return false;

            _immediate_nextstateType = eStateType.NONE;
            return true;
        }

        public void SetImmediateNextStateType(eStateType nextStateType)
        {
            if (CurrentState.StateType == _immediate_nextstateType)
                return;
            
            _immediate_nextstateType = nextStateType;   
        }
        
        public void ResetStateType(eStateType type = eStateType.NONE) => _immediate_nextstateType = type;

        public (bool hasNext, eStateType nextType) CheckImmediateNextStateType()
        {
            bool hasnext = _immediate_nextstateType != eStateType.NONE;
            return (hasnext, _immediate_nextstateType);
        }
    }
}
