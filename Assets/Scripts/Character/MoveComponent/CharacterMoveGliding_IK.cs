using REIW.Animations.Character;
using Unity.Mathematics;
using UnityEngine;

namespace REIW
{
    public partial class CharacterMoveGliding : CharacterMoveComponentBase<CharacterMoveGlidingData>
    {
        private CharacterAnimationMovement Movement => CurrentLocalCharacter.CharacterAnimation.Movement;
        private bool _airBorne = true;

        public class IK_Data
        {
            // ik
            public float _lowerBodyBankMul = 0.5f;
            public float _pelvisBankRatio = 0.7f;
            public float _bankSharpness = 14f;
            public float _footPosTrailWeightAir = 0.6f;
            public float _airPosWeight = 0.2f;
            public float _maxAirPosWeight = 0.1f;
            public float _footPitchDeg = 14f; 
            public float _footYawDeg = 6f;
            public float _speedRef = 6f; 
            public float _yawAccRef = 10f;
            public float _velLPSharpness = 14f; 
            public float _latDeadzone = 0.03f;
        }
        
        private IK_Data _ikData = new IK_Data();

        float ExtractBankDegFromRotation(Quaternion rotation, Vector3 forwardAxis)
        {
            Vector3 upNow = rotation * Vector3.up;
            return Vector3.SignedAngle(Vector3.up, upNow, forwardAxis);
        }

        private void OnEnterIK()
        {
            InitIK();
        }

        private void InitIK()
        {
            InitSaveIK();
            Movement.BodyIK.solver.OnPreUpdate   -= OnSolverPostUpdate;
            Movement.BodyIK.solver.OnPreUpdate   += OnSolverPostUpdate;
        }

        public void OnExitIK()
        {
            var movement = Movement;
            if (!movement)
                return;

            movement.BodyIK.solver.OnPreUpdate   -= OnSolverPostUpdate;
            ResetEffectorsAndRestoreWeights();
        }

        void OnSolverPostUpdate()
        {
            OnSolverPostUpdate_LowerBodyBank();
            OnSolverPostUpdate_GroundIK();            
        }
        
        void OnSolverPostUpdate_GroundIK()
        {
            _airBorne = UpdateGroundIK(MovementData.airborneGate);
        }
        
        private bool UpdateGroundIK(float height)
        {
            var grounder = Movement.GrounderIK;
            var legs = grounder?.solver?.legs;
            if (legs == null || legs.Length < 2)
                return false;

            bool air_l = legs[0].heightFromGround > height || (!legs[0].isGrounded);
            bool air_r = legs[1].heightFromGround > height || (!legs[1].isGrounded);
            // bool ground_l = legs[0].heightFromGround <= height || legs[0].isGrounded;
            // bool ground_r = legs[1].heightFromGround <= height || legs[1].isGrounded;

            return air_l && air_r;
        }
        
        /////////////////////////////////////////
        /// /////////////////////////////////////////
        /// /////////////////////////////////////////
        
        struct EffWeights
        {
            public float bodyPos, bodyRot, lPos, lRot, rPos, rRot;
        }

        EffWeights _savedW;
        bool _savedWValid;
        float _savedGrounderWeight;

        float _bankDegSmoothed;
        Quaternion _lastFinalRotation = Quaternion.identity;

        Vector3 _vLP;
        Vector3 _prevFwdPlanar;

