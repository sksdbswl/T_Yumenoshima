using UnityEditor;

namespace REIW.Animations.Character
{
    [CustomEditor(typeof(SprintAnimationState))]
    public class SprintAnimationStateInspector : LocomotionAnimationStateInspector
    {
        protected override void Awake()
        {
            base.Awake();

            _hideProperties.AddRange(new string[]
            {
                "_checkLocomotionStartGroundedFootType",
                "_whenGroundedFootLocomotionNormalizedTime",
                "_quickTurnLeftRotationData",
                "_quickTurnRightRotationData",
            });

            _hideProperties.AddRange(_moveProperties);
            _hideProperties.AddRange(_turnProperties);
        }
    }
}