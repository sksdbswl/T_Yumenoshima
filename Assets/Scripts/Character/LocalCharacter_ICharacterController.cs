using System;
using KinematicCharacterController;
using UnityEngine;
using System.Linq;

namespace REIW
{
    using REIW.EventLock;
    
    /// <summary>
    /// 기본적으로 KCC의 ExampleCharacterController.cs를 기반으로 작성되었습니다
    /// 필요할 경우 ExampleCharacterController.cs를 참고해주세요
    /// </summary>
    public partial class LocalCharacter : ICharacterController
    {
        private bool wasGroundedLastFrame;
        private RaycastHit[] internalCharacterHits = new RaycastHit[KinematicCharacterMotor.MaxHitsBudget];

        /// Motor에서 FixedUpdate 단계에서 실행하는 회전치 계산 함수
        /// 원하는 계산을 해서 currentRotation에 넣어두면 Motor에서 그 값을 사용
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var baseRotation = currentRotation;
            
            // 기존에 구해둔 characterLookDir 방향으로 회전 보간 
            var sharpness = IsStableOnCollider ? curOrientationSharpness : airOrientationSharpness;
            if (CharacterLookDir.sqrMagnitude > 0f && sharpness > 0f)
            {
                // Smoothly interpolate from current to target look direction
                var lerpValue = 1 - Mathf.Exp(-sharpness * deltaTime);
                var smoothedLookInputDirection = Vector3.Slerp(Forward, CharacterLookDir, lerpValue).normalized;
                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Up);
            }

            // 컴포넌트에 의한 회전값 증감 계산
            var fixRotation = _characterMoveComponentsHandler.UpdateRotation(ref currentRotation, deltaTime);
            if (fixRotation)
            {
                animatorDeltaRotation = Quaternion.identity;
                return;
            }

            // 루트모션의 회전값을 현재 중력값을 기준으로 재계산
            var gravityAlign = Quaternion.FromToRotation(Vector3.up, Up.normalized);
            var correctAnimRot = gravityAlign * animatorDeltaRotation * Quaternion.Inverse(gravityAlign);
            switch (modeRootMotionRotation)
            {
                case CharacterRootMotionMode.Additive:
                    currentRotation = correctAnimRot * currentRotation;
                    break;
                case CharacterRootMotionMode.Override:
                    currentRotation = correctAnimRot * baseRotation;
                    break;
            }
            
