using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace REIW
{
    public partial class InputController
    {
        [Header("Joystick Control Settings")]
        [field: SerializeField]
        public bool IsInputByJoystick { get; private set; } = false;
        
        // [Player] Action Map Event
        public Vector2 Move { get; private set; } = Vector2.zero;
        public Vector2 Look { get; private set; } = Vector2.zero;
        public float ScrollWheel { get; private set; } = 0f; // Scroll stop: 0, Scroll Down: -1, Scroll Up: 1
        public event System.Action<float> OnPlayerScrollWheelChanged;

        public event System.Action OnEscape; // Escape Key[:ESC Key]
        public event System.Action OnJump; // Space Key
        public event System.Action OnJumpDown; // Space Key
        public event System.Action OnJumpUp;
        public event System.Action OnParkour; // Space Key
        public event System.Action OnDownLMC; // Mouse Left Click Down
        public event System.Action OnUpLMC; // Mouse Left Click Up
        public event System.Action OnDownRMC; // Mouse Right Click Down
        public event System.Action OnDownLeftShift; // Left Shift Down
        public event System.Action OnUpLeftShift; // Left Shift Up
        public event System.Action OnUpRMC; // Mouse Right Click Up
        public event System.Action OnToggleWalk; // Left Ctrl
        public event System.Action OnMount; // V Key
        public event System.Action OnSkillActionE; // E Key
        public event System.Action OnSkillActionT; // T Key
        public event System.Action OnInteract; // F Key
        public event System.Action OnCameraSwitch; // Tab Key
        public event System.Action OnEnter; // Enter Key
        public event System.Action OnCharacterStageEnter; // C Key
        public event System.Action OnInventoryOpen; // i Key


        // [UI] Action Map Event
        public float ScrollWheelForUI { get; private set; } = 0f;
        public event System.Action<float> OnUIScrollWheelChanged;
        
        // [CustomizingUI] Action Map Event
        public event System.Action<bool> OnCustomizingUIClick;
        public event System.Action<float> OnCustomizingUIScrollWheelChanged; 

        // [Fishing] Action Map Event
        public event System.Action<Vector2> OnFishingMove;
        public event System.Action<Vector3> OnFishingDeviceRotate;
        public event System.Action<Vector3> OnFishingDeviceTilt;
        
        //private PlayerControlHUD playerControlHUD;
        private Joystick attachedJoystickMove;
        private Joystick attachedJoystickLook;
        private RectTransform joystickAreaMove;
        private RectTransform joystickAreaLook;
        private InputAction moveAction;
        private InputAction lookAction;
        private float lookSensitivity = 1f;
        [SerializeField] private float lookJoystickCoefficient = 15f;

        public void InitializeBinding()
        {
            // Input Controller가 파괴되는 상황이면, System 이 종료 되는 경우 밖에 없으므로
            // 구독한 Binding 을 별도로 해제하지 않아도 됩니다.
            
            var playerMap = this.PlayerInput.actions.FindActionMap("Player");
            moveAction = playerMap["Move"];
            lookAction = playerMap["Look"];

            playerMap["Move"].performed += OnPerformMove;
            playerMap["Move"].started += OnPerformMove;
            playerMap["Move"].canceled += OnPerformMove;
            playerMap["LMC"].performed += OnPerformLMC;
            playerMap["LMC"].canceled += OnCancelLMC;
            playerMap["RMC"].performed += OnPerformRMC;
            playerMap["RMC"].canceled += OnCancelRMC;
            playerMap["LeftShift"].performed += OnPerformLeftShift;
            playerMap["LeftShift"].canceled += OnCancelLeftShift;
            playerMap["Jump"].performed += OnPerformJump;
            playerMap["Jump"].started += OnStartJump;
            playerMap["Jump"].canceled += OnCancelJump;
            playerMap["Parkour"].performed += OnPerformParkour;
            playerMap["ToggleWalk"].performed += OnPerformToggleWalk;
            playerMap["Mount"].performed += OnPerformMount;
            playerMap["SkillActionE"].performed += OnPerformActionE;
            playerMap["SkillActionT"].performed += OnPerformActionT;
            playerMap["Cursor"].performed += OnPerformCursor;
            playerMap["Cursor"].canceled += OnCanceledCursor;
            playerMap["ScrollWheel"].performed += OnPerformPlayerScrollWheel;
            playerMap["ScrollWheel"].canceled += OnCanceledPlayerScrollWheel;
            playerMap["Interact"].performed += OnPerformInteract;
            playerMap["CameraSwitch"].performed += OnPerformCameraSwitch;
            playerMap["CharacterStageEnter"].performed += OnPerformCharacterStageEnter;
            playerMap["InventoryOpen"].performed += OnPerformInventoryOpen; 

            var uiMap = this.PlayerInput.actions.FindActionMap("UI");
            uiMap["Cancel"].performed += OnEscapeCallback;
            uiMap["ScrollWheel"].performed += OnPerformUIScrollWheel;
            uiMap["ScrollWheel"].canceled += OnCanceledUIScrollWheel;
            uiMap["Enter"].performed += OnPerformEnterUIClick;

            var customizingUiMap = this.PlayerInput.actions.FindActionMap("CustomizingUI");
            customizingUiMap["Click"].performed += OnPerformCustomizingUIClick;
            customizingUiMap["Click"].canceled += OnPerformCustomizingUIClick;
            customizingUiMap["ScrollWheel"].performed += OnPerformCustomizingUIScrollWheel;

            var fishingMap = this.PlayerInput.actions.FindActionMap("Fishing");
            fishingMap["Move"].performed += OnPerformFishingMove;
            fishingMap["Move"].canceled += OnPerformFishingMove;
            fishingMap["DeviceRotate"].performed += OnPerformFishingDeviceRotate;
            fishingMap["DeviceTilt"].performed += OnPerformFishingDeviceTilt;
            
#if !UNITY_EDITOR && UNITY_ANDROID || UNITY_IOS
            IsInputByJoystick = true;
#endif
        }

        void OnEnable() => EnhancedTouchSupport.Enable();
        void OnDisable() => EnhancedTouchSupport.Disable();

        void Update()
        {
            // Cursor가 보여지는 상태이면, Move/Look Input 명령을 수행하지 않는다.
            // if (!cursorLocked && !IsInputByJoystick)
            // {
            //     Move = Vector2.zero;
            //     Look = Vector2.zero;
            //     return;
            // }
            //
            // if (IsInputByJoystick && attachedJoystickMove && attachedJoystickLook)
            // {
            //     var touches = Touch.activeTouches;
            //     if (touches.Count == 0)
            //         return;
            //
            //     // Movement Joystick 영역 제외한 유효 터치 추출
            //     var validTouches = new System.Collections.Generic.List<Touch>();
            //     foreach (var t in touches)
            //     {
            //         if (joystickAreaMove == null ||
            //             !RectTransformUtility.RectangleContainsScreenPoint(joystickAreaMove, t.screenPosition, null))
            //         {
            //             validTouches.Add(t);
            //         }
            //     }
            //
            //     if (validTouches.Count == 2) // 카메라 줌 (두 손가락 핀치)
            //     {
            //         var t0 = validTouches[0];
            //         var t1 = validTouches[1];
            //
            //         Vector2 prevPos0 = t0.screenPosition - t0.delta;
            //         Vector2 prevPos1 = t1.screenPosition - t1.delta;
            //
            //         float prevDist = Vector2.Distance(prevPos0, prevPos1);
            //         float currDist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            //         ScrollWheel = currDist - prevDist;
            //
            //         Move = Look = Vector2.zero;
            //     }
            //     else
            //     {
            //         Move = new Vector2(attachedJoystickMove.Horizontal, attachedJoystickMove.Vertical);
            //         Look = lookJoystickCoefficient * lookSensitivity * new Vector2(attachedJoystickLook.Horizontal, attachedJoystickLook.Vertical);
            //     }
            //
            //     // Look = validTouches.Count == 1 ? validTouches[0].delta : Vector2.zero;
            // }
            // else
            // {
            //     if (moveAction != null)
            //     {
            //         attachedJoystickMove?.SetInputWithoutNotify(Move);
            //     }
            //
            //     if (lookAction != null)
            //     {
            //         Look = lookAction.ReadValue<Vector2>() * lookSensitivity;
            //         attachedJoystickLook?.SetInputWithoutNotify(Look);
            //     }
            // }
        }

        // public void AttachPlayerHUD(PlayerControlHUD hud)
        // {
        //     this.playerControlHUD = hud;
        //     this.attachedJoystickMove = hud.JoystickMove;
        //     this.attachedJoystickLook = hud.JoystickLook;
        //     this.joystickAreaMove = hud.JoystickMove.BaseRect;
        //     this.joystickAreaLook = hud.JoystickLook.BaseRect;
        // }
        //
        // public void DetachPlayerHUD(PlayerControlHUD hud)
        // {
        //     this.playerControlHUD = null;
        //     this.attachedJoystickMove = null;
        //     this.joystickAreaMove = null;
        //     this.joystickAreaLook = null;
        // }
        
        public void SetLookSensitivity(float sensitivity)
        {
            lookSensitivity = sensitivity;
        }

        void OnPerformMove(InputAction.CallbackContext context)
        {
            ProcessPerformMoveAction(context);
        }

        void OnPerformLMC(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnDownLMC?.Invoke();
        }

        void OnCancelLMC(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnUpLMC?.Invoke();
        }

        void OnPerformRMC(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnDownRMC?.Invoke();
        }

        void OnCancelRMC(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnUpRMC?.Invoke();
        }
        
        void OnPerformLeftShift(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;

            OnDownLeftShift?.Invoke();
        }

        void OnCancelLeftShift(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;

            OnUpLeftShift?.Invoke();
        }

        void OnPerformJump(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnJump?.Invoke();
        }

        void OnStartJump(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player)
                return;
            
            OnJumpDown?.Invoke();
        }

        void OnCancelJump(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player)
                return;

            OnJumpUp?.Invoke();
        }

        void OnPerformParkour(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player)
                return;

            OnParkour?.Invoke();
        }

        void OnPerformMount(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnMount?.Invoke();
        }

        void OnPerformActionE(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnSkillActionE?.Invoke();
        }

        void OnPerformActionT(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player)
                return;
            
            OnSkillActionT?.Invoke();
        }

        void OnPerformToggleWalk(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnToggleWalk?.Invoke();
        }

        void OnPerformCursor(InputAction.CallbackContext context)
        {
            SetCursorState(false);
        }

        void OnCanceledCursor(InputAction.CallbackContext context)
        {
            RefreshCursorState();
        }

        void OnPerformPlayerScrollWheel(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            ScrollWheel = context.ReadValue<Vector2>().y;
            OnPlayerScrollWheelChanged?.Invoke(ScrollWheel);
        }

        void OnCanceledPlayerScrollWheel(InputAction.CallbackContext context)
        {
            ScrollWheel = 0f;
        }

        void OnPerformInteract(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnInteract?.Invoke();
        }

        void OnPerformCameraSwitch(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnCameraSwitch?.Invoke();
        }
        
        private void OnPerformCharacterStageEnter(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnCharacterStageEnter?.Invoke();
        }
        
        private void OnPerformInventoryOpen(InputAction.CallbackContext context)
        {
            if (currentActionMap != InputActionMapType.Player) 
                return;
            
            OnInventoryOpen?.Invoke();
        }
        
        private void OnPerformEnterUIClick(InputAction.CallbackContext obj)
        {
            OnEnter?.Invoke();
        }

        public void ForceNotifyAttack()
        {
            OnUpLMC?.Invoke();
        }

        public void ForceNotifyDash()
        {
            OnDownRMC?.Invoke();
        }

        public void ForceNotifyJump()
        {
            OnJump?.Invoke();
        }

        public void ForceNotifyParkour()
        {
            OnParkour?.Invoke();
        }

        public void ForceNotifyMount()
        {
            OnMount?.Invoke();
        }

        public void ForceNotifySpecialActionE()
        {
            OnSkillActionE?.Invoke();
        }

        public void ForceNotifySpecialActionT()
        {
            OnSkillActionT?.Invoke();
        }

        public void ForceNotifyCameraSwitch()
        {
            OnCameraSwitch?.Invoke();
        }

        public void ForceNotifyOllie()
        {
            OnSkillActionE?.Invoke();
        }

        void OnEscapeCallback(InputAction.CallbackContext context)
        {
            OnEscape?.Invoke();
        }

        void OnPerformUIScrollWheel(InputAction.CallbackContext context)
        {
            ScrollWheelForUI = context.ReadValue<Vector2>().y;
            OnUIScrollWheelChanged?.Invoke(ScrollWheelForUI);
        }

        void OnCanceledUIScrollWheel(InputAction.CallbackContext context)
        {
            ScrollWheelForUI = 0f;
        }
        
        void OnPerformCustomizingUIClick(InputAction.CallbackContext context)
        {
            OnCustomizingUIClick?.Invoke(context.ReadValueAsButton());
        }

        void OnPerformCustomizingUIScrollWheel(InputAction.CallbackContext context)
        {
            OnCustomizingUIScrollWheelChanged?.Invoke(context.ReadValue<Vector2>().y);
        }

        void OnPerformFishingMove(InputAction.CallbackContext context)
        {
            OnFishingMove?.Invoke(context.ReadValue<Vector2>());
        }
        
        void OnPerformFishingDeviceRotate(InputAction.CallbackContext context)
        {
            OnFishingDeviceRotate?.Invoke(context.ReadValue<Vector3>());
        }
        
        void OnPerformFishingDeviceTilt(InputAction.CallbackContext context)
        {
            OnFishingDeviceTilt?.Invoke(context.ReadValue<Vector3>());
        }
    }
}