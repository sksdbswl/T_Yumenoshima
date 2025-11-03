using UnityEngine;
using REIW.Animations.Character;

namespace REIW
{
    using REIW.EventLock;

    public partial class CharacterMoveGliding : CharacterMoveComponentBase<CharacterMoveGlidingData>, IMoveComponentStateApplier
    {
        public override CharacterMoveType MoveType => CharacterMoveType.Gliding;

        private GlidingAnimationState GlidingState => CurrentLocalCharacter.CurrentState as GlidingAnimationState;
        public Vector3 CameraEventOffset => MovementData.CameraEventOffset;

        public override void EnterComponent()
        {
            base.EnterComponent();
            InitIK();

            _progressTime = 0;
        }

        public override void ExitComponent()
        {
            base.ExitComponent();
            OnExitIK();
        }

        public override void DestroyComponent()
        {
            base.DestroyComponent();
            OnExitIK();
        }

        public override void UpdateInput(PlayerCharacterInputs inputs)
        {
            if (inputs.Jump)
                CurrentGlidingState = GlidingAnimationState.GlidingStateType.Landing_Waiting;
            if (inputs.MouseClickChecker.RightClicked)
                CurrentGlidingState = GlidingAnimationState.GlidingStateType.Jump_Waiting;
        }

        float autoForwardSpeedRatio => MovementData.AutoForwardSpeedRatio; // MaxMoveSpeed의 몇 %로 전진할지(1=풀속도)
        // 필요 파라미터 예시(GlidingData 쪽에 있으면 거기 값 사용)
        float Accel => MovementData.Accel; // m/s^2
        float Decel => MovementData.Decel; // m/s^2
        float MaxLateralAccel => MovementData.MaxLateralAccel; // m/s^2 (언더스티어 한계)
        float LateralFriction => MovementData.LateralFriction; // 6→12 정도로 ↑ (1/s) — 옆미끄럼 감쇠율
        float LateralBrake => MovementData.LateralBrake; // 옆 성분을 0으로 끌어당기는 제동 m/s^2
        float MaxSlipAngleDeg => MovementData.MaxSlipAngleDeg; // 허용되는 최대 슬립 각
        float LowSpeedSnap => MovementData.LowSpeedSnap; // 이 속도 이하면 방향 스냅

        private float _progressTime = 0;

        public override bool UpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            if (_progressTime == 0)
                velocity = Vector3.zero;

            _progressTime += deltaTime;
            

            // 입력 방향
            Vector3 desiredDir = CurrentLocalCharacter.CharacterMoveDir;
            bool noInput = desiredDir.sqrMagnitude <= Mathf.Epsilon;

            desiredDir = desiredDir.normalized;

            float targetSpeed;
            if (noInput)
            {
                // 입력이 없을 때도 앞으로 특정 비율로 전진
                targetSpeed = MovementData.MaxMoveSpeed * Mathf.Clamp01(autoForwardSpeedRatio);
            }
            else
            {
                // 입력 크기(보통 0~1)에 Max 속도를 곱함
                targetSpeed = MovementData.MaxMoveSpeed * CurrentLocalCharacter.CharacterMoveDir.magnitude;
            }

            // 현재 수평 속도
            Vector3 vXZ = new Vector3(velocity.x, 0f, velocity.z);
            float curSpeed = Mathf.Min(vXZ.magnitude, MovementData.MaxMoveSpeed);
            Vector3 curDir = (curSpeed > 1e-4f) ? (vXZ / curSpeed) : desiredDir;

            // ① 언더스티어: 속도에 따른 최대 요율 제한
            float maxYawRadPerSec = (curSpeed > 0.1f) ? (MaxLateralAccel / curSpeed) : 9999f;
            float maxYawThisFrame = maxYawRadPerSec * deltaTime;
            Vector3 newDir = Vector3.RotateTowards(curDir, desiredDir, maxYawThisFrame, float.PositiveInfinity);

            // ② 현재 속도를 newDir 기준 전/횡 성분으로 분해
            Vector3 up = Vector3.up;
            Vector3 latDir = Vector3.Cross(up, newDir).normalized;
            float fwdSpeedNow = Vector3.Dot(vXZ, newDir);
            float latSpeedNow = Vector3.Dot(vXZ, latDir);

            // ③ 전진 가감속: 목표 속도로
            float accel = (targetSpeed >= fwdSpeedNow) ? Accel : Decel;
            float fwdSpeed = Mathf.MoveTowards(fwdSpeedNow, targetSpeed, accel * deltaTime);

            // ④ 옆미끄럼 억제: 강한 감쇠 + 제동
            //   - 지수 감쇠로 기본 줄이고, LateralBrake로 0쪽으로 더 당김
            float latDecay = Mathf.Exp(-LateralFriction * deltaTime); // 남길 비율
            float latSpeed = latSpeedNow * latDecay;
            latSpeed = Mathf.MoveTowards(latSpeed, 0f, LateralBrake * deltaTime);

            // ⑤ 저속 시 스냅: 거의 정지면 옆 성분 즉시 0, 진행방향도 newDir로
            if (curSpeed <= LowSpeedSnap)
            {
                latSpeed = 0f;
                fwdSpeed = Mathf.MoveTowards(fwdSpeedNow, targetSpeed, (Accel + Decel) * deltaTime);
                newDir = desiredDir;
            }

