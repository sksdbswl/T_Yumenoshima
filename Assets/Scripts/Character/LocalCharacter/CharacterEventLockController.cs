using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using REIW.Animations;
using REIW.Animations.Character;

namespace REIW.EventLock
{
    public enum eEventLockType
    {
        None = 0,

        CharacterInputLock = 1 << 0,
        CharacterJump = 1 << 1,
        CharacterDash = 1 << 2,
        CharacterGraple = 1 << 3,
        CharacterMove = 1 << 4,
        CharacterGlide = 1 << 5,
        CharacterMount  = 1 << 6,
        CharacterParkour = 1 << 7,
        CameraRotate = 1 << 8,
        CameraOffset = 1 << 9,

        Max = 10,
        All = (1 << Max) - 1,

        CharacterMoveAllAction = CharacterInputLock | CharacterJump | CharacterDash | CharacterGraple | CharacterMove | CharacterGlide | CharacterMount | CharacterParkour
    }

    public interface ICheckEventLockState
    {
        eEventLockType CurrentEventLockType { get; }
        eEventLockType ReleaseEventLockType { get; }
    }

    public interface ICameraEventType
    {
        //IngameCameraSystem_Event.CameraEventType CameraEventType { get; }
        Vector3 CameraEventOffset { get; set; }
    }

    public class CharacterEventLockController
    {
        public CharacterEventLockController(LocalCharacter character)
        {
            Initialize(character);
        }
        private int _currentEventLockType = (int)(eEventLockType.None);
        public bool IsEventLockType(eEventLockType eventType) => ((int)eventType & _currentEventLockType) != 0;
        
        private int _externalEventLockType = (int)(eEventLockType.None);
        public void SetExternalEventLockType(eEventLockType eventType) => _externalEventLockType |= (int)eventType;
        public bool AnyExternalLockEvent() => _externalEventLockType != 0;
        public void ResetExternalEventLockType() => _externalEventLockType = 0;
        
        private List<ICheckEventLockState> _eventlockList = new List<ICheckEventLockState>();

        private void Initialize(LocalCharacter character)
        {
            if (character == null)
                return;

            // character.CharacterAnimation.Movement.AddInputBufferAction(eCharacterActionInputType.GRAPPLE, (value) =>
            // {
            //     if (value == false)
            //         character.CurrentInputs.Grapple = false;
            //     else
            //     {
            //         character.CurrentInputs.Jump = false;
            //         character.CurrentInputs.Dash = false;
            //         character.CurrentInputs.Parkour = false;
            //     }
            // });
            character.CharacterAnimation.Movement.AddInputBufferAction(eCharacterActionInputType.DASH, (value) =>
            {
                if (value == false)
                    character.CurrentInputs.Dash = false;
                else
                {
                    character.CurrentInputs.Grapple = false;
                    character.CurrentInputs.Jump = false;
                    character.CurrentInputs.Parkour = false;
                }
            });
            character.CharacterAnimation.Movement.AddInputBufferAction(eCharacterActionInputType.JUMP, (value) =>
            {
                if (value == false)
                    character.CurrentInputs.Jump = false;
                else
                {
                    character.CurrentInputs.Grapple = false;
                    character.CurrentInputs.Dash = false;
                    character.CurrentInputs.Parkour = false;
                }
            });
            // character.CharacterAnimation.Movement.AddInputBufferAction(eCharacterActionInputType.PARKOUR, (value) =>
            // {
            //     if (value == false)
            //         character.CurrentInputs.Parkour = false;
            //     else
            //     {
            //         character.CurrentInputs.Jump = false;
            //         character.CurrentInputs.Grapple = false;
            //         character.CurrentInputs.Dash = false;
            //     }
            // });
        }

        public void AddEventLockState(ICheckEventLockState state)
        {
            _eventlockList.Remove(state);
            _eventlockList.Add(state);
        }

        public void RemoveEventLockState(ICheckEventLockState state)
        {
            _eventlockList.Remove(state);
        }

        public void UpdateEventLock(PlayerCharacterInputs inputs, CharacterAnimationState currentState)
        {
            _currentEventLockType = _externalEventLockType;
            _currentEventLockType |= _eventlockList.Select(e => (int)e.CurrentEventLockType).DefaultIfEmpty(0).Aggregate((acc, val) => acc | val);
            _currentEventLockType |= (int)currentState.CurrentEventLockType;

            int releaselock = 0;
            releaselock |= _eventlockList.Select(e => (int)e.ReleaseEventLockType).DefaultIfEmpty(0).Aggregate((acc, val) => acc | val);
            releaselock |= (int)currentState.ReleaseEventLockType;

            _currentEventLockType &= ~releaselock;

            UpdateCurrentEventLock(inputs);
        }

        private void UpdateCurrentEventLock(PlayerCharacterInputs inputs)
        {
            if (_currentEventLockType == 0)
                return;

            for (int i = 0; i < (int)eEventLockType.Max; ++i)
            {
                eEventLockType locktype = (eEventLockType)(1 << i);
                bool islock = IsEventLockType(locktype);
                if (islock == false)
                    continue;

                CheckEventLock(locktype, inputs);
            }
        }

        private void CheckEventLock(eEventLockType locktype, PlayerCharacterInputs inputs)
        {
            switch (locktype)
            {
                case eEventLockType.CharacterInputLock:
                    inputs.Move = Vector3.zero;
                    break;

                case eEventLockType.CharacterJump:
                    inputs.Jump = false;
                    break;

                case eEventLockType.CharacterDash:
                    inputs.Dash = false;
                    break;

                case eEventLockType.CharacterGraple:
                    inputs.Grapple = false;
                    break;

                case eEventLockType.CharacterParkour:
                    inputs.Parkour = false;
                    break;
            }
        }
    }
}
