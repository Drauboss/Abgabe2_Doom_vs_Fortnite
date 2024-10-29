using System;
using System.Collections;
using UnityEngine;

public class PlayerControllerRB : MonoBehaviour
{
    [Header("Parameters")]
    public float walkSpeed;
    public float sprintSpeed;
    public float swingSpeed;
    public float groundDrag;
    public float jumpForce;
    public int maxJumps;
    public float airMultiplier;
    public float playerHeight;
    public LayerMask whatIsGround;
    public Transform orientation;
    public float dashSpeed;
    public float jumpCooldown;
    public float dashSpeedChangeFactor;

    [Header("Debug Values")]
    public float moveSpeed;
    public bool isSprinting;
    public int jumpCount;
    public bool grounded;
    public float rightDirection;
    public float forwardDirection;
    public bool dashing;
    public bool readyToDash;
    public Vector3 moveDirection;
    public Vector3 rbVelocity;
    public bool activeGrapple;
    public bool swinging;
    public bool isClimbing;

    private Rigidbody rb;
    private PlayerInputHandlerRB inputHandler;
    private bool readyToJump;
    private bool enableMovementOnNextTouch;
    private Vector3 velocityToSet;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private float speedChangeFactor;

    public enum MovementState
    {
        walking,
        sprinting,
        dashing,
        air,
        climbing,
        grappling,
        swinging
    }

    public MovementState state;

    /// <summary>
    /// Initializes the player controller.
    /// </summary>
    private void Start()
    {
        inputHandler = GetComponent<PlayerInputHandlerRB>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        readyToDash = true;
        jumpCount = 0;
    }

    /// <summary>
    /// Updates the player controller every frame.
    /// </summary>
    private void Update()
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // Handle drag
        if (grounded && !activeGrapple)
        {
            jumpCount = 0;
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        // For debugging purposes
        rbVelocity = rb.velocity;

        ResetSingleActionInputs();
    }

    /// <summary>
    /// Updates the player controller at fixed intervals.
    /// </summary>
    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Handles player input.
    /// </summary>
    private void MyInput()
    {
        forwardDirection = inputHandler.MoveInput.y;
        rightDirection = inputHandler.MoveInput.x;

        // Handle jumping
        if (inputHandler.IsJumpingPressed && readyToJump && (grounded || jumpCount < maxJumps - 1))
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Handle sprinting
        if (inputHandler.IsSprintingPressed && grounded)
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

    /// <summary>
    /// Sets the move speed based on whether the player is sprinting.
    /// </summary>
    private void SetMoveSpeed()
    {
        if (isSprinting)
        {
            desiredMoveSpeed = sprintSpeed;
        }
        else
        {
            desiredMoveSpeed = walkSpeed;
        }
    }

    /// <summary>
    /// Moves the player based on input and state.
    /// </summary>
    private void MovePlayer()
    {
        if (activeGrapple || swinging) return;

        // Calculate movement direction
        moveDirection = orientation.forward * forwardDirection + orientation.right * rightDirection;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // On ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // In air
        else if (!grounded && !isClimbing)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        // Climbing
        else if (isClimbing)
        {
            jumpCount = 0;
            rb.AddForce(orientation.forward * 100f, ForceMode.Force);

            if (inputHandler.MoveInput.y == 0)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
                rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }
            else
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    /// <summary>
    /// Controls the player's speed to ensure it does not exceed the desired move speed.
    /// </summary>
    private void SpeedControl()
    {
        if (activeGrapple) return;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    /// <summary>
    /// Makes the player jump.
    /// </summary>
    private void Jump()
    {
        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        jumpCount++;
    }

    /// <summary>
    /// Resets the jump state after a cooldown.
    /// </summary>
    private void ResetJump()
    {
        readyToJump = true;
    }

    /// <summary>
    /// Resets single-action inputs like jumping.
    /// </summary>
    public void ResetSingleActionInputs()
    {
        inputHandler.IsJumpingPressed = false;
        // Add any other single-use inputs here
    }

    /// <summary>
    /// Handles the player's movement state.
    /// </summary>
    private void StateHandler()
    {
        // Mode - Dashing
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        // Mode - Grappling
        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }
        // Mode - Sprinting
        else if (grounded && inputHandler.IsSprintingPressed)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - Climbing
        else if (isClimbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = walkSpeed;
        }
        // Mode - Swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }
        // Mode - Air
        else if (!grounded)
        {
            state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    /// <summary>
    /// Smoothly lerps the move speed to the desired value.
    /// </summary>
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // Smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    /// <summary>
    /// Makes the player jump to a specified position.
    /// </summary>
    /// <param name="targetPosition">The target position to jump to.</param>
    /// <param name="trajectoryHeight">The height of the jump trajectory.</param>
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        bool isBelow = targetPosition.y < transform.position.y;

        velocityToSet = isBelow ? Vector3.zero : CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    /// <summary>
    /// Sets the player's velocity.
    /// </summary>
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    /// <summary>
    /// Resets movement restrictions after grappling.
    /// </summary>
    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    /// <summary>
    /// Handles collision events.
    /// </summary>
    /// <param name="collision">The collision event data.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<GrapplingRB>().StopGrapple();
        }
    }

    /// <summary>
    /// Calculates the initial velocity required to jump from a start point to an endpoint with a specified trajectory height.
    /// </summary>
    /// <param name="startPoint">The starting position of the jump.</param>
    /// <param name="endPoint">The target position of the jump.</param>
    /// <param name="trajectoryHeight">The peak height of the jump trajectory.</param>
    /// <returns>The initial velocity required to perform the jump.</returns>
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