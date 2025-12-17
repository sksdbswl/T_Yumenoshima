using System;
using System.Collections.Generic;
using UnityEngine;
using REIW;

public interface ICharacterEventListener { }

/// <summary>
/// Basic character input/movement events (kept for local dummy logic).
/// </summary>
public interface ICharacterBaseEventListener : ICharacterEventListener
{
    void OnMoveStarted();              // 이동 키 입력 (이동 시작)

    void OnJumpRequested();     // 점프 키 입력
    void OnJumpStarted();       // 점프 시작(실제 캐릭터가 위로 올라가기 시작)
    void OnJumpLanded();        // 점프 종료
    void OnJumpCollisionDetected(); // 점프 중 충돌
            
    void OnDashRequested();     // 대시 키 입력
    void OnDashStarted();       // 대시 시작
            
    void OnSprintRequested();   // 스프린트 키 입력
    void OnSprintStarted();     // 스프린트 시작(진행 중)
    void OnSprintReleased();    // 스프린트 키 해제
            
    void OnWalkRequested();     // 걷기 키 입력
    void OnWalkReleased();      // 걷기 키 해제

    void OnMountRequested();    // 탈 것 키 입력
    void OnMountStarted();    // 탈 것 키 입력
    void OnMountReleased();     // 탈 것 키 해제

    void OnCancelCurrentMovement(); // 현재 이동상태 취소
}

/// <summary>
/// Wall climb events (kept because camera offsets can react to gravity change).
/// </summary>
public interface IMoveWallClimbEventListener : ICharacterEventListener
{
    void OnGravityChangeStarted(bool isDownSnapping); // 중력 변화 시작(isDownSnapping:true => 떨어지면서 Snapping 되는 상태를 의미)
    void OnGravityChangeFinished(bool worldGravity); // 중력 변화 완료
    void OnWallClimbStarted();          // 벽타기 모드 활성화
    void OnWallClimbFinished();         // 벽타기 모드 종료
    void OnFailedMovementToEdge(Vector3 failPoint, Vector3 failNormal); // 스냅 시도 실패 시 이벤트
}

/// <summary>
/// Parkour events (kept; ParkourActionData is included in this dummy set).
/// </summary>
public interface IMoveParkourEventListener : ICharacterEventListener
{
    void OnParkourRequested(ParkourActionData actionData, Action<bool> funcStartParkour);
    void OnParkourStarted(ParkourActionData actionData, Action funcFinishedParkour);
    void OnParkourFinished();
}

// public interface IGatheringEventListener : ICharacterEventListener
// {
//     void OnStartGathering(EnumGathering gatheringType, float gatheringSpeed = 1f);
//     void OnStopGathering();
//     void OnStartGatheringSuccess();
// }


/// <summary>
/// 인터페이스
/// IMoveWallClimbEventListener (중력 변화/월클라임 시작/끝/실패지점 등)
/// IMoveParkourEventListener (파쿠르 요청/시작/끝)
/// CharacterBaseEventBus 자체는 단순 List 기반으로:
/// Register / Unregister
/// Post<T>(Action<T>) 형태로 유지
///    → “리스너 중 T를 구현한 애들만 호출”하는 가장 단순한 버전
/// </summary>
public class CharacterBaseEventBus
{
    private readonly List<ICharacterEventListener> listeners = new();

    public void Register(ICharacterEventListener listener)
    {
        if (listener != null && !listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void Unregister(ICharacterEventListener listener)
    {
        listeners.Remove(listener);
    }

    public void Post<T>(Action<T> eventAction) where T : ICharacterEventListener
    {
        if (eventAction == null) return;

        foreach (var listenerObj in listeners)
        {
            if (listenerObj is T listener)
                eventAction.Invoke(listener);
        }
    }
}
