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
    void OnMoveStarted();

    void OnJumpRequested();
    void OnJumpStarted();
    void OnJumpLanded();
    void OnJumpCollisionDetected();

    void OnDashRequested();
    void OnDashStarted();

    void OnSprintRequested();
    void OnSprintStarted();
    void OnSprintReleased();

    void OnWalkRequested();
    void OnWalkReleased();

    void OnMountRequested();
    void OnMountStarted();
    void OnMountReleased();

    void OnCancelCurrentMovement();
}

/// <summary>
/// Wall climb events (kept because camera offsets can react to gravity change).
/// </summary>
public interface IMoveWallClimbEventListener : ICharacterEventListener
{
    void OnGravityChangeStarted(bool isDownSnapping);
    void OnGravityChangeFinished(bool worldGravity);
    void OnWallClimbStarted();
    void OnWallClimbFinished();
    void OnFailedMovementToEdge(Vector3 failPoint, Vector3 failNormal);
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
