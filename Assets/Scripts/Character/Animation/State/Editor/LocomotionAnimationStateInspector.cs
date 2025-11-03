using System.Collections.Generic;
using UnityEditor;

namespace REIW.Animations.Character
{
    [CustomEditor(typeof(LocomotionAnimationState))]
    public class LocomotionAnimationStateInspector : AnimationStateInspector
    {
        protected readonly HashSet<string> _moveProperties = new()
        {
            "_moveStart",
            "_moveMixer",
        };

        protected readonly HashSet<string> _footStartMoveNormalizedTimeProperties = new()
        {
            "_leftFootStartMoveNormalizedTime",
            "_rightFootStartMoveNormalizedTime",
        };

        protected readonly HashSet<string> _turnProperties = new()
        {
            "_turnLeft",
            "_turnRight",
            "_turnLeftRotationData",
            "_turnRightRotationData",
            "_turnAngle",
            "_turnRootMotionRotationSpeed",
            "_turnRootMotionRoationSpeedNormalizedTime"
        };

        protected readonly HashSet<string> _quickTurnProperties = new()
        {
            "_quickTurnLeft",
            "_quickTurnRight",
            "_quickTurnLeftRotationData",
            "_quickTurnRightRotationData",
            "_quickTurnMoveSpeed",
            "_quickTurnAngle",
            "_quickTurnRootMotionRotationSpeed",
            "_continuousQuickTurnNormalizedTime",
            "_turnEnableStopNormalizedTime"
        };

        protected readonly HashSet<string> _stopProperties = new()
        {
            "_standStop",
            "_moveStop",
            "_turnEnableStopNormalizedTime"
        };
    }
}
