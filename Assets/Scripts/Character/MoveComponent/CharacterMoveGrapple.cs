using UnityEngine;
using System.Collections.Generic;

namespace REIW
{
    using REIW.EventLock;
    
    /// <summary>
    /// https://www.notion.so/voyagergames/211229b506f28002989efc1e6603e2a6
    /// </summary>
    public class CharacterMoveGrapple : CharacterMoveComponentBase<CharacterMoveGrappleData>
    {
        public bool IsPossibleGrapple => curTargetedPoint != null;
        
        public enum GrappleState
        {
            None,
            Detecting,      // 포인트 찾는 중(=Idle)
            Throw,          // 그래플 던짐
            Grappling,      // 그래플 이동 중
            Landing,        // 그래플 도착 후 런치 대기(입력 없으면 종료)
            LaunchWaiting,  // 런치 요청 후 대기
            Launching,      // 그래플 도착 후 크게 점프
        }
        
        public override CharacterMoveType MoveType => CharacterMoveType.Grapple;
        
        // 캐릭터 이동 목표 포지션
        public Vector3 destinationPoint
        {
            get
            {
                var grapplePoint = onGrapplePoint ?? curTargetedPoint;
                return grapplePoint ? grapplePoint.ArrivePosition : Controller.CharacterTransform.position;
            }
        }

        // 포인트 검출에 스크린 기반 검사 과정이 있어서 거기에 사용..
        private Camera mainCamera;

        private float detectionLeftPixel;
        private float detectionRightPixel;
        private float detectionTopPixel;
        private float detectionBottomPixel;
        private float launchLandingCheckVelocity;

        private GrappleState currentState = GrappleState.Detecting;
        private float stateElapsed = 0;

        private GrapplePoint onGrapplePoint;
        private Vector3 grapplePosition;
        private Vector3 grappleDirection;
        private Vector3 characterLookDir;
        private Vector3 launchDir;
        private Vector3 currentVelocity;
        private float startSpeed;
        private float currentSpeed;
        private float grappleDistance;
        private float grappleMoveTime;
        private float launchingHorizontalSpeed;
        private float launchingVerticalSpeed;
        private float launchingVerticalAngle;
        private bool isFarDistance;
        private bool isStartLaunching;

        private Vector3 moveInput;
        private bool jumpPressed;

        private GrapplePoint prevTargetedPoint;
        private GrapplePoint curTargetedPoint;

        private float _accelDeceleration = 1.0f;

        public override void Initialize(ICharacterMoveController controller)
        {
            base.Initialize(controller);

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not found!");
            }

            CacheDetectingScreenRegion();
        } 

        public override void FixedUpdateComponent()
        {
            stateElapsed += Time.deltaTime;

            switch (currentState)
            {
                case GrappleState.Detecting:
                    DetectGrapplePoint();
                    break;
                case GrappleState.Launching:
                    if (currentVelocity.y < 0f)
                    {
                        // 런칭 후 하락시 포인트 검사
                        DetectGrapplePoint();
                    }
                    break;
                case GrappleState.Landing:
                    FixedUpdateLanding();
                    break;
            }
        }

