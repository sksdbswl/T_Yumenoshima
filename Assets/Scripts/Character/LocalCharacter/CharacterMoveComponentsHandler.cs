using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using REIW.Animations.Character;
using REIW.EventLock;

namespace REIW
{
    public class CharacterMoveComponentsHandler : CacheMonoBehaviour
    {
        // 특수 이동 컴포넌트 리스트
        [SerializeField]
        private List<ICharacterMoveComponent> moveComponents = new();

        private CharacterMovePlayMode _oldMovePlayMode = CharacterMovePlayMode.None;
        protected List<ICharacterMoveComponent> CurrentMoveComponents
        {
            get
            {
                CharacterMovePlayMode mode = CurrentMovePlayMode;
                if (_oldMovePlayMode != mode)
                {
                    ForeachComponent((int)_oldMovePlayMode, (x) => x.ExitComponent());
                    AddMoveComponent((int)mode);
                    ForeachComponent((int)mode, (x) =>
                    {
                        x.EnterComponent();
                        x.EnterFromPreviousComponentType(_oldMovePlayMode);
                    });

                    _oldMovePlayMode = mode;

                    void ForeachComponent(int movetype, System.Action<ICharacterMoveComponent> action)
                    {
                        if (movetype == -1)
                            return;
                        
                        moveComponents.ForEach(x =>
                        {
                            if (((int)x.MoveType & movetype) != 0)
                                action(x);
                        });
                    }
                }

                return moveComponents.FindAll(x => ((int)x.MoveType & (int)mode) != 0);
            }
        }
        
        protected IEnumerable<IMoveComponentStateApplier> CheckCharacterStateMoveComponents => moveComponents.OfType<IMoveComponentStateApplier>();
        public IEnumerable<ICheckEventLockState> CheckEventLockMoveComponents => CurrentMoveComponents.OfType<ICheckEventLockState>();
        
        private CharacterMovePlayMode _currentMovePlayMode = CharacterMovePlayMode.Normal;
        public CharacterMovePlayMode CurrentMovePlayMode
        {
            get => _animationStateMachine?.CurrentState is IPlayModeState playModeState ? playModeState.MovePlayMode : _currentMovePlayMode;
//            set => _currentMovePlayMode = value;
        }

        public T GetMoveComponent<T>() where T : class, ICharacterMoveComponent
        {
            return moveComponents.Find(x => x is T) as T;
        }

        private ICharacterMoveComponent GetMoveComponent(CharacterMoveType movetype) => moveComponents.Find(x => ((int)x.MoveType & (int)movetype) != 0);

        private void AddMoveComponent(int playmodetype)
        {
            int type = (int)playmodetype;
            for (int i = 0; i < (int)CharacterMoveType.Max; ++i)
            {
                CharacterMoveType checktype = (CharacterMoveType)(1 << i);
                if ((type & (int)checktype) == 0)
                    continue;

                ICharacterMoveComponent moveComponent = GetMoveComponent(checktype);
                if (moveComponent != null)
                    continue;

                ICharacterMoveComponent component = CreateComponent(checktype);
                component.Initialize(_controller);
                moveComponents.Add(component);
            }
        }

        private ICharacterMoveComponent CreateComponent(CharacterMoveType movetype) => movetype switch
        {
            //CharacterMoveType.Grapple   => new CharacterMoveGrapple(),
            // CharacterMoveType.WallClimb => new CharacterMoveWallClimb(),
            CharacterMoveType.WallClimb => new CharacterMoveWallClimb(),
            CharacterMoveType.Gliding => new CharacterMoveGliding(),
            _                         => null,
        };

        private ICharacterMoveController _controller;
        private CharacterAnimationStateMachine _animationStateMachine;

        public void Initialize(ICharacterMoveController controller, CharacterAnimationStateMachine animationStateMachine)
        {
            _controller = controller;
            _animationStateMachine = animationStateMachine;

            moveComponents = GetComponents<ICharacterMoveComponent>().ToList();
            foreach (var comp in moveComponents)
            {
                comp.Initialize(controller);
            }

            CharacterMoveType[] array = EnumUtils.GetEnumValues<CharacterMoveType>();
            foreach (CharacterMoveType item in array)
            {
                AddMoveComponent((int)item);
            }
        }

        private void FixedUpdate()
        {
            foreach (var comp in CurrentMoveComponents)
            {
                comp.FixedUpdateComponent();
            }
        }
        
        private void LateUpdate()
        {
            foreach (var comp in CurrentMoveComponents)
            {
                comp.LateUpdateComponent();
            }
        }

        public void UpdateMoveComponents()
        {
            foreach (var comp in CheckCharacterStateMoveComponents)
            {
                if (comp.MoveComponentStateApply(_controller.CurrentInputs))
                    return;
            }
            
            foreach (var comp in CurrentMoveComponents)
            {
                comp.UpdateInput(_controller.CurrentInputs);
            }
        }

        public bool UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            bool fixRotation = false;
            foreach (var comp in CurrentMoveComponents)
            {
                fixRotation = comp.UpdateRotation(ref currentRotation, deltaTime);
                if (fixRotation)
                    break;
            }

            return fixRotation;
        }

        public bool UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            bool fix = false;
            foreach (var comp in CurrentMoveComponents)
            {
                fix = comp.UpdateVelocity(ref currentVelocity, deltaTime);
                if (fix)
                    break;
            }

            return fix;
        }

        private void OnDestroy()
        {
            foreach (var comp in moveComponents)
                comp.DestroyComponent();
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            foreach (var comp in moveComponents.OfType<ICharacterMoveComponentGizmo>())
                comp.OnDrawGizmos();
        }
#endif
    }
}