        private void InitSaveIK()
        {
            var s = Movement.BodyIK.solver;
            if (!_savedWValid)
            {
                _savedW.bodyPos = s.bodyEffector.positionWeight;
                _savedW.bodyRot = s.bodyEffector.rotationWeight;
                _savedW.lPos = s.leftFootEffector.positionWeight;
                _savedW.lRot = s.leftFootEffector.rotationWeight;
                _savedW.rPos = s.rightFootEffector.positionWeight;
                _savedW.rRot = s.rightFootEffector.rotationWeight;
                _savedWValid = true;
            }

            _savedGrounderWeight = Movement.GrounderIK.weight;
            // (2) 최소 가중치 보장: 발은 "회전"만 기본값(위치는 프레임마다 동적으로)
            s.leftFootEffector.rotationWeight = Mathf.Max(0.55f, s.leftFootEffector.rotationWeight);
            s.rightFootEffector.rotationWeight = Mathf.Max(0.55f, s.rightFootEffector.rotationWeight);
            s.leftFootEffector.positionWeight = _ikData._airPosWeight;
            s.rightFootEffector.positionWeight = _ikData._airPosWeight;
            
            // 골반은 약간 위치/회전 허용 (하체 전체가 따라오도록)
            s.bodyEffector.positionWeight = 0; //Mathf.Max(0.20f, s.bodyEffector.positionWeight);
            s.bodyEffector.rotationWeight = 0; //Mathf.Max(0.35f, s.bodyEffector.rotationWeight);
            // 초기 상태
            _lastFinalRotation = CurrentLocalCharacter.MyTransform.rotation;
            _bankDegSmoothed = 0f;
            // 속도/요율 스무딩 초기화
            _vLP = Vector3.zero;
            _prevFwdPlanar = Vector3.ProjectOnPlane(CurrentLocalCharacter.MyTransform.forward,
                CurrentLocalCharacter.MyTransform.up).normalized;
        }

        void ResetEffectorsAndRestoreWeights()
        {
            var s = Movement.BodyIK.solver;

            // 절대 타깃을 뼈 값으로 리셋 (회전은 원복)
            if (s.bodyEffector.bone)
                s.bodyEffector.rotation = s.bodyEffector.bone.rotation;
            if (s.leftFootEffector.bone)
                s.leftFootEffector.rotation = s.leftFootEffector.bone.rotation;
            if (s.rightFootEffector.bone)
                s.rightFootEffector.rotation = s.rightFootEffector.bone.rotation;

            // ★ positionOffset은 0으로(누적 제거)
            s.bodyEffector.positionOffset   = Vector3.zero;
            s.leftFootEffector.positionOffset  = Vector3.zero;
            s.rightFootEffector.positionOffset = Vector3.zero;

            // (positionWeight/rotationWeight는 저장값으로 복구)
            if (_savedWValid)
            {
                s.bodyEffector.positionWeight  = _savedW.bodyPos;
                s.bodyEffector.rotationWeight  = _savedW.bodyRot;
                s.leftFootEffector.positionWeight  = _savedW.lPos;
                s.leftFootEffector.rotationWeight  = _savedW.lRot;
                s.rightFootEffector.positionWeight = _savedW.rPos;
                s.rightFootEffector.rotationWeight = _savedW.rRot;
                _savedWValid = false;
            }

            Movement.GrounderIK.weight = _savedGrounderWeight;
        }