        private void FixedUpdateLanding()
        {
            if (stateElapsed > MovementData.launchInputWaiting)
            {
                ChangeState(GrappleState.Detecting);
                if (Controller is LocalCharacter localCharacter)
                    localCharacter.ColliderTransformLinker?.RestoreParent();
                return;
            }

            // 그래플 포인트 도착 후에도 지정 시간 안에 점프 입력을 했을 경우 런칭 시작
            if (jumpPressed)
            {
                jumpPressed = false;
                ChangeState(GrappleState.LaunchWaiting);

                Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleLaunchRequested((isSuccess) =>
                {
                    if (isSuccess)
                    {
                        ChangeState(GrappleState.Launching);
                        isStartLaunching = true;
                        Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleLaunchStarted());
                    }
                    else
                    {
                        ChangeState(GrappleState.Detecting);
                        if (Controller is LocalCharacter localCharacter)
                            localCharacter.ColliderTransformLinker?.RestoreParent();
                    }
                }));
            }
        }
        
        public override void UpdateInput(PlayerCharacterInputs inputs)
        {
            if (inputs.Grapple && PlayerController.Instance.InputActionStaminaValidator.CanStaminaAction(eStaminaActionType.Grapple))
            {
                if (curTargetedPoint != null)
                {
                    if (currentState is GrappleState.Detecting or GrappleState.Launching)
                    {
                        RequestGrapple();
                    }   
                }
            }
            else if (inputs.Jump)
            {
                if (isFarDistance)
                {
                    jumpPressed = true;
                }
            }

            moveInput = new Vector3(inputs.Move.x, 0, inputs.Move.y);
        }
        
        public override bool UpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            bool fix = false;
            switch (currentState)
            {
                case GrappleState.Detecting:
                    startSpeed = velocity.magnitude;
                break;
                case GrappleState.Throw:
                    UpdateThrowVelocity(ref velocity, ref fix, deltaTime);
                break;
                case GrappleState.Grappling:
                    UpdateGrappleVelocity(ref velocity, ref fix, deltaTime);
                break;
                case GrappleState.Landing:
                    UpdateLandVelocity(ref velocity, ref fix, deltaTime);
                break;
                case GrappleState.LaunchWaiting:
                    UpdateLaunchWaitingVelocity(ref velocity, ref fix, deltaTime);
                break;
                case GrappleState.Launching:
                    UpdateLaunchVelocity(ref velocity, ref fix, deltaTime);
                break;
            }

            currentVelocity = velocity;
            return fix;
        }

        /// <summary>
        /// 그래플 던진 상태의 속도 제어
        /// </summary>
        private void UpdateThrowVelocity(ref Vector3 velocity, ref bool fix, float deltaTime)
        {
            // 이 상태에선 모든 이동을 무시
            velocity = Vector3.zero;
            fix = true;
        }

        /// <summary>
        /// 그래플 중의 속도 제어
        /// </summary>
        private void UpdateGrappleVelocity(ref Vector3 velocity, ref bool fix, float deltaTime)
        {
            grappleDirection = (destinationPoint - Controller.CharacterTransform.position).normalized;
            
            var minSpeed = velocity.magnitude;
            var maxSpeed = grappleDistance < MovementData.nearDistanceThreshold ? MovementData.nearMaxSpeed : MovementData.farMaxSpeed;
            var timeRatio = stateElapsed / MovementData.timeToMaxSpeed;
            var curveValue = MovementData.speedCurve.Evaluate(timeRatio);    // 지정한 커브에 따라 가속/감속 
            currentSpeed = maxSpeed * curveValue;
            currentSpeed = Mathf.Max(minSpeed, currentSpeed);
            
            // 그래플 중에는 다른 이동 요소를 무시하고 그래플 이동만 적용 
            velocity = grappleDirection * currentSpeed;
            fix = true;

            // 목표 지점과 현재 자신의 위치로 도착 판단
            var remainDistance = Vector3.Distance(Controller.CharacterTransform.position, destinationPoint);
            if (remainDistance <= MovementData.arrivalDistance || grappleMoveTime < timeRatio)
            {
                OnGrappleArrival();
            }
        }

        /// <summary>
        /// 포인트 도착 직후의 속도 제어
        /// </summary>
        private void UpdateLandVelocity(ref Vector3 velocity, ref bool fix, float deltaTime)
        {
            // 이 상태에선 모든 이동을 무시
            velocity.x = 0f;
            velocity.z = 0f;
            fix = true;
        }

        /// <summary>
        /// 런칭 요청 후 속도 제어
        /// </summary>
        private void UpdateLaunchWaitingVelocity(ref Vector3 velocity, ref bool fix, float deltaTime)
        {
            // 이 상태에선 모든 이동을 무시
            velocity.x = 0f;
            velocity.z = 0f;
            fix = true;
        }

