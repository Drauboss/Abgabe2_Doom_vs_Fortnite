using UnityEngine;

public class PlayerControllerRB : MonoBehaviour
{

    [Header("Parameters")]
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpForce;
    public int maxJumps;
    public float airMultiplier;
    public float playerHeight;
    public LayerMask whatIsGround;
    public Transform orientation;
    public float dashSpeed;
    public float dashDuration;
    public float jumpCooldown;

    [Header("CameraEffects")]
    public CameraControllerRB cam;
    public float dashFov;

    [Header("Debug Values")]
    public float moveSpeed;
    public bool isSprinting;
    public int jumpCount;
    bool readyToJump;
    public bool grounded;
    public float rightDirection;
    public float forwardDirection;
    public bool dashing;
    public bool readyToDash;
    public Vector3 moveDirection;
    Rigidbody rb;


    private PlayerInputHandlerRB inputHandler;

    private void Start()
    {
        inputHandler = GetComponent<PlayerInputHandlerRB>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        readyToDash = true;
        jumpCount = 0;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();

        SpeedControl();

        // handle drag
        if (grounded)
        {
            jumpCount = 0;
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }


        ResetSingleActionInputs();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        forwardDirection = inputHandler.MoveInput.y;
        rightDirection = inputHandler.MoveInput.x;

        // when to jump
        if (inputHandler.IsJumpingPressed && readyToJump && (grounded || jumpCount < maxJumps - 1))
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (inputHandler.IsDashingPressed && !isSprinting && readyToDash)
        {
            dashing = true;
            readyToDash = false;
            Dash();
            Invoke(nameof(ResetDash), dashDuration);
        }

        // Check if sprinting
        if (inputHandler.IsSprintingPressed && grounded && !inputHandler.IsDashingPressed)
        {
            isSprinting = true;
        }
        else if (grounded)
        {
            isSprinting = false;
        }

        // Set move speed based on sprinting flag
        SetMoveSpeed();
    }

    private void SetMoveSpeed()
    {
        if (isSprinting)
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = walkSpeed;
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * forwardDirection + orientation.right * rightDirection;

        // on ground
        if (grounded)
        {
            // rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            Vector3 velocity = moveDirection * moveSpeed;
            // rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        }

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        jumpCount++;
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void Dash()
    {
        cam.DoFov();
        rb.AddForce(orientation.forward * dashSpeed, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        cam.ResetFov();
        readyToDash = true;
    }

    public void ResetSingleActionInputs()
    {
        inputHandler.IsJumpingPressed = false;
        inputHandler.IsDashingPressed = false;
        // Add any other single-use inputs here
    }

}