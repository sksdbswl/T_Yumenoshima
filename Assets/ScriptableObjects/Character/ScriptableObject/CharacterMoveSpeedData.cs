using REIW.Animations.Character;
using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "CharacterMoveSpeedData", menuName = "ScriptableObject/CharacterMoveSpeedData")]
    public class CharacterMoveSpeedData : ScriptableObject
    {
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _runSpeed;
        [SerializeField] private float _dashSpeed;
        [SerializeField] private float _sprintSpeed;
        [SerializeField] private float _airborneSpeed;

        public float GetSpeed(CharacterAnimationEnums.eMoveType  InMoveType)
        {
            return InMoveType switch
            {
                CharacterAnimationEnums.eMoveType.WALK => _walkSpeed,
                CharacterAnimationEnums.eMoveType.RUN => _runSpeed,
                CharacterAnimationEnums.eMoveType.DASH => _dashSpeed,
                CharacterAnimationEnums.eMoveType.SPRINT => _sprintSpeed,
                CharacterAnimationEnums.eMoveType.AIRBORNE => _airborneSpeed,
                _ => 0f,
            };
        }

        public float GetSpeed(int InMoveType)
        {
            return GetSpeed((CharacterAnimationEnums.eMoveType)InMoveType);
        }
    }
}
