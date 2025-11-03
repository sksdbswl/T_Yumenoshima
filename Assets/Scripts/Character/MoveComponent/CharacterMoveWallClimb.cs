using System;
using System.Collections.Generic;
using UnityEngine;

namespace REIW
{
    using REIW.EventLock;
    
    public class CharacterMoveWallClimb : CharacterMoveComponentBase<CharacterMoveWallClimbData>, ICharacterMoveComponentGizmo
    {
        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // GizmoUtility.DrawLabeledArrowHandle(this.CharacterTransform.position, targetGravity, 1f, 
            //     Color.red, label: $"Target Gravity:{targetGravity}");
            // GizmoUtility.DrawLabeledArrowHandle(this.CharacterTransform.position, CurrentGravityDirection, 1f, 
            //     Color.blue, label: $"Current Gravity:{CurrentGravityDirection}");
            //
            // if (MovementData == null)
            //     return;
            //
            // Vector3 pos = this.CharacterTransform.position;
            // Vector3 up = this.CharacterTransform.up;
            // Vector3 forward = this.CharacterTransform.forward;
            // float range = MovementData.DetectionRange;
            //
            // // Front 캐스트
            // Vector3 frontOrigin = pos + up;
            // Vector3 frontEnd = frontOrigin + forward * range;
            // Gizmos.color = isDetectedFrontWall ? Color.green : new Color(0f, 1f, 0f, 0.3f);
            // Gizmos.DrawLine(frontOrigin, frontEnd);
            // Gizmos.DrawSphere(frontEnd, 0.05f);
            // UnityEditor.Handles.Label(frontEnd, "FrontCast");
            //
            // // FrontDown 캐스트
            // Vector3 frontDownOrigin = pos - up + forward;
            // Vector3 frontDownEnd = frontDownOrigin - forward * range;
            // Gizmos.color = isDetectedFrontDownWall ? Color.red : new Color(1f, 1f, 0f, 0.3f);
            // Gizmos.DrawLine(frontDownOrigin, frontDownEnd);
            // Gizmos.DrawSphere(frontDownEnd, 0.05f);
            // UnityEditor.Handles.Label(frontDownEnd, "FrontDownCast");
            //
            // // Down 캐스트 (참고용)
            // Vector3 downOrigin = pos;
            // Vector3 downEnd = downOrigin - up * range;
            // Gizmos.color = Color.cyan;
            // Gizmos.DrawLine(downOrigin, downEnd);
            // Gizmos.DrawSphere(downEnd, 0.05f);
            // UnityEditor.Handles.Label(downEnd, "DownCast");
#endif
        }
        
        public override CharacterMoveType MoveType => CharacterMoveType.WallClimb;
        public enum ClimbState { None, OnFloor, Snapping, Falling } // Climb State

