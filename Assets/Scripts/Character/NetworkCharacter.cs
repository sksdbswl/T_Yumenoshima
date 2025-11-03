using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using UnityEngine;

namespace REIW
{
    public class NetworkCharacter : CharacterBase
    {   
        [Space(5)][Header("Movement")] 
        
        [SerializeField] private bool externalDriveMode = false; // 좌석 부착 시 내부 보간 끔

        public override bool IsLocalCharacter => false;

        public void SetExternalDriveMode(bool on) => externalDriveMode = on;
        public bool IsExternalDriven => externalDriveMode;
        
        private Transform _transform;
        
        private Vector3 _targetPos;
        private Vector3 _prevPos;
        
        private Quaternion _targetRot;
        private Quaternion _prevRot;

        private float _targetForwardSpdParam;
        private float _prevForwardSpdParam;
        
        private float _targetVerticalSpdParam;
        private float _prevVerticalSpdParam;
        
        private float _lastTargetUpdateTime = 0.0f;
        
        private eInterPolationType _lastInterpolationType = eInterPolationType.None;
        
        private bool _finalPoseApplied = false;
        private float _elapsed = 0.0f;
        private float _progressR = 0.0f;
        float _progress = 0.0f; 
        
        private Vector3 _smoothedPos;
        private Quaternion _smoothedRot;
        private float _curAnimForwardSpeed;
        private float _curAnimVerticalSpeed;
        

        private void Awake()
        {
            _transform = transform; // 캐싱
        }

        public override void Initialize()
        {
            base.Initialize();
            
            SetDirectPosRot( transform.position, transform.rotation );
            
            currentMoveVelocity = Vector3.zero;
        }

        private void SetDirectPosRot(Vector3 pos, Quaternion rot)
        {
            // targetPos = position;
            // targetRot = rotation.normalized;
            _lastInterpolationType = eInterPolationType.None;
            _finalPoseApplied = true;
        }

        enum eInterPolationType
        {
            None,
            PosRotation, //pos + rot + ani
            Animation,   // only ani
        }

        public void SetNextMoveSnapShot(Vector3 pos, Quaternion rot, float forwardSpeed, float verticalSpeed)
        {
            _targetPos = pos;
            _targetRot = rot;
            _targetForwardSpdParam = forwardSpeed;
            _targetVerticalSpdParam = verticalSpeed;
            
            _prevPos = _transform.position;
            _prevRot = _transform.rotation;
            _prevForwardSpdParam = CharacterAnimation.ForwardSpeedParameter;
            _prevVerticalSpdParam = CharacterAnimation.VerticalSpeedParameter;
            
            _lastInterpolationType = eInterPolationType.PosRotation;
            _finalPoseApplied = false;
            _lastTargetUpdateTime = Time.time;
        }

        public void SetNextMoveSnapShot(float forwardSpeed, float verticalSpeed)
        {
            _prevForwardSpdParam = CharacterAnimation.ForwardSpeedParameter;
            _prevVerticalSpdParam = CharacterAnimation.VerticalSpeedParameter;
            
            _targetForwardSpdParam = forwardSpeed;
            _targetVerticalSpdParam = verticalSpeed;
            
            _lastInterpolationType = eInterPolationType.Animation;
            _finalPoseApplied = false;
            _lastTargetUpdateTime = Time.time;
        }
        
        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool directly = true)
        {
            if (directly)
            {
                base.SetPositionAndRotation(position, rotation.normalized, true);
                SetDirectPosRot(position, rotation);
            }
        }
        
        private void SetAnim(float forward, float vertical)
        {
            CharacterAnimation.ForwardSpeedParameter = forward;
            CharacterAnimation.VerticalSpeedParameter = vertical;
        }
        
        private void LerpAnim(float t)
        {
            CharacterAnimation.ForwardSpeedParameter =
                Mathf.Lerp(_prevForwardSpdParam, _targetForwardSpdParam, t);

            CharacterAnimation.VerticalSpeedParameter =
                Mathf.Lerp(_prevVerticalSpdParam, _targetVerticalSpdParam, t);
        }
        protected override void FixedUpdate()
        {
            // base.FixedUpdate();
            if (externalDriveMode || _finalPoseApplied)
            {
                return;
            }
             _elapsed = Time.time - _lastTargetUpdateTime;
             _progressR = _elapsed / Constant.SnapShotInterval; // 이동은 0.04f 단위에 한번씩 꺼내기 때문에 

             if (_lastInterpolationType == eInterPolationType.PosRotation)
             {
                 
                 // bool farJump = Vector3.Distance(_prevPos, _targetPos) >= teleportDistance;
                 // bool largeAngle = Quaternion.Angle(_prevRot, _targetRot) >= TELEPORT_ANGLE_DEG;

                 if (_progressR >= 1f)
                 {
                     _transform.SetPositionAndRotation(_targetPos, _targetRot);
                     SetAnim( _targetForwardSpdParam, _targetVerticalSpdParam );
                     _finalPoseApplied = true;
                 }
                 else
                 {
                     _progress = Mathf.Clamp01(_progressR); // targetPos 주기(0.04s) 기준
                
                     _smoothedPos = Vector3.Lerp(_prevPos, _targetPos, _progress);
                     _smoothedRot = Quaternion.Slerp(_prevRot , _targetRot, _progress);
                
                     _transform.SetPositionAndRotation(_smoothedPos, _smoothedRot);
                     
                     LerpAnim(_progress);
                 }     
             }
             else if (_lastInterpolationType == eInterPolationType.Animation)
             {
                 if (_progressR >= 1f)
                 {
                      SetAnim( _targetForwardSpdParam, _targetVerticalSpdParam );
                     _finalPoseApplied = true;
                 }
                 else
                 {
                     _progress = Mathf.Clamp01(_progressR); // targetPos 주기(0.04s) 기준
                     
                     LerpAnim(_progress);
                 }     
             }
            // Debug.LogWarning("elapsed:" + elapsed +", progressR:" +progressR);
        }
        
        
        float _nextCheck;     // 다음 체크 시각
        Vector3 _lastLocalPos; // 로컬 플레이어 위치 캐시(선택, 흔들림 방지용)
        void OnEnable()
        {
            // 시작 시점 랜덤 페이즈로 분산 → N개가 같은 프레임에 몰려 실행되는 걸 방지
            _nextCheck = Time.time + checkInterval;
        }

        float checkInterval = 0.3f;
        private Vector3 delta;
        protected override void Update()
        {
            // base.Update();
            
            if (Time.time < _nextCheck) 
                return;
            
            _nextCheck = Time.time + checkInterval;
            
            // Debug.LogError(_transform.position);
            // Debug.LogError(LocalCharacter.Transform.position);

            if (LocalCharacter.Transform == null)
            {
                return;
            }
             delta = _transform.position - LocalCharacter.Transform.position;
             SqrMagnitude = delta.sqrMagnitude;
             
             // Debug.LogWarning("Name:" + gameObject.name +":" + SqrMagnitude);
        }
    }
}