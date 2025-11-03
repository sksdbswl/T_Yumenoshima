using UnityEngine;
using Animancer.Units;
using RootMotion.FinalIK;
using static Animancer.Validate;

namespace REIW.Animations.Character
{
    public partial class CharacterAnimationMovement
    {
        private const AvatarIKGoal NONE_AVATAR_IK_TYPE = (AvatarIKGoal)int.MinValue;
        private const int LEFT_FOOT_INDEX = 0;
        private const int RIGHT_FOOT_INDEX = 1;

        [Header("GrounderIK Settings")]
        [SerializeField, Meters] private float _checkLandingDistance = 0f;
        [SerializeField, Range(0, 1)] private float _airbornGrounderIKWeight = 0.01f;
        [SerializeField] private float _airborneGrounderIKMaxStep = 2;
        [SerializeField] private float _applyingGroundIKRotationOffset = 200f;
        [SerializeField, Degrees(Rule = Value.IsNotNegative)]
        private float _flatGroundAngle = 5f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _checkStopDelay = 2f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _stopGrounderIKFootSpeed = 0.01f;

        private FullBodyBipedIK _bodyIK;
        private GrounderFBBIK _grounderIK;

        private float _targetGrounderIKWeight;
        private float _stopStartTime;
        private ReadOnlyValue<float> _grounderIKWeight;
        private ReadOnlyValue<float> _grounderIKFootSpeed;
        private ReadOnlyValue<float> _grounderIKMaxStep;
        private ReadOnlyValue<bool> _grounderIKRotateSolver;

        public FullBodyBipedIK BodyIK => _bodyIK;
        public GrounderFBBIK GrounderIK => _grounderIK;

        private bool IsValidBodyIK => _bodyIK && _bodyIK.solver.initiated && _bodyIK.solver.IKPositionWeight > 0f;
        private bool IsValidGrounderIK => _grounderIK && _grounderIK.initiated && _grounderIK.weight > 0f;

        public bool IsGrounded => Character ? Character.IsStableOnCollider : (!IsValidGrounderIK || _grounderIK.solver.isGrounded);

        public bool IsLanding { get; set; }

        public bool IsAirborne
        {
            get => _isAirborne;
            set
            {
                _isAirborne = value;
                if (_isAirborne)
                    IsLanding = false;
                if (IsValidGrounderIK)
                {
                    EnableGrounderIK(true);
                    SetAirbornStateGrounderIKMaxStep(_isAirborne);

                    if (_isAirborne)
                        SetAirbornStateGrounderIKWeight(true);
                }
            }
        }

        public void EnableIK(bool InEnable, bool InUpdate = false)
        {
            EnableBodyIK(InEnable, InUpdate);
            EnableGrounderIK(InEnable, InUpdate);
        }

        public void EnableBodyIK(bool InEnable, bool InUpdate = false)
        {
            if (!_bodyIK || _bodyIK.enabled == InEnable)
                return;

            _bodyIK.enabled = InEnable;

            if (InEnable && InUpdate)
                _bodyIK.UpdateSolverExternal();
        }

        public void EnableGrounderIK(bool InEnable, bool InUpdate = false)
        {
            if (!_grounderIK || _grounderIK.enabled == InEnable)
                return;

            _grounderIK.enabled = InEnable;

            if (InEnable && InUpdate)
                _grounderIK.solver.Update();
        }

        public void CheckGroundedMaxStepGrounderIK()
        {
            if (!IsValidGrounderIK)
                return;

            if (IsGrounded && !_grounderIK.solver.isGrounded)
                _grounderIK.solver.maxStep += 0.1f;
        }

        public void SetAirbornStateGrounderIKMaxStep(bool InAirborne)
        {
            if (!IsValidGrounderIK)
                return;

            _grounderIK.solver.maxStep = InAirborne ? _airborneGrounderIKMaxStep : _grounderIKMaxStep;
        }

