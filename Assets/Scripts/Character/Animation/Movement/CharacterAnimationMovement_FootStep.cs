using System;
using Animancer.Units;
using RootMotion.FinalIK;
using UnityEngine;
using static Animancer.Validate;

namespace REIW.Animations.Character
{
    public partial class CharacterAnimationMovement
    {
        [Header("Grounded Foot Settings")]
        [SerializeField, Meters(Rule = Value.IsNotNegative)]
        private float _walkGroundedFrontFootCheckDistance = 0f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _walkGroundedFootCheckTime = 0f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _runGroundedFootCheckTime = 0f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _sprintGroundedFootCheckTime = 0f;

        [Header("Foot Step Settings")]
        [SerializeField, MetersPerSecond(Rule = Value.IsNotNegative)]
        private float _footStepDownSpeedMin = 0.1f;
        [SerializeField, MetersPerSecond(Rule = Value.IsNotNegative)]
        private float _footStepDownSpeedMax = 1.0f;
        [SerializeField, Seconds(Rule = Value.IsNotNegative)]
        private float _footStepCoolTime = 0.1f;
        [SerializeField] private float _footStepBasePower = 1.0f;

        private float[] _footGroundedTimes;
        private float[] _footStepCoolTimes;
        private bool[] _footGroundedStates;
        private Vector3[] _footStepPositions;

        public bool ForceFindGroundedFoot { set; private get; }
        public AvatarIKGoal JumpFoot { get; private set; }

        public AvatarIKGoal FrontFoot
        {
            get
            {
                AvatarIKGoal footType = NONE_AVATAR_IK_TYPE;

                if (IsValidGrounderIK)
                {
                    var groundedLegs = UnityEngine.Pool.ListPool<(AvatarIKGoal FootType, Vector3 IKPosition)>.Get();
                    if (_grounderIK.solver.legs[LEFT_FOOT_INDEX].isGrounded)
                        groundedLegs.Add((AvatarIKGoal.LeftFoot,
                            _grounderIK.solver.legs[LEFT_FOOT_INDEX].IKPosition));
                    if (_grounderIK.solver.legs[RIGHT_FOOT_INDEX].isGrounded)
                        groundedLegs.Add((AvatarIKGoal.RightFoot,
                            _grounderIK.solver.legs[RIGHT_FOOT_INDEX].IKPosition));

                    if (groundedLegs.Count == 0)
                    {
                        groundedLegs.Add((AvatarIKGoal.LeftFoot,
                            _grounderIK.solver.legs[LEFT_FOOT_INDEX].IKPosition));
                        groundedLegs.Add((AvatarIKGoal.RightFoot,
                            _grounderIK.solver.legs[RIGHT_FOOT_INDEX].IKPosition));
                    }

                    float footZ = float.MinValue;
                    for (int i = 0; i < groundedLegs.Count; ++i)
                    {
                        float z = Character.CharacterTransform.InverseTransformPoint(groundedLegs[i].IKPosition).z;
                        if (footZ < z)
                        {
                            footZ = z;
                            footType = groundedLegs[i].FootType;
                        }
                    }

                    UnityEngine.Pool.ListPool<(AvatarIKGoal FootType, Vector3 IKPosition)>.Release(groundedLegs);
                }

                return footType;
            }
        }

        private float GroundedFootCheckTime => IsSprint ? _sprintGroundedFootCheckTime :
            (IsWalking ? _walkGroundedFootCheckTime : _runGroundedFootCheckTime);

        public event Action<(AvatarIKGoal footType, float footPower, eKnownSfxSound groundTag)> FootStepEvent;

