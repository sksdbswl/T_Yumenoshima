using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace REIW
{
    public partial class InputController : SingletonBase<InputController>
    {
        public enum InputActionMapType { Player, UI, CustomizingUI, Fishing }

        public InputActionMapType CurrentActionMap
        {
            get => currentActionMap;
            set
            {
                if (currentActionMap == value)
                    return;

                // 이전 맵 Disable (단, UI는 항상 유지)
                if (previousActionMap != InputActionMapType.UI)
                {
                    var prevMap = PlayerInput.actions.FindActionMap(previousActionMap.ToString(), false);
                    prevMap?.Disable();
                }

                // 새 맵 Enable
                var newMap = PlayerInput.actions.FindActionMap(value.ToString(), false);
                newMap?.Enable();

                // UI & Console 맵은 항상 Enable (중복 호출해도 무방)
                var uiMap = PlayerInput.actions.FindActionMap("UI", false);
                uiMap?.Enable();
                var consoleMap = PlayerInput.actions.FindActionMap("DebugConsole", false);
                consoleMap?.Enable();
                var fishingMap = PlayerInput.actions.FindActionMap("Fishing", false);
                fishingMap?.Enable();

                previousActionMap = currentActionMap;
                currentActionMap = value;

                SetCursorState(!uiActionMaps.Contains(currentActionMap));
            }
        }

        public PlayerInput PlayerInput
        {
            get
            {
                if (!playerInput)
                {
                    this.playerInput = gameObject.AddComponent<UnityEngine.InputSystem.PlayerInput>();
                    //this.playerInput.actions = AssetManager.Singleton.LoadedInputActionAsset;
                }
                
                return playerInput;
            }
        }
        [SerializeField, ReadOnly] private PlayerInput playerInput;
        [SerializeField, ReadOnly] private InputActionMapType currentActionMap = InputActionMapType.Player;
        [SerializeField, ReadOnly] private InputActionMapType previousActionMap = InputActionMapType.Player;
        [SerializeField, ReadOnly] private bool cursorLocked = true;

#if UNITY_EDITOR
        [SerializeField] private bool isForceCursorUnlock = false; // Editor 전용 옵션 - Cursor 를 무조건 강제로 Unlock 상태로 유지
#endif

        public bool IsInitialized { get; private set; } = false;
        private readonly HashSet<InputActionMapType> uiActionMaps = new()
        {
            InputActionMapType.UI, InputActionMapType.CustomizingUI, InputActionMapType.Fishing
        };

        public event System.Action OnDebugConsole; // Keyboard: Left Ctrl + `[:~key] / Mobile : 5 Count Touch Tap
        public event System.Action OnDebugConsoleCommandUp; // Arrow Up Key
        public event System.Action OnDebugConsoleCommandDown; // Arrow Down Key
        
        private int lastHorizontalDir = 0;
        private int lastVerticalDir = 0;
        
        public void Initialize()
        {
            if (IsInitialized)
                return;

            var debugConsoleMap = this.PlayerInput.actions.FindActionMap("DebugConsole");
            debugConsoleMap.FindAction("ConsoleOnOff").performed += OnDebugConsoleOnOffCallback;
            debugConsoleMap.FindAction("PreviousCommand").performed += OnDebugConsoleUp;
            debugConsoleMap.FindAction("NextCommand").performed += OnDebugConsoleDown;

            this.PlayerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            this.PlayerInput.uiInputModule =
                EventSystem.current
                    ? EventSystem.current.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>()
                    : null;

            // Custom C# Binding ActionMap
            InitializeBinding();

            // 1) 안전하게 필요한 맵들을 직접 Enable
            var player = PlayerInput.actions.FindActionMap("Player", false);
            var ui = PlayerInput.actions.FindActionMap("UI", false);
            var console = PlayerInput.actions.FindActionMap("DebugConsole", false);
            var fishing = PlayerInput.actions.FindActionMap("Fishing", false);
            player?.Enable();
            ui?.Enable();
            console?.Enable();
            fishing?.Enable();
            
            // 2) 커서 상태 포함한 내부 상태 갱신을 위해 CurrentActionMap 흐름도 한번 태우기
            previousActionMap = InputActionMapType.UI; // UI는 항상 유지하려는 의도에 맞춤
            CurrentActionMap = InputActionMapType.Player;
            
            IsInitialized = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(!uiActionMaps.Contains(currentActionMap));
        }

        private void SetCursorState(bool isLocked)
        {
            cursorLocked =  isLocked;
            
#if UNITY_EDITOR
            if (isForceCursorUnlock) cursorLocked = false;
#endif
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }

        private void RefreshCursorState()
        {
            bool isNeedCursor = (uiActionMaps.Contains(currentActionMap));
            SetCursorState(!isNeedCursor);
        }
        
        private void ProcessPerformMoveAction(InputAction.CallbackContext context)
        {
            if (context.control.device is Keyboard)
            {
                var hDir = context.ReadValue<Vector2>().x;
                var vDir = context.ReadValue<Vector2>().y;
                if (Keyboard.current.aKey.isPressed && Keyboard.current.dKey.isPressed)
                {
                    hDir = lastHorizontalDir;
                }
                else
                {
                    if (hDir != 0)
                        lastHorizontalDir = hDir < 0 ? -1 : +1;
                }

                if (Keyboard.current.wKey.isPressed && Keyboard.current.sKey.isPressed)
                {
                    vDir = lastVerticalDir;
                }
                else if (Mathf.Abs(vDir) > 0.1f)
                {
                    if (vDir != 0)
                        lastVerticalDir = vDir < 0 ? -1 : +1;
                }

                Move = new Vector2(hDir, vDir);
            }
            else
            {
                Move = context.ReadValue<Vector2>();
            }
        }

        void OnDebugConsoleOnOffCallback(InputAction.CallbackContext context)
        {
            OnDebugConsole?.Invoke();
        }

        void OnDebugConsoleUp(InputAction.CallbackContext context)
        {
            OnDebugConsoleCommandUp?.Invoke();
        }

        void OnDebugConsoleDown(InputAction.CallbackContext context)
        {
            OnDebugConsoleCommandDown?.Invoke();
        }
    }
}