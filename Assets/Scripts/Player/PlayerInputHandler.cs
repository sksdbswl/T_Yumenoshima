using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] public InputActionReference moveAction;
    [SerializeField] public InputActionReference jumpAction;
    [SerializeField] public InputActionReference interactAction;
    [SerializeField] public InputActionReference cancelAction;

    public Vector2 MoveInput { get; private set; }

    public Action OnJump;
    public Action OnInteract;
    public Action OnCancel;

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        interactAction.action.Enable();
        cancelAction.action.Enable();

        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled  += OnMoveCanceled;
        jumpAction.action.performed += OnJumpPerformed;
        interactAction.action.performed += OnInteractPerformed;
        cancelAction.action.performed   += OnCancelPerformed;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMovePerformed;
        moveAction.action.canceled  -= OnMoveCanceled;
        jumpAction.action.performed -= OnJumpPerformed;
        interactAction.action.performed -= OnInteractPerformed;
        cancelAction.action.performed   -= OnCancelPerformed;

        moveAction.action.Disable();
        jumpAction.action.Disable();
        interactAction.action.Disable();
        cancelAction.action.Disable();
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
}
