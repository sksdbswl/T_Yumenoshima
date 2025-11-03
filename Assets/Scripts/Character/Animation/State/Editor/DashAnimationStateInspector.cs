using UnityEditor;

namespace REIW.Animations.Character
{
    [CustomEditor(typeof(DashAnimationState))]
    public class DashAnimationStateInspector : LocomotionAnimationStateInspector
    {
        protected override void Awake()
        {
            base.Awake();

            _hideProperties.Add("_standStop");
            _hideProperties.AddRange(_moveProperties);
            _hideProperties.AddRange(_footStartMoveNormalizedTimeProperties);
            _hideProperties.AddRange(_turnProperties);
            _hideProperties.AddRange(_quickTurnProperties);
        }
    }
}