            Vector3 foward = Vector3.zero;
            if (noInput == false && Mathf.Abs(desiredDir.x) > 0f && Mathf.Abs(desiredDir.z) < 0.01f)
                foward = CurrentLocalCharacter.Forward.normalized;
            else if (noInput == false && Mathf.Abs(desiredDir.z) > 0f && Mathf.Abs(desiredDir.x) < 0.01f)
                foward = CurrentLocalCharacter.Forward.normalized;

            Vector3 candVelXZ = newDir * fwdSpeed + latDir * latSpeed + foward;
            float slip = (candVelXZ.sqrMagnitude >  Mathf.Epsilon)
                ? Vector3.Angle(newDir, candVelXZ.normalized)
                : 0f;

            if (slip > MaxSlipAngleDeg)
            {
                float t = Mathf.Clamp01((slip - MaxSlipAngleDeg) / slip);
                Vector3 clampedDir = Vector3.Slerp(candVelXZ.normalized, newDir, t).normalized;
                candVelXZ = clampedDir * candVelXZ.magnitude;
            }

            velocity.x = candVelXZ.x;
            velocity.z = candVelXZ.z;

            return UpdateVelocityByState(ref velocity, deltaTime);
        }

        private GlidingAnimationState.GlidingStateType CurrentGlidingState
        {
            set => GlidingState.GlidingState = value;
            get => GlidingState.GlidingState;
        }

        private bool UpdateVelocityByState(ref Vector3 velocity, float deltaTime)
        {
            if (_airBorne == false && CurrentGlidingState == GlidingAnimationState.GlidingStateType.Playing)
            {
                velocity = Vector3.zero;
                CurrentGlidingState = GlidingAnimationState.GlidingStateType.Landing_Waiting;
                return true;
            }

            var state = CurrentGlidingState;

            float targetVy = velocity.y;

            if (state == GlidingAnimationState.GlidingStateType.Start)
            {
                targetVy = _progressTime < MovementData.DescentStartDelay ? UpdateJump(ref velocity) : UpdatePlay(ref velocity);
            }
            else if(state == GlidingAnimationState.GlidingStateType.Jumping)
            {
                targetVy = UpdateJump(ref velocity);
            }
            else if (state == GlidingAnimationState.GlidingStateType.Landing ||
                     state == GlidingAnimationState.GlidingStateType.Landing_Waiting)
            {
                velocity.y = 0;
                targetVy = Mathf.Min(MovementData.Gravity * deltaTime, MovementData.Gravity);
            }
            else if (state == GlidingAnimationState.GlidingStateType.Playing)
            {
                targetVy = UpdatePlay(ref velocity);
            }

            if (targetVy == float.MinValue)
                velocity.y = 0;
            else
            {
                float _velYRef = 0f;
                velocity.y = Mathf.SmoothDamp(velocity.y, targetVy, ref _velYRef, MovementData.PlaySmoothTime);
            }

            return true;

            float UpdateJump(ref Vector3 velocity)
            {
                float move_y = Mathf.Clamp01(GlidingState.DeltaPosition.y);
                float result = Mathf.Max(velocity.y, 0f);
                return Mathf.Min(result + MovementData.RisePower * deltaTime * move_y, MovementData.RiseMax);
            }

            float UpdatePlay(ref Vector3 velocity)
            {
                float result  = Mathf.Min(velocity.y, 0f);
                result = Mathf.Max(result - MovementData.Gravity * deltaTime, -MovementData.Gravity);
                return result;
            }
        }

        public bool CheckConditionGliding()
        {
            return !Physics.Raycast(CharacterTransform.position, Vector3.down, MovementData.CheckConditionHeight, MovementData.GroundLayer);
        }

        public override bool UpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            var sharpness = MovementData.RotationSharpness;
            Vector3 lookdir = CurrentLocalCharacter.CharacterLookDir;

            if (lookdir.sqrMagnitude <= 0f || sharpness <= 0f)
                return true;

            // Smoothly interpolate from current to target look direction
            var lerpValue = 1 - Mathf.Exp(-sharpness * deltaTime);
            var smoothedLookInputDirection = Vector3.Slerp(CurrentLocalCharacter.Forward, lookdir, lerpValue).normalized;
            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            rotation = Quaternion.LookRotation(smoothedLookInputDirection, Vector3.up);
            return true;
        }

        public bool MoveComponentStateApply(PlayerCharacterInputs curInputs)
        {
            if (CurrentLocalCharacter.CharacterMoveComponentsHandler.CurrentMovePlayMode != CharacterMovePlayMode.Normal)
                return false;
            if (CurrentLocalCharacter.IsStableOnCollider || curInputs.Jump == false)
                return false;

            if (CheckConditionGliding() == false)
                return false;

            if (CurrentLocalCharacter.CharacterEventLockController.IsEventLockType(eEventLockType.CharacterGlide))
                return false;

            if (UpdateGroundIK(MovementData.airborneGate) == false)
                return false;

            CurrentLocalCharacter.CharacterAnimation.Movement.IsJumpInput = false;
            CurrentLocalCharacter.CharacterAnimation.StateMachine.SetImmediateNextStateType(CharacterAnimationEnums.eStateType.GLIDING);
            return true;
        }
    }
}