//        private Vector3 _launchpos;
        
        private Vector3 InputDir
        {
            get
            {
                var input = new Vector3(moveInput.x, 0, moveInput.z);
                return mainCamera.transform.TransformDirection(input).normalized;
            }
        }

        /// <summary>
        /// 런칭 속도 제어
        /// </summary>
        private void UpdateLaunchVelocity(ref Vector3 velocity, ref bool fix, float deltaTime)
        {
            if (isStartLaunching)
            {
                var characterTransform = Controller.CharacterTransform;
//                _launchpos = characterTransform.position;
                
                // 런칭 수평 방향 계산
                var planarGrappleDir = new Vector3(grappleDirection.x, 0, grappleDirection.z);
                launchDir = planarGrappleDir;
                if (moveInput.magnitude > 0.1f)
                {
                    // 이동 입력이 있을 경우, 입력 방향으로 틀어준다
                    launchDir = Vector3.Normalize(launchDir + InputDir);

                    // 각도 상한은 존재
                    var angle = Vector3.Angle(planarGrappleDir, launchDir);
                    if (angle > MovementData.launchHorizontalAngleLimit)
                    {
                        var axis = Vector3.Cross(Controller.Forward, launchDir).normalized;
                        launchDir = Quaternion.AngleAxis(MovementData.launchHorizontalAngleLimit, axis) * Controller.Forward;
                    }

                    _accelDeceleration = GetAccelDecelation(planarGrappleDir, InputDir);
                }
                else
                    _accelDeceleration = 1.0f;

                // 런칭 수직 방향 계산
                var angleRad = Mathf.Deg2Rad * launchingVerticalAngle;
                launchDir = new Vector3(launchDir.x, Mathf.Sin(angleRad), launchDir.z);
                launchDir = launchDir.normalized;

                isStartLaunching = false;

                if (MovementData.showLaunchRays)
                {
                    var originalDir = Quaternion.AngleAxis(-launchingVerticalAngle, characterTransform.right) * characterTransform.forward;
                    var dest = characterTransform.position + originalDir * 3;
                    Debug.DrawLine(characterTransform.position, dest, Color.red);

                    dest = characterTransform.position + InputDir * 3;
                    Debug.DrawLine(characterTransform.position, dest, Color.green);

                    dest = characterTransform.position + InputDir * 3;
                    Debug.DrawLine(characterTransform.position, dest, Color.blue);
                }

                ResetSmoothRotation(launchDir);
            }

            var horizontalTimeRatio = stateElapsed / MovementData.launchHorizontalTimeToMaxSpeed;
            var verticalTimeRatio = stateElapsed / MovementData.launchVerticalTimeToMaxSpeed;
            float moveTimeRatio = stateElapsed / MovementData.launchMoveMaxSpeed;
            
            var minSpeed = velocity.magnitude;
            var horizontalCurveValue = MovementData.launchHorizontalSpeedCurve.Evaluate(horizontalTimeRatio);
            var horizontalSpeed = Mathf.Max(minSpeed, launchingHorizontalSpeed * horizontalCurveValue);

            Vector3 launchDirXZ = new Vector3(launchDir.x, 0f, launchDir.z).normalized;
            Vector3 horiz = launchDirXZ * (horizontalSpeed * _accelDeceleration);

            velocity.x = horiz.x;
            velocity.z = horiz.z;
            // 런칭 방향과 현재 방향에 따른 감속 
            DecelByCharacterMoveDir(ref velocity, InputDir, launchDirXZ);

            if (verticalTimeRatio <= 1f)
            {
                var verticalCurveValue = MovementData.launchVerticalSpeedCurve.Evaluate(verticalTimeRatio);
                velocity.y = launchDir.y * Mathf.Max(minSpeed, launchingVerticalSpeed * verticalCurveValue);

            }
            else
            {
                // 런칭 감속
                if (MovementData.launchDeceleration > 0f)
                {
                    var decelerated = velocity.magnitude + (MovementData.launchDeceleration * deltaTime);
                    var decelerationVelocity = velocity.normalized * decelerated;
                    velocity.x = decelerationVelocity.x;
                    velocity.z = decelerationVelocity.z;
                }
            }

            // 런칭 중 땅에 착지하면 Detecting 상태로 전환
            if (Controller.IsStableOnCollider && stateElapsed > MovementData.launchLandingCheckDelay)
            {
                ChangeState(GrappleState.Detecting);
                Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleLaunchLanding());
            }
            
            // 런칭 중에는 다른 속도 제어 요소는 무시
            fix = true;
        }

        void DecelByCharacterMoveDir(ref Vector3 currentvelocity, Vector3 movedir, Vector3 launchdir)
        {
            Vector3 vXZ = new Vector3(currentvelocity.x, 0f, currentvelocity.z);
            float speed = vXZ.magnitude;
            if (speed < Mathf.Epsilon)
                return;
            
            Vector3 velDir = vXZ / Mathf.Max(speed, Mathf.Epsilon);

            Vector3 moveDir = new Vector3(movedir.x, 0f, movedir.z);
            if (moveDir.sqrMagnitude <= Mathf.Epsilon) 
                moveDir = velDir;
            else 
                moveDir.Normalize();

            Vector3 ldir = new Vector3(launchdir.x, 0f, launchdir.z);
            if (ldir.sqrMagnitude <= Mathf.Epsilon)
                ldir = velDir; 
            else 
                ldir.Normalize();
            
            float align = Vector3.Dot(moveDir, ldir); // -1..1 (1: 같은 방향, -1: 반대)
            float back = Mathf.Clamp01(-align); // 0→1 (같은→반대)
            float side = 1f - Mathf.Abs(align); // 0→1 (같거나 반대→직각)

            float launchDecelAgainst = MovementData.launchDecelAgainst;
            float launchDecelPerp = launchDecelAgainst * 0.5f;
            float launchDecelWith = 0;
            
            float decelRate = (align >= 0f)
                ? Mathf.Lerp(launchDecelWith, launchDecelPerp, side) // 같은/비슷한 방향: 약~중 감속
                : Mathf.Lerp(launchDecelPerp, launchDecelAgainst, back); // 반대쪽일수록 더 큰 감속
            
            float newSpeed = speed - (speed * decelRate);
            Vector3 newVXZ = launchdir * newSpeed;
#if JIN_TEST
            Debugging.LogGreen($"movedir : {movedir} , launchdir : {launchdir}, align : {align}, decelRate : {decelRate}, speed : {speed}, newSpeed : {newSpeed}");
#endif
            currentvelocity.x = newVXZ.x;
            currentvelocity.z = newVXZ.z;
        }

        private float GetAccelDecelation(Vector3 launchDir, Vector3 currentForward)
        {
            Vector3 dH = Vector3.ProjectOnPlane(launchDir, Vector3.up).normalized;
            Vector3 fH = Vector3.ProjectOnPlane(currentForward, Vector3.up).normalized;

            float dot = Vector3.Dot(dH, fH);
            float thresholdDot = Mathf.Cos(120f * Mathf.Deg2Rad);
            float targetFactor;
            if (dot <= thresholdDot)
                targetFactor = 0f;
            else
                targetFactor = (dot - thresholdDot) / (1f - thresholdDot);

            return targetFactor;
        }
        
        private Vector3 _smoothedLookDir = Vector3.zero;
        
        public override bool UpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            Vector3? lookDirection =  null;
            switch (currentState)
            {
                case GrappleState.Grappling:
                    if (onGrapplePoint != null)
                    {
                        _smoothedLookDir = Vector3.zero;
                        // 그래플 중, 시선 방향은 포인트로 고정
                        lookDirection = (onGrapplePoint.Position - Controller.CharacterTransform.position).normalized;
                    }
                    break;

                case GrappleState.Launching:
                {
                    float moveTimeRatio = stateElapsed / MovementData.launchMoveMaxSpeed;
                    Vector3 launchDirXZ = new Vector3(launchDir.x, 0f, launchDir.z).normalized;
                    Vector3 movenomalized;
                    // 일정시간까지 런칭 방향 유지
                    if (moveTimeRatio <= 1f)
                    {
                        movenomalized = launchDirXZ;
                    }
                    else
                    {
                        float time = Mathf.Clamp01(MovementData.launchMoveRotaionPower * deltaTime);
                        movenomalized = Vector3.Slerp(_smoothedLookDir, InputDir, time);
                    }

                    lookDirection = movenomalized;
                    break;
                }
            }

