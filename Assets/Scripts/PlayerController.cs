using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public bool canMove = true;
    public bool isSprinting = false;

    [Header("Movement Params")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float jumpPower = 5f;
    private int jumpCount = 0;
    private int maxJumps = 2;
    public float gravity = 9.81f;
    private bool isDashing = false;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    private float dashTimer = 0f;

    public bool isClimbing = false;

    [Header("Camera Params")]
    public Camera fpsCamera;
    public float lookSpeed = 0.5f;
    public float lookLimit = 45.0f;
    public float dashFov;

    // [Header("Weapons")]
    // public GameObject GravityGun;
    // public GameObject ObstacleGun;

    public CharacterController characterController;
    public Vector3 moveVelocity;
    public Vector2 moveInput;
    private Vector2 lookInput;
    private float rotation;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // // Weapons
        // GravityGun.SetActive(true);
        // ObstacleGun.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float currentSpeedX = canMove ? (isSprinting ? sprintSpeed : walkSpeed) * moveInput.y : 0;
        float currentSpeedY = canMove ? (isSprinting ? sprintSpeed : walkSpeed) * moveInput.x : 0;

        float movementVelocity = moveVelocity.y;
        moveVelocity = (forward * currentSpeedX) + (right * currentSpeedY);

        // Handle dashing
        if (isDashing)
        {
            DoFov(dashFov);
            moveVelocity += forward * dashSpeed;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                DoFov(60);
            }
        }

        // Jump

        moveVelocity.y = movementVelocity;
        if (!characterController.isGrounded && !isClimbing)
        {
            moveVelocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            jumpCount = 0;
        }



        characterController.Move(moveVelocity * Time.deltaTime);

        // Camera
        if (canMove)
        {
            rotation += -lookInput.y * lookSpeed;
            rotation = Mathf.Clamp(rotation, -lookLimit, lookLimit);

            fpsCamera.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
        }
    }

    public void DoFov(float fov)
    {
        fpsCamera.DOFieldOfView(fov, 0.25f);
    }

    public void HandleMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void HandleSprintInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }

    public void HandleLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed && canMove && (characterController.isGrounded || jumpCount < maxJumps - 1))
        {
            moveVelocity.y = jumpPower;
            jumpCount++;
        }
    }

    public void HandleDashInput(InputAction.CallbackContext context)
    {
        if (context.performed && canMove && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }
    }
    // public void HandleWeaponSwitch(InputAction.CallbackContext context)
    // {
    //     if (context.started)
    //     {
    //         GravityGun.SetActive(!GravityGun.activeSelf);
    //         ObstacleGun.SetActive(!ObstacleGun.activeSelf);
    //     }
    // }


}
