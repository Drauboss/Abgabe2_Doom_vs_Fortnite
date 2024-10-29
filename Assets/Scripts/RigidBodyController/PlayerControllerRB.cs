using System;
using System.Collections;
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
    public float jumpCooldown;

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
    public Vector3 rbVelocity;

    public bool activeGrapple;

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
        StateHandler();

        // handle drag
        if (grounded && !activeGrapple)
        {
            jumpCount = 0;
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }


        rbVelocity = rb.velocity;
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

        // Check if sprinting
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

    private void MovePlayer()
    {
        if (activeGrapple) return;
        // calculate movement direction
        moveDirection = orientation.forward * forwardDirection + orientation.right * rightDirection;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        // on ground
        if (grounded)
        {
            // rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            Vector3 velocity = moveDirection * moveSpeed;
            // rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        }

        // in air
        else if (!grounded && !isClimbing)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        else if (isClimbing)
        {
            Debug.Log("Climbing");
            jumpCount = 0;
            rb.AddForce(orientation.forward * 100f, ForceMode.Force);
            // rb.useGravity = false;
            //make the rigidbody freez in y axis
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

    private void SpeedControl()
    {
        if (activeGrapple) return;
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

    public void ResetSingleActionInputs()
    {
        inputHandler.IsJumpingPressed = false;
        // Add any other single-use inputs here
    }


    //new code

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        dashing,
        air,
        climbing,
        grappling
    }

    public float dashSpeedChangeFactor;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;
    private float speedChangeFactor;
    public bool isClimbing;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
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
        else if (isClimbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = walkSpeed;
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

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        bool isBelow = targetPosition.y < transform.position.y;

        velocityToSet = isBelow ? Vector3.zero : CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<GrapplingRB>().StopGrapple();
        }
    }

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