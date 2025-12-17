using UnityEngine;

namespace REIW
{
    // Minimal placeholders to keep ParkourActionData usable in a dummy project.
    public enum eDirection { None, Left, Right }

    public static class MathUtility
    {
        public static bool Greater(float a, float b) => a > b;
    }

    public static class LogUtil
    {
        public static void Log(string msg) => Debug.Log(msg);
    }

    public abstract class ParkourActionData : ScriptableObject
    {
        [Tooltip("우선순위 (오름차순)")]
        [SerializeField] private int priority;

        [Header("Animation Settings (dummy)")]
        [Tooltip("애니메이션 길이")]
        [SerializeField] protected float animationLength;
        [Tooltip("발 체크하여 애니메이션 선택")]
        [SerializeField] private bool checkFootToSelectAnimation;

        [Header("Root Motion Settings")]
        [SerializeField] protected CharacterRootMotionMode horizontalPosRootMotion = CharacterRootMotionMode.Ignore;
        [SerializeField] protected CharacterRootMotionMode verticalPosRootMotion = CharacterRootMotionMode.Ignore;
        [SerializeField] protected CharacterRootMotionMode rotationRootMotion = CharacterRootMotionMode.Ignore;
        [SerializeField] protected float rootMotionHorizontalPos;
        [SerializeField] protected float rootMotionVerticalPos;
        [SerializeField] private bool rotateToTarget;

        [Header("Input Settings")]
        [SerializeField] protected bool manualOperation;
        [SerializeField] protected bool lockMoveInput = true;

        [Header("Motor Settings (dummy flags)")]
        [SerializeField] protected bool inactivationGroundSolving = true;
        [SerializeField] protected bool inactivationCollisionWithHitTarget = true;
        [SerializeField] protected bool syncColliderAnimationTransform = true;

        [Header("Additional Settings")]
        [SerializeField] protected float movementThreshold = 2f;
        [SerializeField] protected float startActionDelay;
        [SerializeField] protected float postActionDelay;

        public float AnimationLength => animationLength;
        public bool CheckFootToSelectAnimation => checkFootToSelectAnimation;

        public CharacterRootMotionMode HorizontalPosRootMotion => horizontalPosRootMotion;
        public CharacterRootMotionMode VerticalPosRootMotion => verticalPosRootMotion;
        public CharacterRootMotionMode RotationRootMotion => rotationRootMotion;

        public bool RotateToTarget => rotateToTarget;
        public bool ManualOperation => manualOperation;
        public bool LockMoveInput => lockMoveInput;
        public bool AtStartLockMoveInput => RotateToTarget || LockMoveInput;

        public bool InactivationGroundSolving => inactivationGroundSolving;
        public bool SyncColliderAnimationTransform => syncColliderAnimationTransform;

        public float MovementThreshold => movementThreshold;
        public float StartActionDelay => startActionDelay;
        public float PostActionDelay => postActionDelay;

        public eDirection Direction { get; protected set; }
        public float Duration { get; set; }

        protected virtual void InitializeParkourActionData() { }

        protected virtual void ResetParkourActionData()
        {
            Direction = eDirection.None;
            Duration = 0f;
        }

        public virtual void StartParkourAction()
        {
            InitializeParkourActionData();
        }

        public virtual void FinishParkourAction()
        {
            ResetParkourActionData();
        }

        public virtual int Compare(ParkourActionData a, ParkourActionData b)
        {
            return a.priority.CompareTo(b.priority);
        }

        public bool CheckIfPossible(float moveSpeed, bool isManualOperation)
        {
            if (manualOperation != isManualOperation)
                return false;

            return !MathUtility.Greater(movementThreshold, 0) || !MathUtility.Greater(movementThreshold, moveSpeed);
        }
    }
}
