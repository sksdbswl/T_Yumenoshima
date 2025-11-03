using System.Collections.Generic;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;

        public partial class CharacterAnimation
        {
            private List<AnimationEventListener> _eventListenerList;
            private CharacterAnimationEventListener _characterEventListener;
            private CharacterWallClimbAnimationEventListener _wallClimbEventListener;
            // private CharacterGrappleAnimationEventListener _grappleEventListener;
            // private CharacterGatheringAnimationEventListener _gatheringEventListener;
            private CharacterFishingAnimationEventListener _fishingEventListener;

            protected override void InitializeAnimationEventListener()
            {
                base.InitializeAnimationEventListener();

                if (Character == null)
                    return;

                _characterEventListener = new CharacterAnimationEventListener(Character.EventBus);
                {
                    _characterEventListener.WalkEvent += (InEnable) => { Movement.IsWalkInput = InEnable; };

                    _characterEventListener.JumpEvent += () => { Movement.IsJumpInput = true; };

                    _characterEventListener.JumpStartedEvent += () => { StateMachine.Jump.StartJump(); };

                    _characterEventListener.LandedEvent += () => { Movement.IsLanding = true; };

                    _characterEventListener.JumpCollisionDetected += () =>
                    {
                        Movement.SetAirbornStateGrounderIKMaxStep(false);
                    };

                    _characterEventListener.DashEvent += (InStartSprintCallback) =>
                    {
                        Movement.IsDashInput = true;
                        StateMachine.Sprint.StartSprintEvent += InStartSprintCallback;
                    };

                    _characterEventListener.SprintEvent += (InEnable) =>
                    {
                        Movement.IsSprintInput = InEnable;
                    };

                    _characterEventListener.MountEvent += (InEnable) => { Movement.IsMountInput = InEnable; };
                }

                _wallClimbEventListener = new CharacterWallClimbAnimationEventListener(Character.EventBus);
                {
                    _wallClimbEventListener.GravityChangedEvent +=
                        (InWorldGravity) => Movement.GravityChange(InWorldGravity);
                }

                // _grappleEventListener = new CharacterGrappleAnimationEventListener(Character.EventBus);
                // {
                //     _grappleEventListener.GrappleRequestedEvent += (InTarget, InGrapplePosition, InGrappleDistance,
                //         InFar, InStartGrappleCallback) =>
                //     {
                //         if (StateMachine.Grapple.IsEnableThrowGrapple)
                //         {
                //             Movement.IsGrappleInput = true;
                //             StateMachine.Grapple.SetGrappleInfo(
                //                 GrappleAnimationState.GrappleInformation.Create(InTarget, InGrapplePosition,
                //                     InGrappleDistance, InFar, InStartGrappleCallback));
                //         }
                //         else
                //         {
                //             InStartGrappleCallback?.Invoke(false);
                //         }
                //     };
                //
                //     _grappleEventListener.GrappleStartedEvent += (InTarget, InGrappleMoveTime) =>
                //     {
                //         StateMachine.Grapple.StartGrapple(InTarget, InGrappleMoveTime);
                //     };
                //
                //     _grappleEventListener.GrappleArrivalEvent += () =>
                //     {
                //         StateMachine.Grapple.ArriveGrapple();
                //     };
                //
                //     _grappleEventListener.GrappleLaunchRequestedEvent += (InStartLaunchCallback) =>
                //     {
                //         StateMachine.Grapple.LaunchRequested(InStartLaunchCallback);
                //     };
                //
                //     _grappleEventListener.GrappleLaunchLandedEvent += () =>
                //     {
                //         StateMachine.Grapple.LandingLaunch();
                //     };
                // }
                //
                // _gatheringEventListener = new CharacterGatheringAnimationEventListener(Character.EventBus);
                // {
                //     _gatheringEventListener.StartGatheringEvent += (InGatheringType, gatheringSpeed) =>
                //     {
                //         StateMachine.Gathering.PlayAnimationType =
                //             StateMachine.Gathering.ConvertToAnimationType(InGatheringType);
                //         StateMachine.Gathering.PlayAnimationSpeed = gatheringSpeed;
                //     };
                //
                //     _gatheringEventListener.StopGatheringEvent += () =>
                //     {
                //         StateMachine.Gathering.PlayAnimationType = eAnimationType.NONE;
                //     };
                //     
                //     _gatheringEventListener.StartGatheringSuccessEvent += () =>
                //     {
                //         StateMachine.Gathering.PlayAnimationType = eAnimationType.GATHERING_SUCCESS;
                //         StateMachine.Gathering.PlayAnimationSpeed = 1f;
                //     };
                // }

                _fishingEventListener = new CharacterFishingAnimationEventListener(Character.EventBus);
                {
                    _fishingEventListener.PlayFishingAnimation += (type) =>
                    {
                        StateMachine.Fishing.PlayAnimationType = type;
                    };
                }

                Character.OnInitialized += () => { _characterEventListener.Register(Character.EventBus); };

                SetEventListenerList();
            }

            private void SetEventListenerList()
            {
                _eventListenerList = new()
                {
                    _characterEventListener,
                    _wallClimbEventListener,
                    // _grappleEventListener,
                    // _gatheringEventListener,
                    _fishingEventListener,
                };
            }

            private void RegisterEvents()
            {
                if (_eventListenerList == null)
                    return;
                
                _eventListenerList.ForEach(eventListener => eventListener?.Register(Character.EventBus));
            }

            private void UnregisterEvents()
            {
                _eventListenerList?.ForEach(eventListener => eventListener?.Unregister(Character.EventBus));
            }

            public virtual void ResetEventData()
            {
            }
        }
}