        public bool IsActivateWallClimb
        {
            get => isActivatedWallClimb;
            set
            {
                if (!isActivatedWallClimb && value) // WallClimb 비활성화 상태였었는데 => 활성화로 변경되는 경우?
                    Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnWallClimbStarted());
                
                isActivatedWallClimb = value;
                if (!isActivatedWallClimb)
                {
                    ResetState();
                    SetGravity(Vector3.down);
                    Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnWallClimbFinished());
                }
            }
        }
        public bool IsNotDownGravity => targetGravity != Vector3.down;
        public bool IsGrounded => curState == ClimbState.None || curState == ClimbState.OnFloor;
        public bool IsPossibleGrapple => targetGravity == Vector3.down;
        public bool IsDetectedWall => isDetectedFrontWall || isDetectedFrontDownWall;
        public bool IsPossibleWallClimb => isPossibleWallClimb;
        public Vector3 CurrentGravityDirection => curGravity.normalized;
        public Vector3 TargetGravityDirection => targetGravity.normalized;
        public ClimbState CurrentState => curState;

        
        private bool isActivatedWallClimb;
        private bool isDetectedFrontWall;
        private bool isDetectedFrontDownWall;
        private bool isPossibleWallClimb;
        private ClimbState curState = ClimbState.None;
        private ClimbState prevState = ClimbState.None;
        private Vector3 curGravity;
        private Vector3 targetGravity;
        
        private Vector3 _targetWallNormal;
        private Vector3 _targetWallPoint;
        private float _fallingTime;
        private Vector2 _inputMove;

        private LayerMask _groundLayer;
        
        public override eEventLockType CurrentEventLockType => IsGrounded == false ? eEventLockType.CharacterGlide : base.CurrentEventLockType;

        public override void EnterComponent()
        {
            base.EnterComponent();
            
            ChangeState(ClimbState.None);
            if (TryCastClimbWallToDown(out var hitDown))
            {
                SetGravity(-hitDown.normal);
            }
        }

        public override void EnterFromPreviousComponentType(CharacterMovePlayMode prevmode)
        {
            if (prevmode != CharacterMovePlayMode.Gliding)
                return;
            
            SetGravity(Vector3.down);
        }

        public override void ExitComponent()
        {
            base.ExitComponent();
            
            ChangeState(ClimbState.None);
            SetGravity(Vector3.down);
            
            //PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
        }

        public override void Initialize(ICharacterMoveController controller)
        {
            base.Initialize(controller);
            
            //_groundLayer = LayerMask.GetMask(LayerMask.LayerToName(Layer.LAYER_GROUND));
        }

        public override void FixedUpdateComponent()
        {
            if (!IsActivateWallClimb)
            {
                bool isFoundFrontWall = TryCastClimbWallToFront(out var hitFront); // 정면에 있는 벽 검사
                bool isFoundFrontDownWall = TryCastClimbWallToFrontDown(out var hitFrontDown); // 캐릭터 정면 + 아래에서 역으로 Casting (:Foot 아래 절벽 면이 있는지)
                bool isEnoughSpace = TryCastClimbSpace(hitFront.point, hitFront.normal, out var hitSpace);
                isPossibleWallClimb = (isFoundFrontWall || isFoundFrontDownWall) && isEnoughSpace;
                return;
            }
            
            // PlayerController.Instance.CurrentExecuteActionTypeStateType = 
            //     IsNotDownGravity ? eStaminaActionType.WallClimb : eStaminaActionType.Normal;
            
            switch (curState)
            {
                case ClimbState.None:
                case ClimbState.OnFloor:
                {
                    bool isFoundFrontWall = TryCastClimbWallToFront(out var hitFront); // 정면에 있는 벽 검사
                    if (isFoundFrontWall)
                    {
                        bool isEnoughSpace = TryCastClimbSpace(hitFront.point, hitFront.normal, out var hitSpace);
                        isPossibleWallClimb = isEnoughSpace;
                        if (isEnoughSpace)
                        {
                            if (_inputMove.sqrMagnitude > 0f)
                            {
                                _targetWallPoint = hitFront.point;
                                _targetWallNormal = hitFront.normal;
                                
                                targetGravity = -hitFront.normal.normalized;
                                ChangeState(ClimbState.Snapping);
                                
                                Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnGravityChangeStarted(false));
                            }
                            break;
                        }
                    }
                    
                    bool isFoundDownWall = TryCastClimbWallToDown(out var hitDown);
                    if (!isFoundFrontWall && isFoundDownWall)
                    {
                        ChangeState(ClimbState.OnFloor);
                    }
                    else
                    {
                        ChangeState(ClimbState.Falling);
                    }
                }
                break;
                case ClimbState.Snapping:
                {
                    // Snapping 중에는 중력 외력으로 흔들리지 않게 제어
                    // Controller.SetGravity(Vector3.zero);

                    Vector3 targetUp = _targetWallNormal.normalized;
                    float upDot  = Vector3.Dot(this.CharacterTransform.up, targetUp);
                    float fwdAbs = Mathf.Abs(Vector3.Dot(this.CharacterTransform.forward, targetUp));

                    bool upAligned     = upDot  >= MovementData.SnapCompleteUpDot;
                    bool fwdPerpEnough = fwdAbs <= MovementData.SnapCompleteFwdPerpMax;

                    if (upAligned && fwdPerpEnough)
                    {
                        ChangeState(ClimbState.OnFloor);
                        Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnGravityChangeFinished(!this.IsNotDownGravity));
                        
                        CurrentLocalCharacter.ResetInputs();
                        
                        break;
                    }

                    // 스냅 진행 중이면 targetGravity를 계속 벽 방향으로 수렴
                    Vector3 desired = -targetUp;
                    targetGravity = Vector3.Slerp(targetGravity, desired, MovementData.ChangeGravitySharpness * Time.fixedDeltaTime).normalized;
                }
                break;
                case ClimbState.Falling:
                {
                    _fallingTime += Time.fixedDeltaTime;
                    if (_fallingTime > MovementData.FallingLimitTime)
                    {
                        this.IsActivateWallClimb = false;
                        break;
                    }

                    if (_fallingTime > MovementData.FallingDetectionTryDelay) // Falling 시작 후, 0.1초 동안은 아무일도 하지 않음
                                             // Ledge 면을 어느정도 통과하도록 일부러 0.1초 동안 낙하 시키고나서 Snapping 처리를 위함
                    {
                        bool isFoundDownWall = TryCastClimbWallToDown(out var hitDown); // 캐릭터 Down 방향으로 검사
                        if (isFoundDownWall) // 현재 지면 위에 서있는 상태
                        {
                            _targetWallPoint = hitDown.point;
                            _targetWallNormal = hitDown.normal;
                                
                            ChangeState(ClimbState.OnFloor);
                            targetGravity = -hitDown.normal.normalized;
                        }
                        else // 지면위에 서 있지 않은 상태 => Falling 상태
                        {
                            bool isFoundBehindWall = TryCastClimbWallToBehind(out var hitBehind); // 캐릭터 후면 검사
                            if (isFoundBehindWall)
                            {
                                _targetWallPoint = hitBehind.point;
                                _targetWallNormal = hitBehind.normal;
                            
                                targetGravity = -hitBehind.normal.normalized;
                                ChangeState(ClimbState.Snapping);
                            
                                Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnGravityChangeStarted(true));
                            }
                            else
                            {
                                ChangeState(ClimbState.Falling);
                            }
                        }
                        
                        Controller.SetGravity(curGravity);
                    }
                }
                break;
            }
            
            // Gravity 보간
            InterpolateGravityDirection(Time.fixedDeltaTime);
        }
        
        public override void LateUpdateComponent()
        {
            base.LateUpdateComponent();
        }

        public override void UpdateInput(PlayerCharacterInputs inputs)
        {
            _inputMove = inputs.Move;
            if (inputs.Jump)
            {
                IsActivateWallClimb = false;
            }
        }

        public override bool UpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            if (curState == ClimbState.Snapping)
            {
                velocity = (targetGravity * MovementData.SnappingGravityStrength * deltaTime);
            }
            
            return false;
        }
        
        public override bool UpdateRotation(ref Quaternion rotation, float deltaTime)
        {
            var currentUp = rotation * Vector3.up;
            var dot = Vector3.Dot(currentUp, -targetGravity.normalized);
            if (dot > 0.99999f)
            {
                return false;
            }

            var t = Mathf.Exp(MovementData.ChangeGravitySharpness * deltaTime);
            var smoothedGravityDir = Vector3.Slerp(currentUp, -curGravity, t);
            smoothedGravityDir.Normalize();
            rotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * rotation;
            
            return true;
        }

        void ResetState()
        {
            prevState = ClimbState.None;
            curState = ClimbState.None;
            Controller.EventBus.Post<IMoveWallClimbEventListener>(_ => _.OnWallClimbFinished());
        }
        
        void ChangeState(ClimbState newState)
        {
            prevState = curState;
            switch (newState)
            {
                case ClimbState.None:
                {
                    SetGravity(Vector3.down);
                    this.isActivatedWallClimb = false;
                    if (Controller is LocalCharacter localCharacter)
                        localCharacter.LockMoveInput = false;
                }
                break;
                case ClimbState.OnFloor:
                {
                    if (Controller is LocalCharacter localCharacter)
                        localCharacter.LockMoveInput = false;
                }
                break;
                case ClimbState.Snapping:
                {
                    _fallingTime = 0f;
                    if (Controller is LocalCharacter localCharacter)
                        localCharacter.LockMoveInput = true;
                }
                break;
                case ClimbState.Falling:
                {
                    if (Controller is LocalCharacter localCharacter)
                        localCharacter.LockMoveInput = true;
                }
                break;
            }
            curState = newState;
        }
        
        bool TryCastClimbWallToDown(out RaycastHit hit) => TryCastClimbWall(
            // 위치 : 캐릭터 position
            // 방향 : down
            this.CharacterTransform.position, -this.CharacterTransform.up, MovementData.DetectionRange, MovementData.StableLayer, out hit);
        bool TryCastClimbWallToFront(out RaycastHit hit)
        {
            // 위치 : 캐릭터 기준 - 캐릭터 Up
            // 방향 : forward
            isDetectedFrontWall = TryCastClimbWall(this.CharacterTransform.position + this.CharacterTransform.up, this.CharacterTransform.forward, MovementData.DetectionRange,
                MovementData.StableLayer, out hit);
            return isDetectedFrontWall;
        }
        bool TryCastClimbWallToFrontDown(out RaycastHit hit)
        {
            // 위치 : 캐릭터 기준 - 캐릭터 Down + 캐릭터 Forward
            // 방향 : -forward
            isDetectedFrontDownWall = TryCastClimbWall(
                this.CharacterTransform.position - this.CharacterTransform.up + this.CharacterTransform.forward,
                -this.CharacterTransform.forward, MovementData.DetectionRange,
                MovementData.StableLayer, out hit);
            return isDetectedFrontDownWall;
        }
        bool TryCastClimbWallToBehind(out RaycastHit hit) => TryCastClimbWall( /* Behind Cast 에서는 Detection Range를 1.2배수로 통제 */
            // 위치 : 캐릭터 position
            // 방향 : -forward
            this.CharacterTransform.position, -this.CharacterTransform.forward, MovementData.DetectionRange * 1.2f, MovementData.StableLayer, out hit);
        bool TryCastClimbWall(Vector3 origin, Vector3 direction, float range, LayerMask mask, out RaycastHit hitInfo)
        {
            Ray ray = new Ray(origin, direction);
            return Physics.Raycast(ray, out hitInfo, range, mask, QueryTriggerInteraction.Ignore);
        }
        bool TryCastClimbSpace(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            Ray ray = new Ray(origin, direction);
            return false == Physics.SphereCast(ray, 1f, out hit, MovementData.SpaceDetectionDistance, MovementData.ObstacleLayer, QueryTriggerInteraction.Ignore);
        }
        
        void SetGravity(Vector3 gravity)
        {
            targetGravity = gravity;
            curGravity = gravity;
            
            Controller.SetGravity(curGravity);
        }
        
        void InterpolateGravityDirection(float deltaTime)
        {
            if (curGravity != targetGravity)
            {
                var v = 1f - Mathf.Exp(-MovementData.ChangeGravitySharpness * deltaTime);
                curGravity = Vector3.Lerp(curGravity, targetGravity, v).normalized;
                Controller.SetGravity(curGravity);
            }
        }
    }
}