//            DrawLine();

            if (TryRotate(lookDirection, deltaTime * MovementData.rotationSpeed, ref rotation))
            {
                return true;
            }

            return false;

/*            void DrawLine()
            {
                Vector3 lookdir = (Controller.CharacterTransform.position - _launchpos).normalized;
                Debug.DrawLine(_launchpos, _launchpos + (lookdir * 100), Color.yellow );
            }*/
        }

        private void ResetSmoothRotation(Vector3 launch)
        {
            _smoothedLookDir = launch.normalized;
        }

        bool TryRotate(Vector3? lookDirection, float deltaTime, ref Quaternion rotation)
        {
            if (!lookDirection.HasValue) return false;

            // 1) Y 제거 + 데드존
            Vector3 rawDir = Vector3.ProjectOnPlane(lookDirection.Value, Vector3.up);
            if (rawDir.sqrMagnitude < Mathf.Epsilon) return false;

            rawDir.Normalize();

            // 2) 180° 근처 뒤집힘 억제(선택)
            if (_smoothedLookDir.sqrMagnitude > Mathf.Epsilon &&
                Vector3.Angle(_smoothedLookDir, rawDir) > 175f)
            {
                rawDir = _smoothedLookDir;
            }

            // 3) 입력 방향 평활화(각속도 제한)
            float maxRadStep = Mathf.Deg2Rad * MovementData.launchDirTrackDegPerSec * deltaTime;
            _smoothedLookDir = (_smoothedLookDir.sqrMagnitude < Mathf.Epsilon)
                ? rawDir
                : Vector3.RotateTowards(_smoothedLookDir, rawDir, maxRadStep, 0f);

            // ==== Yaw만 사용 ====
            float targetYaw = Mathf.Atan2(_smoothedLookDir.x, _smoothedLookDir.z) * Mathf.Rad2Deg;
            Quaternion targetYawOnly = Quaternion.Euler(0f, targetYaw, 0f);

            // 4) 최종 회전도 각속도 제한 (Yaw만 따라감)
            float maxYawStep = MovementData.launchYawDegPerSec * deltaTime;
            Quaternion stepped = Quaternion.RotateTowards(Controller.CharacterTransform.rotation, targetYawOnly, maxYawStep);

            // 혹시 모를 Pitch/Roll 제거(보수적)
            float newYaw = stepped.eulerAngles.y;
            rotation = Quaternion.Euler(0f, newYaw, 0f);

            if (Controller is LocalCharacter local)
                local.CharacterLookDir = _smoothedLookDir;

#if JIN_TEST
            Debugging.LogOrange(
                $"cur:{Controller.CharacterTransform.rotation.eulerAngles}, " +
                $"targetYaw:{targetYaw:F2}, newYaw:{newYaw:F2}, dt:{deltaTime:F6}"
            );
#endif

            return true;
        }

        private bool CheckGrappleCondition()
        {
            if (Controller is not LocalCharacter local)
                return false;

            if (local.IsEventLockType(eEventLockType.CharacterGraple))
                return false;

            CharacterMoveWallClimb wallclimb = local.CharacterMoveComponentsHandler.GetMoveComponent<CharacterMoveWallClimb>();
            return wallclimb.IsPossibleGrapple;
        }

        private void DetectGrapplePoint()
        {
            if (CheckGrappleCondition() == false)
            {
                ReleaseGrapplePoint();
                return;
            }
                
            
            if(MovementData.refreshDetectionScreenRegion)
                CacheDetectingScreenRegion();
            
            var candidates = new List<Transform>();
            var characterTransform = Controller.CharacterTransform;

            // 구 형태로 1차적 포인트 검사
            var colliders = Physics.OverlapSphere(characterTransform.position, MovementData.detectionDistance, MovementData.grapplePointLayer);
            foreach (var col in colliders)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(col.transform.position);

                // Check object's screen position
                var inScreenRange = screenPos.z > 0 &&
                              screenPos.x >= detectionLeftPixel && screenPos.x <= detectionRightPixel &&
                              screenPos.y >= detectionBottomPixel && screenPos.y <= detectionTopPixel;
                if(!inScreenRange)
                    continue;
                
                // Check player vertical angle
                var dirToPoint = (col.transform.position - characterTransform.position).normalized;
                var angle = Vector3.Angle(Vector3.up, dirToPoint);
                if (angle < MovementData.detectionCharacterVerticalAngle)
                    continue;
                
                // Check distance
                var distance = Vector3.Distance(characterTransform.position, col.transform.position);
                if (distance > MovementData.maxGrappleDistance)
                    continue;
                if (distance < MovementData.minGrappleDistance)
                    continue;
                
                // Check obstacle
                distance -= Controller.Radius;
                var heightRate = 0f;
                var deltaHeight = characterTransform.position.y - col.transform.position.y;
                if (deltaHeight > 0f)
                    heightRate = Mathf.Lerp(0f, MovementData.detectionMaxHeightRate, deltaHeight / Controller.Height);
                var origin = characterTransform.position + Vector3.up * Controller.Height * heightRate;
                var direction = (col.transform.position - origin).normalized;
                if (Physics.Raycast(origin, direction, out var hit, distance, MovementData.grapplePointLayer | MovementData.obstacleLayer) && ((1 << hit.transform.gameObject.layer) & MovementData.obstacleLayer) != 0)
                    continue;
                origin = characterTransform.position + Vector3.up * Controller.Height;
                direction = (col.transform.position - origin).normalized;
                if (Physics.Raycast(origin, direction, out hit, distance, MovementData.grapplePointLayer | MovementData.obstacleLayer) && ((1 << hit.transform.gameObject.layer) & MovementData.obstacleLayer) != 0)
                    continue;

                candidates.Add(col.transform);
            }

            // select nearest point from screen's center
            Transform nearestPoint = null;
            var minScreenDistance = float.MaxValue;
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            foreach (var point in candidates)
            {
                var screenPos = mainCamera.WorldToScreenPoint(point.position);
                if (screenPos.z < 0)
                    continue;
            
                var screenPos2D = new Vector2(screenPos.x, screenPos.y);
                var screenDistance = (screenCenter - screenPos2D).sqrMagnitude;
                if (screenDistance < minScreenDistance)
                {
                    var grapplePoint = point.GetComponent<GrapplePoint>();
                    if (grapplePoint?.IsEnable(Controller.CharacterTransform) == true)
                    {
                        minScreenDistance = screenDistance;
                        nearestPoint = point;
                    }
                }
            }

            if (nearestPoint != null && (curTargetedPoint == null || nearestPoint != curTargetedPoint.transform))
            {
                prevTargetedPoint = curTargetedPoint;
                curTargetedPoint = nearestPoint.GetComponent<GrapplePoint>();

                var offset =  Vector3.up * Controller.Height * 0.5f;
                var targetPoint = curTargetedPoint.ArrivePosition;
                var origin = characterTransform.position + offset;
                var distance = Vector3.Distance(origin, targetPoint);
                var direction = (targetPoint - origin).normalized;
                grapplePosition = Physics.Raycast(origin, direction, out var hit, distance, MovementData.obstacleLayer) ? hit.point : targetPoint;
                
                Controller.EventBus.Post<IMoveGrappleEventListener>(_=>_.OnGrapplePointTargeted(prevTargetedPoint, curTargetedPoint, grapplePosition));
            }
            else if (curTargetedPoint != null && (nearestPoint == null || !curTargetedPoint.IsEnable(Controller.CharacterTransform)))
            {
                ReleaseGrapplePoint();
            }

            if (MovementData.showAngleRays)
            {
                if (curTargetedPoint != null)
                {
                    var dirToPoint = (curTargetedPoint.Position - mainCamera.transform.position).normalized;
                    var cAngle = Vector3.Angle(mainCamera.transform.forward, dirToPoint);
                    Debug.DrawLine(mainCamera.transform.position, curTargetedPoint.Position, Color.magenta);
                    var end = mainCamera.transform.position + mainCamera.transform.forward * 3;
                    Debug.DrawLine(mainCamera.transform.position, end, Color.magenta);

                    dirToPoint = (curTargetedPoint.Position - characterTransform.position).normalized;
                    var pAngle = Vector3.Angle(characterTransform.up, dirToPoint);
                    Debug.DrawLine(characterTransform.position, curTargetedPoint.Position, Color.cyan);
                    end = characterTransform.position + characterTransform.up * 3;
                    Debug.DrawLine(characterTransform.position, end, Color.cyan);
                }
            }
        }

        private void ReleaseGrapplePoint()
        {
            if (curTargetedPoint == null)
                return;
                    
            prevTargetedPoint = curTargetedPoint;
            curTargetedPoint = null;
            
            Controller.EventBus.Post<IMoveGrappleEventListener>(_=>_.OnGrapplePointTargeted(prevTargetedPoint, curTargetedPoint, Vector3.negativeInfinity));
        }

        private void ClearGrapplePoint()
        {
            if (curTargetedPoint == null)
                return;

            ReleaseGrapplePoint();
        }
        
        private void RequestGrapple()
        {
            if (curTargetedPoint == null)
                return;

            onGrapplePoint = curTargetedPoint;
            grappleDirection = (destinationPoint - Controller.CharacterTransform.position).normalized;
            grappleDistance = Vector3.Distance(Controller.CharacterTransform.position, destinationPoint);

            isFarDistance = grappleDistance > MovementData.nearDistanceThreshold;
            currentSpeed = startSpeed;
            jumpPressed = false;

            Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleRequested(onGrapplePoint, grapplePosition, grappleDistance, isFarDistance, StartGrapple));
        }

        public void StartThrow()
        {
            ChangeState(GrappleState.Throw);
        }
        
        private void StartGrapple(bool isSuccess)
        {
            if (isSuccess)
            {
                if (curTargetedPoint == null)
                    return;

                ChangeState(GrappleState.Grappling);

                grappleMoveTime = CalculateGrappleMoveTime(grappleDistance,
                    grappleDistance < MovementData.nearDistanceThreshold ? MovementData.nearMaxSpeed : MovementData.farMaxSpeed, MovementData.timeToMaxSpeed, MovementData.speedCurve);

                Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleStarted(onGrapplePoint, grappleMoveTime));
            }
            else
            {
                ChangeState(GrappleState.Detecting);
                onGrapplePoint = null;

                if (Controller is LocalCharacter localCharacter)
                    localCharacter.LockMoveInput = false;
            }
        }

        private void OnGrappleArrival()
        {
            var grapplePoint = onGrapplePoint ?? curTargetedPoint;
            if (grapplePoint != null)
            {
                if (grapplePoint.IsValidLaunchHorizontalSpeed)
                    launchingHorizontalSpeed = grapplePoint.LaunchHorizontalSpeed;
                else
                    launchingHorizontalSpeed = MovementData.launchHorizontalSpeed;

                if (grapplePoint.IsValidLaunchVerticalSpeed)
                    launchingVerticalSpeed = grapplePoint.LaunchVerticalSpeed;
                else
                    launchingVerticalSpeed = MovementData.launchVerticalSpeed;

                if (grapplePoint.IsValidLaunchVerticalAngle)
                    launchingVerticalAngle = grapplePoint.LaunchVerticalAngle;
                else
                    launchingVerticalAngle = MovementData.launchVerticalAngle;
            }

            ChangeState(GrappleState.Landing);
            ReleaseGrapplePoint();
            onGrapplePoint = null;
            
            Controller.EventBus.Post<IMoveGrappleEventListener>(_ => _.OnGrappleArrival());
        }

        private void ChangeState(GrappleState newState)
        {
            currentState = newState;
            stateElapsed = 0;
        }
        
        private void CacheDetectingScreenRegion()
        {
            var scale = Screen.height / 1080f;
            detectionLeftPixel = (Screen.width * 0.5f) - (MovementData.detectionHorizontalPx * scale);
            detectionRightPixel = (Screen.width * 0.5f) + (MovementData.detectionHorizontalPx * scale);
            detectionBottomPixel = (Screen.height * 0.5f) + (MovementData.detectionBotPx * scale);
            detectionTopPixel = Screen.height;
        }

        private float CalculateGrappleMoveTime(float distance, float moveSpeed, float timeToMaxSpeed, AnimationCurve speedCurve)
        {
            const int samples = 100;
            float totalTime = 0f;
            float traveled = 0f;

            float lastSpeed = moveSpeed * speedCurve.Evaluate(0f);
            float deltaTime = timeToMaxSpeed / samples;

            for (int i = 1; i <= samples; i++)
            {
                float t = (float)i / samples;
                float curveVal = speedCurve.Evaluate(t);
                float speed = moveSpeed * curveVal;

                // 평균 속도 * 시간 = 구간 거리
                float segmentDistance = ((lastSpeed + speed) * 0.5f) * deltaTime;

                traveled += segmentDistance;
                totalTime += deltaTime;

                if (traveled >= distance)
                    return totalTime;

                lastSpeed = speed;
            }

            // 만약 커브로 못 채웠다면, 남은 거리 일정 속도로 이동
            float remaining = distance - traveled;
            totalTime += remaining / moveSpeed;

            return totalTime;
        }
        
        public bool IsCurrentState(GrappleState state) => currentState == state;

        public override void ExitComponent()
        {
            base.ExitComponent();
            ClearGrapplePoint();
            currentState = GrappleState.Detecting;
        }

        public bool AvailableLandingMove(float normaltime)
        {
            return normaltime >= MovementData.landingNormalMoveEnableDelay;
        }

        public bool ShowGrapplePoint => MovementData.showGrapplePoint;
    }
}