            // 기록해두었던 루트모션 회전값 초기화
            animatorDeltaRotation = Quaternion.identity;
        }

        /// Motor에서 FixedUpdate 단계에서 실행하는 속도 계산 함수
        /// 원하는 계산을 해서 currentVelocity에 넣어두면 Motor에서 그 값을 사용
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (_characterMoveComponentsHandler.CurrentMovePlayMode)
            {
                case CharacterMovePlayMode.Gliding:
                    UpdateVelocity_Gilding(ref currentVelocity, deltaTime);
                break;
                
                default:
                    UpdateVelocity_Default(ref currentVelocity, deltaTime);
                break;
            }
            
            // 컴포넌트에 의한 속도 증감 계산
            var fixVelocity = _characterMoveComponentsHandler.UpdateVelocity(ref currentVelocity, deltaTime);
            if (!fixVelocity)
            {
                // 루트모션에 의한 속도 증감 계산
                UpdateVelocity_RootMotion(ref currentVelocity, deltaTime);
            }

            if (IsEventLockType(eEventLockType.CharacterMove))
                currentVelocity = Vector3.zero;
            
            // 결정된 속도 할당 및 투르모션 이동값 초기화
            currentMoveVelocity = currentVelocity;
            animatorDeltaPosition = Vector3.zero;
        }

        private void UpdateVelocity_Gilding(ref Vector3 currentVelocity, float deltaTime)
        {
            if (IsStableOnCollider == false)
            {
                if (currentVelocity.y < 0f)
                    motor.SetGroundSolvingActivation(true);
            }
        }

        public void UpdateVelocity_Default(ref Vector3 currentVelocity, float deltaTime)
        {
            if (deltaTime <= 0)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            var baseVelocity = currentVelocity;
            if (IsStableOnCollider)
            {
                // 지면에 닿아있을 경우의 속도 계산
                UpdateVelocity_Ground(ref currentVelocity, deltaTime);
            }
            else
            {
                // 공중에서 이동 방향 처리
                UpdateVelocity_AirMove(ref currentVelocity, deltaTime);
                // 공중에서의 속도 계산
                UpdateVelocity_Air(ref currentVelocity, baseVelocity, deltaTime);

                if (onJump && currentVelocity.y < 0f)
                    motor.SetGroundSolvingActivation(true);
            }

            // 랜딩 체크
            if (onJump && motor.GroundingStatus.IsStableOnGround)
            {
                EventBus.Post<ICharacterBaseEventListener>(_ => _.OnJumpLanded());
                onJump = false;
            }

            // 점프 입력시 수직 속도 증가치 추가
            UpdateVelocity_Jump(ref currentVelocity, deltaTime);
        }

        /// 지면에 닿아있을 경우의 속도 계산
        private void UpdateVelocity_Ground(ref Vector3 currentVelocity, float deltaTime)
        {
            Vector3 up = motor.CharacterUp;

            // 현재 닿아있는 경사면에 대해 속도 재계산
            var groundNormal = motor.GroundingStatus.GroundNormal;
            // 비정상 노멀(매우 가파른 곳)일 때 업벡터로 대체
            if (groundNormal.sqrMagnitude < 0.5f)
                groundNormal = up;

            // 현재 속도를 '지면 접선'에 투영
            float curSpeedMag = currentVelocity.magnitude;
            Vector3 tangentCurrent = motor.GetDirectionTangentToSurface(
                curSpeedMag > 0f ? currentVelocity : Vector3.zero, groundNormal
            );
            tangentCurrent = tangentCurrent.normalized * curSpeedMag;

            // 입력을 '지면 접선'으로 재지향
            Vector3 moveDir = (LockMoveInput ? Vector3.zero : characterMoveDir);
            Vector3 desiredDir = Vector3.zero;
            if (moveDir.sqrMagnitude > Mathf.Epsilon)
            {
                desiredDir = motor.GetDirectionTangentToSurface(moveDir.normalized, groundNormal);
                desiredDir = desiredDir.normalized * moveDir.magnitude;
            }

            Vector3 targetMovementVelocity = desiredDir * maxMoveSpeed;

            // 접지 상태에서 '상향(법선 방향) 성분' 제거 → 바닥에서 튀는 원인 차단
            // (접지 중엔 바닥에서 떨어져 나가려는 성분은 0으로)
            float nDotV = Vector3.Dot(tangentCurrent, groundNormal);
            if (nDotV > 0f)
                tangentCurrent -= groundNormal * nDotV;

            // 속도 보간
            float lerpValue = 1f - Mathf.Exp(-movementSharpness * deltaTime);
            Vector3 blended = Vector3.Lerp(tangentCurrent, targetMovementVelocity, lerpValue);

            // 다운힐 과가속(부스트) 방지: 접선 속도를 상한으로 클램프 (옵션)
            float limit = maxSlopeSpeed > 0f ? maxSlopeSpeed : maxMoveSpeed;
            if (blended.sqrMagnitude > limit * limit)
                blended = blended.normalized * limit;

            // 지면 중력(스틱 투 그라운드용): 살짝 아래로 당겨 지면 상실 줄이기
            //blended += -up * (GravityMagnitude * deltaTime);

            currentVelocity = blended;
        }

        private void UpdateVelocity_AirMove(ref Vector3 currentVelocity, float deltaTime)
        {
            if (characterMoveDir == Vector3.zero)
                return;

            Vector3 flatVel = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
            Vector3 movedir = Vector3.ProjectOnPlane(characterMoveDir, Vector3.up);
            float magnitude = flatVel.magnitude;
            // 무조건 캐릭터가 보는 방향으로 맞춰줌
            currentVelocity = movedir * magnitude + Vector3.up * currentVelocity.y;
        }

        private void UpdateVelocity_Air(ref Vector3 currentVelocity, Vector3 baseVelocity, float deltaTime)
        {
            // 현재 속도의 평면 벡터
            var curVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Up);
            // 현재 속도의 수직 벡터
            var curVelocityVertical = currentVelocity - curVelocityOnInputsPlane;
            
            // 평면 벡터 크기의 속도 제한
            if (curVelocityOnInputsPlane.magnitude > curMaxAirMoveSpeed)
            {
                curVelocityOnInputsPlane = curVelocityOnInputsPlane.normalized * curMaxAirMoveSpeed;
            }
            // 재조립
            currentVelocity = curVelocityOnInputsPlane + curVelocityVertical;
            
            // 이동값이 있을 경우,
            if (characterMoveDir.sqrMagnitude > 0f)
            {
                // 공중 가속도를 보정한 추가 속도값
                var addedVelocity = characterMoveDir * airAccelerationSpeed * deltaTime;

                if (curVelocityOnInputsPlane.magnitude < curMaxAirMoveSpeed)
                {
                    // 현재 속도가 공중 최대 속도보다 낮을 경우 최대 속도까지 가속 허용
                    var total = Vector3.ClampMagnitude(curVelocityOnInputsPlane + addedVelocity, curMaxAirMoveSpeed);
                    addedVelocity = total - curVelocityOnInputsPlane;
                }
                else
                {
                    if (Vector3.Dot(curVelocityOnInputsPlane, addedVelocity) > 0f)
                    {
                        // 현재 속도가 공중 최대 속도보다 큰 경우, addedVelocity에서 현재 속도와 같은 방향 성분을 제거
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, curVelocityOnInputsPlane.normalized);
                    }
                }

                // 경사면 타고 올라가는 상황 방지
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
                    {
                        var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Up, motor.GroundingStatus.GroundNormal), Up).normalized;
                        addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                    }
                }

                // Apply added velocity
                currentVelocity += addedVelocity;
            }

            // Gravity
            var gravityStrength = gravityMagnitude;
            if (onJump)
            {
                // 점프 중일 경우 상승/하강 중에 따라 다른 중력 적용
                gravityStrength = baseVelocity.y > 0 ? jumpUpGravityMagnitude : jumpDownGravityMagnitude;
            }
            currentVelocity += gravityDir * gravityStrength * deltaTime;

            // 공중 감속
            currentVelocity *= (1f / (1f + (airMoveDrag * deltaTime)));
        }

        /// 점프시의 속도 증가 처리
        private void UpdateVelocity_Jump(ref Vector3 currentVelocity, float deltaTime)
        {
            if (jumpPrevFrame)
                jumpPrevFrame = false;
            
            if (jumpRequested)
            {
                jumpRequested = false;
                jumpPrevFrame = true;

                if (Vector3.Dot(Forward, currentVelocity) < 0)
                    currentVelocity = Vector3.zero;

                // 점프 전까지 루트모션 속력 평균 값을 사용하여 속도를 새로 설정
                // >>> 기본 이동 제어에 루트모션 값만을 사용한다는 전제하에 작성한 코드이므로 주의해주세요
                var curVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Up);
                var curVelocityVertical = currentVelocity - curVelocityOnInputsPlane;
                curVelocityOnInputsPlane = curVelocityOnInputsPlane.normalized * curMaxAirMoveSpeed;
                currentVelocity = curVelocityOnInputsPlane + curVelocityVertical;
                
                // 점프에 따른 상방향 추가 벡터
                currentVelocity += (Up * jumpUpVelocity) - Vector3.Project(currentVelocity, Up);
                // 점프 시에 존재하는 이동 입력값에 대한 추가 속도 
                currentVelocity += characterMoveDir * jumpScalableForwardSpeed;
                
                motor.ForceUnground();
                
                EventBus.Post<ICharacterBaseEventListener>(_=>_.OnJumpStarted());

                onJump = true;
            }
        }
        
        private void UpdateVelocity_RootMotion(ref Vector3 currentVelocity, float deltaTime)
        {
            // 기존 속도를 수직/수평방향 분리
            var vCurVelocity = Vector3.Project(currentVelocity, Up);
            var hCurVelocity = currentVelocity - vCurVelocity;
            
            // 루트모션의 이동 변화값을 속도로 변환
            var rootMotionVelocity = animatorDeltaPosition / deltaTime;
            
            // 루트모션 속도를 수직/수평방향 분리
            var vRootMotionVelocity = Vector3.Project(rootMotionVelocity, Up);
            var hRootMotionVelocity = rootMotionVelocity - vRootMotionVelocity;
            
            // 수평 방향 루트모션 처리
            switch (modeRootMotionHorizotalPos)
            {
                case CharacterRootMotionMode.Additive:
                    hCurVelocity += hRootMotionVelocity;
                    break;
                case CharacterRootMotionMode.Override:
                    hCurVelocity = hRootMotionVelocity;
                    break;
            }
            // 수직 방향 루트모션 처리
            switch (modeRootMotionVerticalPos)
            {
                case CharacterRootMotionMode.Additive:
                    vCurVelocity += vRootMotionVelocity;
                    break;
                case CharacterRootMotionMode.Override:
                    vCurVelocity = vRootMotionVelocity;
                    break;
            }

            // 재조립
            currentVelocity = hCurVelocity + vCurVelocity;

            // 루트모션만으로 이동이 제어되고 있을 경우, 수평방향 속도를 저장(점프에 사용하기 위해)
            if (UpdateRootmotionVelocity && modeRootMotionHorizotalPos == CharacterRootMotionMode.Override)
            {
                rootmotionVelocityQueue.Enqueue(hCurVelocity.magnitude);
            }
        }
        
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // if (IsMountedToGadget)
            // {
            //     // 부모 오브젝트(transform.parent)가 가젯이므로
            //     var gadgetT = transform.parent;
            //     motor.SetPositionAndRotation(gadgetT.position, gadgetT.rotation);
            // }
            
            animatorDeltaPosition = Vector3.zero;
            animatorDeltaRotation = Quaternion.identity;
            
            if (jumpRequested)
            {
                jumpRequested = false;
            }

            if (dashRequested)
            {
                dashRequested = false;
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            // 점프 중 충돌 체크
            if (onJump)
            {
                // 점프 상승 중
                if (currentMoveVelocity.y > 0f && ((1 << hitCollider.gameObject.layer) & motor.CollidableLayers) != 0)
                {
                    // 상단의 오브젝트와 충돌 체크
                    if (Vector3.Dot(hitNormal, motor.CharacterUp) < jumpCollisionHitNormalDotProduct)
                    {
                        motor.SetGroundSolvingActivation(true);
                        EventBus.Post<ICharacterBaseEventListener>(_ => _.OnJumpCollisionDetected());
                    }
                }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}