using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsSprintingPressed { get; set; }
    public bool IsJumpingPressed { get; set; }
    public bool IsDashingPressed { get; set; }
    public bool IsClimbingPressed { get; set; }

    public void HandleMoveInput(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void HandleSprintInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsSprintingPressed = true;
        }
        else if (context.canceled)
        {
            IsSprintingPressed = false;
        }
    }

    public void HandleLookInput(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsJumpingPressed = true;
        }
        else if (context.canceled)
        {
            IsJumpingPressed = false;
        }
    }

    public void HandleDashInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsDashingPressed = true;
        }
        else if (context.canceled)
        {
            IsDashingPressed = false;
        }
    }

    public void HandleClimbInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsClimbingPressed = true;
        }
        else if (context.canceled)
        {
            IsClimbingPressed = false;
        }
    }
}