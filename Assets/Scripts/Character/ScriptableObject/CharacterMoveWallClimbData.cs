using UnityEngine;

namespace REIW
{
    [CreateAssetMenu(fileName = "CharacterMoveWallClimbData", menuName = "ScriptableObject/CharacterMoveWallClimbData")]
    public class CharacterMoveWallClimbData : ScriptableObject
    {
        public float ChangeGravitySharpness => changeGravitySharpness;
        public LayerMask StableLayer => stableLayer;
        public LayerMask ObstacleLayer => obstacleLayer;
        public float RotationSharpness => rotationSharpness;
        public float DetectionRange => detectionRange;
        public float MinWallHeight => minWallHeight;
        public float MaxSlopeAngle => maxSlopeAngle;
        public float MaxApproachAngle => maxApproachAngle;
        public float SpaceDetectionDistance => spaceDetectionDistance;
        public float SnappingGravityStrength => snappingGravityStrength;
        public float SnapCompleteUpDot => snapCompleteUpDot;
        public float SnapCompleteFwdPerpMax => snapCompleteFwdPerpMax;
        public float FallingDetectionTryDelay => fallingDetectionTryDelay;
        public float FallingLimitTime => fallingLimitTime;
        
        
        [Header("Gravity & LayerMask")]
        [SerializeField] private float changeGravitySharpness = 10f; // Climb.Change State 에서 Gravity 변화 강도 값
        [SerializeField] private LayerMask stableLayer; // Climb 가능한 LayerMask
        [SerializeField] private LayerMask obstacleLayer; // 공간 감지 시 장애물로 인지되는 LayerMask
        [SerializeField] private float rotationSharpness = 12f; // 회전 보간 강도
        
        [Header("Detection Config")]
        [SerializeField] private float detectionRange = 1f;
        [SerializeField] private float minWallHeight = 4f;
        [SerializeField] private float maxSlopeAngle = 70f;
        [SerializeField] private float maxApproachAngle = 45f;
        [SerializeField] private float spaceDetectionDistance = 3f;
        
        [Header("Snapping Completion")]
        [SerializeField] private float snappingGravityStrength = 30f; // Snapping 상태 중력 강도 값
        [SerializeField] private float snapCompleteUpDot = 0.998f;       // Up·targetUp 임계치
        [SerializeField] private float snapCompleteFwdPerpMax = 0.25f;   // |dot(fwd, targetUp)| 허용 상한(수직≈0, 평행≈1)
        [SerializeField] private float fallingDetectionTryDelay = 0.1f;
        [SerializeField] private float fallingLimitTime = 1f;
        
        
    }
}
