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
