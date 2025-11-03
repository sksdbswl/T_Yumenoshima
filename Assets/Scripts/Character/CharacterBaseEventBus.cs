using System;
using System.Collections.Generic;
using REIW.Animations.Character;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REIW
{
    public interface ICharacterEventListener { }
    
    /// <summary>
    /// 캐릭터 기본 조작 이벤트
    /// </summary>
    public interface ICharacterBaseEventListener : ICharacterEventListener
    {
        void OnMoveStarted();              // 이동 키 입력 (이동 시작)

        void OnJumpRequested();     // 점프 키 입력
        void OnJumpStarted();       // 점프 시작(실제 캐릭터가 위로 올라가기 시작)
        void OnJumpLanded();        // 점프 종료
        void OnJumpCollisionDetected(); // 점프 중 충돌
            
        void OnDashRequested(Action funcStartSprint);     // 대시 키 입력
            
        void OnSprintRequested();   // 스프린트 키 입력
        void OnSprintReleased();    // 스프린트 키 해제
            
        void OnWalkRequested();     // 걷기 키 입력
        void OnWalkReleased();      // 걷기 키 해제

        void OnMountRequested();    // 탈 것 키 입력
        void OnMountReleased();     // 탈 것 키 해제
    }
    
    /// <summary>
    /// 벽타기 이벤트 
    /// </summary>
    public interface IMoveWallClimbEventListener : ICharacterEventListener
    {
        void OnGravityChangeStarted(bool isDownSnapping); // 중력 변화 시작(isDownSnapping:true => 떨어지면서 Snapping 되는 상태를 의미)
        void OnGravityChangeFinished(bool worldGravity); // 중력 변화 완료
        void OnWallClimbStarted();          // 벽타기 모드 활성화
        void OnWallClimbFinished();         // 벽타기 모드 종료
    }

    /// <summary>
    /// 그래플 이벤트
    /// </summary>
    public interface IMoveGrappleEventListener : ICharacterEventListener
    {
        // void OnGrappleRequested(GrapplePoint target, Vector3 grapplePosition, float grappleDistance, bool isFar, Action<bool> funcStartGrapple); // 그래플 키 입력
        // void OnGrappleStarted(GrapplePoint target, float grappleMoveTime);            // 그래플 시작
        void OnGrappleArrival();                                        // 그래플 완료
        void OnGrappleLaunchRequested(Action<bool> funcStartLaunch);    // 런칭(그래플 후 점프) 요청
        void OnGrappleLaunchStarted();                                  // 런칭(그래플 후 점프) 시작
        void OnGrappleLaunchLanding();                                  // 런칭(그래플 후 점프) 착지
        //void OnGrapplePointTargeted(GrapplePoint prev, GrapplePoint target, Vector3 grapplePosition);  // 그래플 포인트 변경
    }

    /// <summary>
    /// 채집 이벤트
    /// </summary>
    public interface IGatheringEventListener : ICharacterEventListener
    {
        //void OnStartGathering(EnumGathering gatheringType, float gatheringSpeed = 1f);
        void OnStopGathering();
        void OnStartGatheringSuccess();
    }
    
    public interface IFishingEventListener : ICharacterEventListener
    {
        void OnFishing(CharacterAnimationEnums.eAnimationType animationType);
    }

    public interface ICharacterStateEventListener : ICharacterEventListener
    {
        //void OnChangeStaminaActionType(eStaminaActionType staminaActionType);
    }
    
    public class CharacterBaseEventBus
    {
        private readonly List<ICharacterEventListener> listeners = new ();

        public void Register(ICharacterEventListener listener)
        {
            listeners.Add(listener);
        }
        
        public void Unregister(ICharacterEventListener listener)
        {
            listeners.Remove(listener);
        }
        
        public void Post<T>(Action<T> eventAction) where T : ICharacterEventListener
        {
            foreach (var listenerObj in listeners)
            {
                if(listenerObj is T listener)
                    eventAction?.Invoke(listener);
            }
        }
    }
}