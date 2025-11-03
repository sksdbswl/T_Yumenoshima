namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class NetworkAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.NETWORK;

        private bool _isMoving;

        public override bool LateUpdateState()
        {
            var isMoving = Animation.IsMoving;
            if (!isMoving)
                isMoving = (eAnimationType)Animation.CurrentAnimation > eAnimationType.IDLE_TYPE_END;

            if (isMoving != _isMoving)
            {
                Movement.EnableIK(isMoving || Movement.IsApplyingGrounderIK());
                _isMoving = isMoving;
            }

            return base.LateUpdateState();
        }

        protected override void UpdateAnimationParameters()
        {
            Movement.UpdateVerticalSpeedParameter(true);
        }
    }
}
