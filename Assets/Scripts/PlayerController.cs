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

    private PlayerInputHandler inputHandler;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        cameraController = GetComponent<CameraController>();
    }

    private void Update()
    {
        HandleMovement();
        // Handle camera rotation
        cameraController.HandleCameraRotation(inputHandler.LookInput);
    }

    private void HandleMovement()
    {
        // Calculate movement direction vectors
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Determine current speed based on input and movement state
        float currentSpeedX = canMove ? (inputHandler.IsSprintingPressed ? sprintSpeed : walkSpeed) * inputHandler.MoveInput.y : 0;
        float currentSpeedY = canMove ? (inputHandler.IsSprintingPressed ? sprintSpeed : walkSpeed) * inputHandler.MoveInput.x : 0;

        // Preserve the current vertical velocity
        float verticalVelocity = moveVelocity.y;

        // Calculate horizontal movement velocity only if grounded or climbing
        if (characterController.isGrounded || isClimbing)
        {
            moveVelocity = (forward * currentSpeedX) + (right * currentSpeedY);
        }

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

        // Apply gravity if not grounded
        if (!characterController.isGrounded)
        {
            moveVelocity.y = verticalVelocity - gravity * Time.deltaTime;
        }
        else if (jumpCount > 0)
        {
            moveVelocity.y = 0;
            jumpCount = 0;
        }


        // Handle jumping
        if (inputHandler.IsJumpingPressed && (characterController.isGrounded || jumpCount < maxJumps))
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
        characterController.Move(moveVelocity * Time.deltaTime);

        ResetSingleActionInputs();
    }

    public void ResetSingleActionInputs()
    {
        inputHandler.IsJumpingPressed = false;
        inputHandler.IsDashingPressed = false;
        // Add any other single-use inputs here
    }
}