        void OnSolverPostUpdate_LowerBodyBank()
        {
            var s = Movement.BodyIK.solver;
            var basis = CurrentLocalCharacter.MyTransform;
            Vector3 up = basis.up, right = basis.right, fwd = basis.forward;
            float air = MovementData.IK_POWER;
            
            // --- 회전 입력 → Z-롤 추출 + 스무딩 ---
            Quaternion srcRot = _lastFinalRotation;
            float bankDegTarget = ExtractBankDegFromRotation(srcRot, fwd) * _ikData._lowerBodyBankMul;
            float k = 1f - Mathf.Exp(-_ikData._bankSharpness * Mathf.Max(Time.deltaTime, 1e-5f));
            _bankDegSmoothed = Mathf.Lerp(_bankDegSmoothed, bankDegTarget, k);
            float bankDeg = _bankDegSmoothed * air;

            // --- 이펙터 참조 ---
            var body = s.bodyEffector;
            var leftE = s.leftFootEffector;
            var rightE = s.rightFootEffector;

            // --- 회전 타깃(절대): 뼈회전 × 뱅크 (원하면 유지) ---
            Quaternion qBankAll = Quaternion.AngleAxis(bankDeg, fwd);
            Quaternion qBankHip = Quaternion.AngleAxis(bankDeg * _ikData._pelvisBankRatio, fwd);

            Quaternion bodytarget = qBankHip * body.bone.rotation;
            Quaternion lefttarget = qBankAll * leftE.bone.rotation;
            quaternion righttarget = qBankAll * rightE.bone.rotation;
            
            var sharpness = MovementData.IK_Sharpness;
            var lerpValue = 1 - Mathf.Exp(-sharpness * Time.deltaTime);
            body.rotation = Quaternion.Slerp(body.rotation, bodytarget, lerpValue);
            leftE.rotation = Quaternion.Slerp(leftE.rotation, lefttarget, lerpValue);
            rightE.rotation = Quaternion.Slerp(rightE.rotation, righttarget, lerpValue);

            // --- 속도/요율 ---
            Vector3 v = Movement.CurrentMoveVelocity;
            v.y = 0f;
            float dt = Mathf.Max(Time.deltaTime, 1e-5f);
            float kVel = 1f - Mathf.Exp(-_ikData._velLPSharpness * dt);
            _vLP = Vector3.Lerp(_vLP, v, kVel);

            Vector3 fwdNow = Vector3.ProjectOnPlane(basis.forward, up).normalized;
            float yawRate = Vector3.SignedAngle(_prevFwdPlanar, fwdNow, up) / dt;
            float speed = _vLP.magnitude;

            float wSpd = Mathf.Clamp01(speed / Mathf.Max(_ikData._speedRef, 1e-3f));
            float wTurn = Mathf.Clamp01(Mathf.Abs(speed * yawRate * Mathf.Deg2Rad) / Mathf.Max(_ikData._yawAccRef, 1e-3f));

            // --- 좌우 관성 기호 ---
            float latVel = Vector3.Dot(_vLP, right);
            float inertiaSign = (Mathf.Abs(latVel) > _ikData._latDeadzone) ? -Mathf.Sign(latVel) : 0f;
            // 공중 posWeight 설정(지상=0, 공중은 최소치 보장)

            // float airposweight = _ikData._airPosWeight; 
            // leftE.positionWeight = airposweight;
            // rightE.positionWeight = airposweight;

            Vector3 backDir = (speed > 1e-4f ? _vLP.normalized : fwd);
            float wTrail = Mathf.Max(wSpd, wTurn);
            float wSide = Mathf.Max(wTurn, wSpd) * 1.1f;

            Vector3 footBack = -backDir * (MovementData.footBackMax * wTrail);
            Vector3 footUp = up * (MovementData.footUpMax * wTrail);
            Vector3 footSide = right * (MovementData.footSideMax * wSide * inertiaSign);

            var grounder = Movement.GrounderIK;
            var legs = grounder?.solver?.legs;
            if (legs == null || legs.Length < 2)
                return;

            float lAir = legs[0].heightFromGround;
            float rAir = legs[1].heightFromGround;
            float airL = Mathf.Max(lAir, MovementData.IK_POWER);
            float airR = Mathf.Max(rAir, MovementData.IK_POWER);

            Vector3 lOff = (footBack + footUp + footSide) * airL;
            Vector3 rOff = (footBack + footUp + footSide) * airR;

            leftE.position = Vector3.Slerp(leftE.position, leftE.bone.position, lerpValue);
            rightE.position = Vector3.Slerp(leftE.position, rightE.bone.position, lerpValue);

            leftE.positionOffset = Vector3.Slerp(leftE.positionOffset, lOff, lerpValue);
            rightE.positionOffset = Vector3.Slerp(rightE.positionOffset, rOff, lerpValue);

            leftE.maintainRelativePositionWeight  = 0f;
            rightE.maintainRelativePositionWeight = 0f;

            _prevFwdPlanar = fwdNow;            
        }

        private T UpdateSlerp<T>(T src, T target, float k, System.Func<T, T, float, T> action) where T : struct
        {
            return action(src, target, k);
        }
    }
}