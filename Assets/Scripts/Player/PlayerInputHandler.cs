using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] public InputActionReference moveAction; // WASD
    [SerializeField] public InputActionReference jumpAction; // Space
    [SerializeField] public InputActionReference interactAction; // E
    [SerializeField] public InputActionReference cancelAction; // Escape
    [SerializeField] public InputActionReference tabAction; // Tab
    [SerializeField] public InputActionReference freeLookAction; // Tab

    public Vector2 MoveInput { get; private set; }

    public Action OnJump;
    public Action OnInteract;
    public Action OnCancel;
    public Action OnTab;
    public Action OnFreeLook; 
    
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        interactAction.action.Enable();
        cancelAction.action.Enable();
        tabAction.action.Enable(); 
        freeLookAction.action.Enable(); 
        
        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled  += OnMoveCanceled;
        jumpAction.action.performed += OnJumpPerformed;
        interactAction.action.performed += OnInteractPerformed;
        cancelAction.action.performed   += OnCancelPerformed;
        tabAction.action.performed += OnFreeLookActivePerformed;  
        freeLookAction.action.performed += OnFreeLookPerformed;  
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMovePerformed;
        moveAction.action.canceled  -= OnMoveCanceled;
        jumpAction.action.performed -= OnJumpPerformed;
        interactAction.action.performed -= OnInteractPerformed;
        cancelAction.action.performed   -= OnCancelPerformed;
        tabAction.action.performed   -= OnFreeLookActivePerformed;
        freeLookAction.action.performed -= OnFreeLookPerformed;  
        
        moveAction.action.Disable();
        jumpAction.action.Disable();
        interactAction.action.Disable();
        cancelAction.action.Disable();
        tabAction.action.Disable();
        freeLookAction.action.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
        => MoveInput = ctx.ReadValue<Vector2>();

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
        => MoveInput = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
        => OnJump?.Invoke();

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
        => OnInteract?.Invoke();

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
        => OnCancel?.Invoke();
    
    private void OnFreeLookActivePerformed(InputAction.CallbackContext ctx) 
        => OnTab?.Invoke();
    
    private void OnFreeLookPerformed(InputAction.CallbackContext ctx) 
        => OnFreeLook?.Invoke();
}
