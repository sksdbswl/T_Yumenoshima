using UnityEditor;
using UnityEngine;

namespace REIW.Animations.Character
{
    [CustomEditor(typeof(PlayTargetAnimationState), true)]
    public class PlayTargetAnimationStateInspector : AnimationStateInspector
    {
        private bool IsPlayTargetAnimationState => serializedObject.targetObject.GetType() == typeof(PlayTargetAnimationState);

        protected override void Awake()
        {
            base.Awake();

            _hideProperties.Add("_logic");
        }

        protected override void PreDrawProperties()
        {
            base.PreDrawProperties();

            bool enabledGUI = GUI.enabled;
            GUI.enabled = IsPlayTargetAnimationState;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_logic._playAnimationType"));
            GUI.enabled = enabledGUI;
        }
    }
}
