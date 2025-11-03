using System;
using System.Collections.Generic;
using Animancer.FSM;
using UnityEngine;

namespace REIW.Animations
{
    public abstract class AnimationStateMachine<TAnimationType, TStateType, TState, TStateMachine, TAnimation> : StateMachine<TState>.WithDefault
        where TAnimationType : Enum
        where TStateType : Enum
        where TState : AnimationState<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TStateMachine : AnimationStateMachine<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
        where TAnimation : AnimationBase<TAnimationType, TStateType, TState, TStateMachine, TAnimation>
    {
        public GameObject StateRoot
        {
            get => _statesRoot;
            set => _statesRoot = value;
        }
        
        [SerializeField] protected GameObject _statesRoot;
        [SerializeField, Range(0, 1)] private float _checkPlayingAnimationNormalizedTime = 0.8f;

        protected Dictionary<TStateType, TState> _dicStates = new();
        protected TState[] _secondaryCheckNextStateList;

        public new TState CurrentState { get; set; }
        public new TState PreviousState { get; set; }

        public virtual bool IsPlayingAnyAnimation
        {
            get
            {
                if (IsPlayingPrevStateAnimation)
                    return true;
                if (CurrentState && CurrentState.IsPlayingAnimation(_checkPlayingAnimationNormalizedTime))
                    return true;
                return false;
            }
        }

        public virtual bool IsPlayingPrevStateAnimation
        {
            get
            {
                if (PreviousState && PreviousState.IsPlayingAnimation(_checkPlayingAnimationNormalizedTime))
                    return true;
                return false;
            }
        }

        public virtual void Initialize(in TAnimation InAnimation)
        {
            if (!_statesRoot)
                return;

            _dicStates.Clear();

            var states = _statesRoot.GetComponentsInChildren<TState>(true);
            foreach (var state in states)
            {
                _dicStates.TryAdd(state.StateType, state);
                state.Initialize(InAnimation);
            }

            SetSecondaryCheckStates();
        }

        protected virtual void SetSecondaryCheckStates()
        {
        }

        public TState GetNextStateSecondaryCheck()
        {
            for (int i = 0; i < _secondaryCheckNextStateList.Length; ++i)
            {
                if (_secondaryCheckNextStateList[i].CanEnterState)
                    return _secondaryCheckNextStateList[i];
            }

            return null;
        }

        public TState GetAnimationState(in TStateType InType)
        {
            return _dicStates.GetValueOrDefault(InType);
        }

        public T GetAnimationState<T>(in TStateType InType) where T : TState
        {
            return GetAnimationState(InType) as T;
        }

        public T GetAnimationState<T>() where T : TState
        {
            foreach (var state in _dicStates.Values)
            {
                if (state is T t) 
                    return t;
            }

            return null;
        }
    }
}
