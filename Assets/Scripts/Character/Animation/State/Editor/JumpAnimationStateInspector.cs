using UnityEditor;

namespace REIW.Animations.Character
{
    [CustomEditor(typeof(JumpAnimationState))]
    public class JumpAnimationStateInspector : AnimationStateInspector
    {
        protected override void Awake()
        {
            base.Awake();

            _hideProperties.Add("_fall");
        }
    }
}