using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("References")]
    [SerializeField] public CharacterController characterController;
    [SerializeField] private CameraController cameraController;

    private int jumpCount = 0;
    private bool isDashing = false;
    private float dashTimer = 0f;
    public bool isClimbing = false;
    public Vector3 moveVelocity;
    public bool freeze = false;
    public bool activeGrapple;
    private bool enableMovementOnNextTouch;

    private PlayerInputHandler inputHandler;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        cameraController = GetComponent<CameraController>();
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!activeGrapple)
        {
            HandleMovement();
        }
        else
        {
            HandleGrappleMovement();
        }

        // Handle camera rotation
        cameraController.HandleCameraRotation(inputHandler.LookInput);
    }

    private void HandleMovement()
    {
        // Calculate movement direction vectors
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Determine current speed based on input and movement state
        float forwardSpeed = canMove ? (inputHandler.IsSprintingPressed ? sprintSpeed : walkSpeed) * inputHandler.MoveInput.y : 0;
        float rightSpeed = canMove ? (inputHandler.IsSprintingPressed ? sprintSpeed : walkSpeed) * inputHandler.MoveInput.x : 0;

        // Preserve the current vertical velocity
        float verticalVelocity = moveVelocity.y;

        moveVelocity = (forward * forwardSpeed) + (right * rightSpeed);

        // Handle dashing
        if (isDashing)
        {
            // Change FOV for dashing effect
            cameraController.DoFov();

            // Add dash speed to forward movement
            moveVelocity += forward * dashSpeed;

            // Decrease dash timer
            dashTimer -= Time.deltaTime;

            // Stop dashing when timer runs out
            if (dashTimer <= 0)
            {
                isDashing = false;
                cameraController.ResetFov();
            }
        }

        // Apply gravity if not grounded and not climbing
        if (!characterController.isGrounded && !isClimbing)
        {
            moveVelocity.y = verticalVelocity - gravity * Time.deltaTime;
        }
        else if (jumpCount > 0)
        {
            moveVelocity.y = 0;
            jumpCount = 0;
        }

        // Handle jumping
        if (inputHandler.IsJumpingPressed && (isClimbing || (jumpCount < maxJumps)))
        {
            moveVelocity.y = jumpPower;
            jumpCount++;
        }

        // Handle dashing input
        if (inputHandler.IsDashingPressed && !isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
        }

        // Move the character controller
        if (freeze)
        {
            moveVelocity = Vector3.zero;
        }

        characterController.Move(moveVelocity * Time.deltaTime);

        ResetSingleActionInputs();
    }

    private void HandleGrappleMovement()
    {
        // Move the character controller towards the target position
        characterController.Move(velocityToSet * Time.deltaTime);

        // Check if the character has reached the target position
        if (Vector3.Distance(transform.position, grappleTargetPosition) < 1f)
        {
            activeGrapple = false;
            cameraController.ResetFov();
        }
    }

    public void ResetSingleActionInputs()
    {
        inputHandler.IsJumpingPressed = false;
        inputHandler.IsDashingPressed = false;
        // Add any other single-use inputs here
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        grappleTargetPosition = targetPosition;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        cameraController.DoFov();
    }

    private Vector3 velocityToSet;
    private Vector3 grappleTargetPosition;

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}