        public void SetAirbornStateGrounderIKWeight(bool InSet)
        {
            if (!IsValidGrounderIK)
                return;

            _targetGrounderIKWeight = InSet ? _airbornGrounderIKWeight : _grounderIKWeight;
            if (InSet)
                _grounderIK.weight = _airbornGrounderIKWeight;
        }

        public bool IsApplyingGrounderIK()
        {
            if (!IsValidGrounderIK)
                return false;

            return _grounderIK.solver.legs[LEFT_FOOT_INDEX].rotationOffset.eulerAngles.magnitude >
                   _applyingGroundIKRotationOffset ||
                   _grounderIK.solver.legs[RIGHT_FOOT_INDEX].rotationOffset.eulerAngles.magnitude >
                   _applyingGroundIKRotationOffset ||
                   !IsFlatGround();
        }

        public bool IsFlatGround()
        {
            if (!IsValidGrounderIK)
                return true;

            return Vector3.Angle(GetGroundNormal(_grounderIK.solver.legs[LEFT_FOOT_INDEX].IKPosition,
                _grounderIK.solver.legs[RIGHT_FOOT_INDEX].IKPosition), -Character.Gravity) < _flatGroundAngle;
        }

        public void GravityChange(bool InWorldGravity)
        {
            if (!IsValidGrounderIK)
                return;

            _grounderIK.solver.rotateSolver = !InWorldGravity || _grounderIKRotateSolver;
        }

        private Vector3 GetGroundNormal(Vector3 InLeftFoot, Vector3 InRightFoot)
        {
            var up = -Character.Gravity;
            if (!IsValidGrounderIK)
                return up;

            var normalL = up;
            var normalR = up;

            if (Physics.Raycast(InLeftFoot, -up, out var hit, _grounderIK.solver.maxStep,
                    _grounderIK.solver.layers))
                normalL = hit.normal;

            if (Physics.Raycast(InRightFoot, -up, out hit, _grounderIK.solver.maxStep, _grounderIK.solver.layers))
                normalR = hit.normal;

            return ((normalL + normalR) * 0.5f).normalized;
        }

        private void UpdateGrounderIK()
        {
            if (!IsValidGrounderIK)
                return;

            bool isMoving = _characterAnimation.IsMoving;
            if (isMoving)
                _stopStartTime = 0f;
            else if (_stopStartTime == 0f)
                _stopStartTime = Time.time;

            if (!isMoving && Time.time - _stopStartTime > _checkStopDelay)
                _grounderIK.solver.footSpeed = _stopGrounderIKFootSpeed;
            else
                _grounderIK.solver.footSpeed = _grounderIKFootSpeed;

            _grounderIK.solver.frontFootOverstepFallsDown = isMoving && IsGrounded;

            if (!IsLocalCharacter)
                _grounderIK.solver.quality = isMoving ? Grounding.Quality.Fastest : Grounding.Quality.Best;

            _grounderIK.weight = Mathf.Lerp(_grounderIK.weight, _targetGrounderIKWeight, Time.deltaTime * 10f);
        }

        private void UpdateAirborneState()
        {
            if (!IsValidGrounderIK)
                return;

            if (IsAirborne)
            {
                if (IsGrounded)
                {
                    IsAirborne = false;
                    IsLanding = true;
                }
                else if (_grounderIK.enabled && !IsLanding && CurrentMoveVelocity.y < 0f)
                {
                    var heightFromGround = float.MaxValue;
                    for (int i = 0; i < _grounderIK.solver.legs.Length; ++i)
                        heightFromGround = Mathf.Min(heightFromGround, _grounderIK.solver.legs[i].heightFromGround);

                    if (heightFromGround < _checkLandingDistance)
                        IsLanding = true;
                }
            }
            else if (!IsGrounded)
            {
                if (VerticalSpeedParameter < _checkAirborneVerticalSpeed)
                    IsAirborne = true;
            }
        }
    }
}
