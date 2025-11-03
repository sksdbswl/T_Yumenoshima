using System;
using Animancer;
using UnityEngine;

namespace REIW.Animations.Character
{
    using eAnimationType = CharacterAnimationEnums.eAnimationType;
    using eStateType = CharacterAnimationEnums.eStateType;

    public class FishingAnimationState : CharacterAnimationState
    {
        public override eStateType StateType => eStateType.FISHING;

        public override bool CanExitState => base.CanExitState || _playingAniState == null;

        protected eAnimationType _playAnimationType;
        protected float _playAnimationSpeed = 1f;

        [SerializeField] private float struggleBlendSpeed = 0.2f;
        [SerializeField] private LinearMixerTransition struggleMixer;

        [SerializeField] private ClipTransitionSequence castingClip;
            //[SerializeField] private ClipTransition waitingClip;
        [SerializeField] private ClipTransitionSequence liftingClip;
        [SerializeField] private ClipTransitionSequence readyClip;
        [SerializeField] private ClipTransition reelClip;
        [SerializeField] private ClipTransition readyLoopClip;
        [SerializeField] private ClipTransition missClip;

        private float struggleParameter;

        protected override void Start()
        {
            base.Start();
            
            AddIgnoreChangeNetObjectAnimationType(eAnimationType.FISHING_FIGHTING);
            AddIgnoreChangeNetObjectAnimationType(eAnimationType.FISHING_FIGHTING_LEFT);
            AddIgnoreChangeNetObjectAnimationType(eAnimationType.FISHING_FIGHTING_RIGHT);
            AddIgnoreChangeNetObjectAnimationType(eAnimationType.FISHING_REEL);
            //AddIgnoreChangeNetObjectAnimationType(eAnimationType.FISHING_MISS);
        }

        protected override void Awake()
        {
            base.Awake();
            
        }

        public eAnimationType PlayAnimationType
        {
            set
            {
                _playAnimationType = value;
                Movement.IsFishingInput = value != eAnimationType.NONE;
                if (!Movement.IsFishingInput)
                    _playingAniState = null;
                else
                    PlayAnimation();
            }
        }

        protected void PlayAnimation()
        {
            _playingAniState = InternalPlayAnimation(_playAnimationType);
        }

        private float GetFishingAnimationParameter(eAnimationType InAnimationType)
        {
            return InAnimationType - eAnimationType.FISHING_TYPE_START;
        }

        public override bool IsPlayingAnimation(in float InNormalizedTime)
        {
            if (base.IsPlayingAnimation(InNormalizedTime)
            || (Animancer.States.TryGet(castingClip, out var state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(liftingClip, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(readyClip, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(reelClip, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(readyLoopClip, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(struggleMixer, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime)
            || (Animancer.States.TryGet(missClip, out state) && state.IsPlaying && state.NormalizedTime < InNormalizedTime))
                return true;
            return false;
        }

        protected override AnimancerState InternalPlayAnimation(in eAnimationType InAnimationType,
            in float InAnimationSpeed = 1f, in Func<AnimancerState, float> InCalculateSpeedFunc = null)
        {
            AnimancerState state = null;

            switch (InAnimationType)
            {
                case eAnimationType.FISHING_CASTING:
                    state = Animation.PlayAnimation(InAnimationType, castingClip, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if(!IsLocal) PlayOtherRodAnim(InAnimationType);
                    break;
                case eAnimationType.FISHING_FIGHTING:
                case eAnimationType.FISHING_FIGHTING_LEFT:
                case eAnimationType.FISHING_FIGHTING_RIGHT:
                    state = Animation.PlayAnimation(InAnimationType, struggleMixer, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    break;
                case eAnimationType.FISHING_LIFTING:
                    state = Animation.PlayAnimation(InAnimationType, liftingClip, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if(!IsLocal) PlayOtherRodAnim(InAnimationType);
                    break;
                case eAnimationType.FISHING_READY:
                    state = Animation.PlayAnimation(InAnimationType, readyClip, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    if(!IsLocal) PlayOtherRodAnim(InAnimationType);
                    break;
                case eAnimationType.FISHING_READY_LOOP:
                    state = Animation.PlayAnimation(InAnimationType, readyLoopClip, InAnimationSpeed,
                        InCalculateSpeedFunc);
                    break;
                case eAnimationType.FISHING_REEL:
                    state = Animation.PlayAnimation(InAnimationType, reelClip, InAnimationSpeed, InCalculateSpeedFunc);
                    break;
                case eAnimationType.FISHING_MISS:
                    if (IsLocal)
                    {
                        // 로컬 캐릭터는 Miss 애니메이션 재생 
                        state = Animation.PlayAnimation(InAnimationType, missClip, InAnimationSpeed,
                            InCalculateSpeedFunc);
                        SetAnimationEndEvent(state, OnAnimation_MissEndEvent);
                    }
                    else if (Animation is NetworkCharacterAnimation netAnim)
                    {
                        // 타 캐릭터는 바로 Idle로 전환
                        netAnim.PlayAnimation(eAnimationType.IDLE);
                    }

                    break;
            }

            SetUseRootMotion(state);

            return state;
        }
        
        // 타 캐릭터 용. 로컬 캐릭터는 FishingSystem에서 처리
        private void PlayOtherRodAnim(eAnimationType type)
        {
            //IngameFishingSystem.Instance.PlayOtherRodAnim(Character.DatabaseID, type);
        }

        void OnAnimation_MissEndEvent()
        {
            Movement.IsFishingInput = false;
            _playAnimationType = eAnimationType.NONE;
            _playingAniState = null;
        }

        // 로컬 캐릭터 용
        // 루어가 수면에 도착한 순간 FX 재생 목적
        public void OnAnimation_LureStartEvent()
        {
            if (IsLocal)
            {
                //IngameFishingSystem.Instance.OnThrowLureLocal();
            }
        }

        // 로컬 캐릭터 용
        // 물고기가 수면에서 튀어나오는 순간 FX 재생 목적
        public void OnAnimation_LiftUpEvent()
        {
            if (IsLocal)
            {
                //IngameFishingSystem.Instance.PlayFishCatchEffect();
            }
        }
        
        public override bool LateUpdateState()
        {
            if (_playAnimationType is >= eAnimationType.FISHING_FIGHTING_LEFT
                and <= eAnimationType.FISHING_FIGHTING_RIGHT)
            {
                struggleParameter =
                    Mathf.Lerp(struggleParameter, GetFishingAnimationParameter(_playAnimationType),
                        struggleBlendSpeed);
                struggleMixer.State.Parameter = struggleParameter;
                struggleMixer.State.RecalculateWeights();
            }
            else struggleParameter = GetFishingAnimationParameter(eAnimationType.FISHING_FIGHTING);

            return base.LateUpdateState();

        }
    }
}