        private void UpdateFoots()
        {
            if (!IsValidGrounderIK)
                return;

            AvatarIKGoal prev = JumpFoot;
            JumpFoot = NONE_AVATAR_IK_TYPE;

            if (!_grounderIK.solver.isGrounded ||
                (!IsMoving && Mathf.Approximately(AnimationParameters.ForwardSpeed, 0f)))
            {
                for (int i = 0; i < _footGroundedTimes.Length; ++i)
                    _footGroundedTimes[i] = 0f;
                return;
            }

            float groundedFootCheckTime = GroundedFootCheckTime;
            float time = Time.time;
            float footZ = 0f;
            int footIndex = -1;

            for (int i = 0; i < _grounderIK.solver.legs.Length; ++i)
            {
                if (_grounderIK.solver.legs[i].isGrounded)
                {
                    float z = Character.CharacterTransform.InverseTransformPoint(_grounderIK.solver.legs[i].IKPosition).z;
                    if (_footGroundedTimes[i] == 0f && (ForceFindGroundedFoot || z > _grounderIK.solver.footRadius))
                        _footGroundedTimes[i] = time;

                    if (time - _footGroundedTimes[i] <= groundedFootCheckTime)
                    {
                        if (footIndex >= 0)
                        {
                            if (z > footZ)
                            {
                                footIndex = i;
                                footZ = z;
                            }
                        }
                        else
                        {
                            footIndex = i;
                            footZ = z;
                        }

                        JumpFoot = footIndex == LEFT_FOOT_INDEX ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                    }
                }
                else
                {
                    _footGroundedTimes[i] = 0f;
                }
            }

            if (JumpFoot != NONE_AVATAR_IK_TYPE)
            {
                return;
            }
            else if (ForceFindGroundedFoot)
            {
                for (int i = 0; i < _grounderIK.solver.legs.Length; ++i)
                {
                    if (_grounderIK.solver.legs[i].isGrounded)
                    {
                        float z = Character.CharacterTransform.InverseTransformPoint(_grounderIK.solver.legs[i].IKPosition).z;
                        if (footIndex >= 0)
                        {
                            if (z > footZ)
                            {
                                footIndex = i;
                                footZ = z;
                            }
                        }
                        else
                        {
                            footIndex = i;
                            footZ = z;
                        }

                        JumpFoot = footIndex == LEFT_FOOT_INDEX ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                    }
                }

                if (JumpFoot != NONE_AVATAR_IK_TYPE)
                    return;
            }

            if (IsWalking)
            {
                float foot0 = Character.CharacterTransform
                    .InverseTransformPoint(_grounderIK.solver.legs[LEFT_FOOT_INDEX].IKPosition).z;
                float foot1 = Character.CharacterTransform
                    .InverseTransformPoint(_grounderIK.solver.legs[RIGHT_FOOT_INDEX].IKPosition).z;
                if (foot0 - foot1 > _walkGroundedFrontFootCheckDistance)
                    JumpFoot = AvatarIKGoal.LeftFoot;
                else if (foot1 - foot0 > _walkGroundedFrontFootCheckDistance)
                    JumpFoot = AvatarIKGoal.RightFoot;
            }
        }

        private void UpdateFootStep()
        {
            if (FootStepEvent == null || !CheckGroundedFoot || !IsValidGrounderIK)
                return;

            for (int i = 0; i < _grounderIK.solver.legs.Length; ++i)
            {
                _footStepCoolTimes[i] -= Time.deltaTime;
                FootStepProcess(i, ref _footGroundedStates[i], ref _footStepPositions[i],  ref _footStepCoolTimes[i]);
            }
        }

        
        
        private void FootStepProcess(int InFootIndex, ref bool InWasGrounded, ref Vector3 InLastPos, ref float InCoolTime)
        {
            Grounding.Leg leg = _grounderIK.solver.legs[InFootIndex];
            bool grounded = leg.isGrounded && IsGrounded;
            Vector3 curPos = leg.IKPosition;
            float downSpeed = 0f;

            if (!InWasGrounded && grounded && InCoolTime <= 0f && leg.GetHitPoint.collider is { } hitCollider)
            {
                if (Time.deltaTime > 0f)
                {
                    Vector3 v = (curPos - InLastPos) / Time.deltaTime;
                    downSpeed = Vector3.Dot(v, -Character.Up);
                }

                if (downSpeed > _footStepDownSpeedMin)
                {
                    float t = Mathf.InverseLerp(_footStepDownSpeedMin, _footStepDownSpeedMax, downSpeed);
                    float footPower = _footStepBasePower * Mathf.Clamp01(t);
                    eKnownSfxSound soundType = eKnownSfxSound.None;

                    if (hitCollider.CompareTag(ReIWTags.Ground))
                        soundType = eKnownSfxSound.SE_Footstep_Run_Normal;
                    else if (hitCollider.CompareTag(ReIWTags.Grass))
                        soundType = eKnownSfxSound.SE_Footstep_Run_Grass;
                    else if (hitCollider.CompareTag(ReIWTags.Metal))
                        soundType = eKnownSfxSound.SE_Footstep_Run_Metal;
                    else if (hitCollider.CompareTag(ReIWTags.Water))
                        soundType = eKnownSfxSound.SE_Footstep_Run_Water;

                    FootStepEvent?.Invoke(
                        (InFootIndex == LEFT_FOOT_INDEX ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot,
                            footPower, soundType));
                    //LogUtil.Log($"FootGroundedEvent[{InFootIndex}] - tag: {leg.GetHitPoint.collider?.tag} / footPower: {footPower}");

                    InWasGrounded = true;
                    InCoolTime = _footStepCoolTime;
                }
            }

            if (!grounded)
                InWasGrounded = grounded;
            InLastPos = curPos;
        }

// #if UNITY_EDITOR
//         private void OnGUI()
//         {
//             if (_footGroundedStates[0])
//             {
//                 Vector3 screenPos = Camera.main.WorldToScreenPoint(_grounderIK.solver.legs[0].IKPosition);
//                 GUI.color = Color.blue;
//                 GUI.DrawTexture(new Rect(screenPos.x - 15, Screen.height - screenPos.y - 15, 30, 30), Texture2D.normalTexture);
//             }
//             if (_footGroundedStates[1])
//             {
//                 Vector3 screenPos = Camera.main.WorldToScreenPoint(_grounderIK.solver.legs[1].IKPosition);
//                 GUI.color = Color.red;
//                 GUI.DrawTexture(new Rect(screenPos.x - 15, Screen.height - screenPos.y - 15, 30, 30), Texture2D.normalTexture);
//             }
//         }
// #endif
    }
}
