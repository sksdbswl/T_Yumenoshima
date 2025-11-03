using System;
using UnityEngine;

namespace REIW.Animations.Character
{
    public class CharacterAnimationEventListener : AnimationEventListener, ICharacterBaseEventListener
        {
            public event Action<bool> WalkEvent;
            public event Action<bool> SprintEvent;
            public event Action<bool> MountEvent;
            public event Action<Action> DashEvent;
            public event Action JumpEvent;
            public event Action JumpStartedEvent;
            public event Action LandedEvent;
            public event Action JumpCollisionDetected;

            public CharacterAnimationEventListener(CharacterBaseEventBus eventBus) : base(eventBus)
            {
            }

            public void OnMoveStarted()
            {
            }

            public void OnWalkRequested()
            {
                WalkEvent?.Invoke(true);
            }

            public void OnWalkReleased()
            {
                WalkEvent?.Invoke(false);
            }

            public void OnDashRequested(Action funcStartSprint)
            {
                DashEvent?.Invoke(funcStartSprint);
            }

            public void OnSprintRequested()
            {
                SprintEvent?.Invoke(true);
            }

            public void OnSprintReleased()
            {
                SprintEvent?.Invoke(false);
            }

            public void OnJumpRequested()
            {
                JumpEvent?.Invoke();
            }

            public void OnJumpStarted()
            {
                JumpStartedEvent?.Invoke();
            }

            public void OnJumpLanded()
            {
                LandedEvent?.Invoke();
            }

            public void OnJumpCollisionDetected()
            {
                JumpCollisionDetected?.Invoke();
            }

            public void OnMountRequested()
            {
                MountEvent?.Invoke(true);
            }

            public void OnMountReleased()
            {
                PlayerController.Instance.CurrentExecuteActionTypeStateType = eStaminaActionType.Normal;
                MountEvent?.Invoke(false);
            }
        }

        public class CharacterWallClimbAnimationEventListener : AnimationEventListener, IMoveWallClimbEventListener
        {
            public event Action<bool> GravityChangedEvent;

            public CharacterWallClimbAnimationEventListener(CharacterBaseEventBus eventBus) : base(eventBus)
            {
            }

            public void OnGravityChangeStarted(bool isDownSnapping)
            {
            }

            public void OnGravityChangeFinished(bool worldGravity)
            {
                GravityChangedEvent?.Invoke(worldGravity);
            }

            public void OnWallClimbStarted()
            {
            }

            public void OnWallClimbFinished()
            {
            }
        }

        public class CharacterGrappleAnimationEventListener : AnimationEventListener, IMoveGrappleEventListener
        {
            public event Action<GrapplePoint, Vector3, float, bool, Action<bool>> GrappleRequestedEvent;
            public event Action<GrapplePoint, float> GrappleStartedEvent;
            public event Action GrappleArrivalEvent;
            public event Action<Action<bool>> GrappleLaunchRequestedEvent;
            public event Action GrappleLaunchLandedEvent;
            public event Action<GrapplePoint, GrapplePoint, Vector3> GrapplePointTargetedEvent;

            public CharacterGrappleAnimationEventListener(CharacterBaseEventBus eventBus) : base(eventBus)
            {
            }

            public void OnGrappleRequested(GrapplePoint target, Vector3 grapplePosition, float grappleDistance,
                bool isFar, Action<bool> funcStartGrapple)
            {
                GrappleRequestedEvent?.Invoke(target, grapplePosition, grappleDistance, isFar, funcStartGrapple);
            }

            public void OnGrappleStarted(GrapplePoint target, float grappleMoveTime)
            {
                GrappleStartedEvent?.Invoke(target, grappleMoveTime);
            }

            public void OnGrappleArrival()
            {
                GrappleArrivalEvent?.Invoke();
            }

            public void OnGrappleLaunchRequested(Action<bool> funcStartLaunch)
            {
                GrappleLaunchRequestedEvent?.Invoke(funcStartLaunch);
            }

            public void OnGrappleLaunchStarted()
            {
            }

            public void OnGrappleLaunchLanding()
            {
                GrappleLaunchLandedEvent?.Invoke();
            }

            public void OnGrapplePointTargeted(GrapplePoint prev, GrapplePoint target, Vector3 grapplePosition)
            {
                GrapplePointTargetedEvent?.Invoke(prev, target, grapplePosition);
            }
        }

        public class CharacterGatheringAnimationEventListener : AnimationEventListener, IGatheringEventListener
        {
            public CharacterGatheringAnimationEventListener(CharacterBaseEventBus eventBus) : base(eventBus)
            {
            }

            public event Action<EnumGathering, float> StartGatheringEvent;
            public event Action StopGatheringEvent;
            public event Action StartGatheringSuccessEvent;

            public void OnStartGathering(EnumGathering gatheringType, float gatheringSpeed = 1f)
            {
                StartGatheringEvent?.Invoke(gatheringType, gatheringSpeed);
            }

            public void OnStopGathering()
            {
                StopGatheringEvent?.Invoke();
            }

            public void OnStartGatheringSuccess()
            {
                StartGatheringSuccessEvent?.Invoke();
            }
        }

        public class CharacterFishingAnimationEventListener : AnimationEventListener, IFishingEventListener
        {
            public event Action<CharacterAnimationEnums.eAnimationType> PlayFishingAnimation;

            public CharacterFishingAnimationEventListener(CharacterBaseEventBus eventBus) : base(eventBus)
            {
            }

            public void OnFishing(CharacterAnimationEnums.eAnimationType animationType)
            {
                PlayFishingAnimation?.Invoke(animationType);
            }
        }
}
