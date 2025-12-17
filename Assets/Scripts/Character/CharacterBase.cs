using System;
using UnityEngine;

namespace REIW
{
    /// <summary>
    /// Root motion handling modes (kept because ParkourActionData references it).
    /// </summary>
    public enum CharacterRootMotionMode
    {
        None = 0,
        Ignore,
        Additive,
        Override,
    }

    [Serializable]
    public class PlayerCharacterInputs
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Jump;
        public bool JumpHold;
        public bool Parkour;
        public bool Walk;
        public bool Sprint;
        public bool Dash;
        public bool Mount;
        public bool WallClimb;
    }

    [Serializable]
    public struct CharacterSurfaceContactReport
    {
        public bool IsStableOnSurface;
        public Collider SurfaceCollider;
        public Vector3 SurfaceNormal;
        public Vector3 SurfacePoint;
    }

    /// <summary>
    /// Minimal character base for a dummy/offline project.
    /// Network / data model / asset streaming dependencies removed.
    /// </summary>
    public class CharacterBase : MonoBehaviour
    {
        [Header("Collider")]
        [SerializeField] protected CapsuleCollider characterCollider;

        [Header("Movement (optional debug fields)")]
        [SerializeField] protected Vector3 gravityDir = Vector3.down;
        [SerializeField] protected float gravityMagnitude = 30f;

        // Events (kept)
        public CharacterBaseEventBus EventBus { get; protected set; } = new();

        // Look / move data (camera & character dummy usage)
        [SerializeField] private Vector3 characterLookDir = Vector3.forward;
        protected Vector3 characterMoveDir = Vector3.zero;
        protected Vector3 currentMoveVelocity = Vector3.zero;

        public virtual Vector3 CharacterLookDir
        {
            get => characterLookDir;
            set => characterLookDir = value;
        }

        public Vector3 CharacterMoveDir => characterMoveDir;
        public virtual Vector3 CurrentMoveVelocity => currentMoveVelocity;

        public Transform CharacterTransform => transform;
        public Vector3 Up => transform.up;
        public Vector3 Forward => transform.forward;
        public Vector3 Right => transform.right;

        public float Height => characterCollider ? characterCollider.height : 0f;
        public float Radius => characterCollider ? characterCollider.radius : 0f;

        public Vector3 Gravity => gravityDir.normalized;
        public float GravityMagnitude => gravityMagnitude;

        // Root motion mode flags (kept; no-op in dummy project)
        public virtual CharacterRootMotionMode ModeRootMotionHorizontalPos { get; set; }
        public virtual CharacterRootMotionMode ModeRootMotionVerticalPos { get; set; }
        public virtual CharacterRootMotionMode ModeRootMotionRotation { get; set; }

        public virtual bool LockMoveInput { get; set; }
        public virtual PlayerCharacterInputs CurrentInputs => null;

        public event Action OnInitialized;
        protected bool isInitialized;

        public virtual bool IsLocalCharacter { get; } = false;

        public virtual void Initialize()
        {
            isInitialized = true;
            OnInitialized?.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            GroundedCheck(ref surfaceStatus);
        }

        private CharacterSurfaceContactReport surfaceStatus;
        public CharacterSurfaceContactReport SurfaceStatus => surfaceStatus;
        public bool IsStableOnCollider => surfaceStatus.IsStableOnSurface;
        public Collider GroundCollider => surfaceStatus.SurfaceCollider;

        public virtual void SetGravity(Vector3 gravity) => gravityDir = gravity;

        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool directly = true)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        public virtual void AddRootMotionPosition(Vector3 deltaPos) { }
        public virtual void AddRootMotionRotation(Quaternion deltaRot) { }

        public virtual void GroundedCheck(ref CharacterSurfaceContactReport report)
        {
            // Dummy project: keep empty (or implement simple raycast if you want).
            report.IsStableOnSurface = false;
            report.SurfaceCollider = null;
            report.SurfaceNormal = Vector3.up;
            report.SurfacePoint = transform.position;
        }

        /// <summary>
        /// Helper for a dummy controller to inject movement values (so the camera can react).
        /// </summary>
        public virtual void SetMoveData(Vector3 moveDir, Vector3 velocity)
        {
            characterMoveDir = moveDir;
            currentMoveVelocity = velocity;
        }
    }
}
