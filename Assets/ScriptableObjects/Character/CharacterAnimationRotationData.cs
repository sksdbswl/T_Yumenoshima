using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "CharacterAnimationRotationData", menuName = "ScriptableObject/CharacterAnimationRotationData")]
    public class CharacterAnimationRotationData : ScriptableObject
    {
        [SerializeField] private AnimationCurve _rotationCurveX;
        [SerializeField] private AnimationCurve _rotationCurveY;
        [SerializeField] private AnimationCurve _rotationCurveZ;
        [SerializeField] private AnimationCurve _rotationCurveW;

        public AnimationCurve RotationCurveX { set => _rotationCurveX = value; }
        public AnimationCurve RotationCurveY { set => _rotationCurveY = value; }
        public AnimationCurve RotationCurveZ { set => _rotationCurveZ = value; }
        public AnimationCurve RotationCurveW { set => _rotationCurveW = value; }

        public Quaternion GetRotation(float InTime)
        {
            return new Quaternion(_rotationCurveX?.Evaluate(InTime) ?? 0f, _rotationCurveY?.Evaluate(InTime) ?? 0f,
                _rotationCurveZ?.Evaluate(InTime) ?? 0f, _rotationCurveW?.Evaluate(InTime) ?? 0f);
        }
    }
}
