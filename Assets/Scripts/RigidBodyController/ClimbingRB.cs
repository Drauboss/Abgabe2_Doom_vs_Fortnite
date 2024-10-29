using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClimbingRB : MonoBehaviour
{
    [Header("Climbing Settings")]
    [SerializeField] private float climbSpeed = 5.0f;
    [SerializeField] private float maxWallLookAngle = 45.0f;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float sphereCastRadius = 0.5f;
    [SerializeField] private float detectionLength = 2.0f;
    [SerializeField] private float maxClimbTime = 5.0f;

    [Header("References")]
    [SerializeField] private PlayerControllerRB playerController;
    [SerializeField] private PlayerInputHandlerRB inputHandler;
    [SerializeField] private Transform orientation;
    public Rigidbody rb;

    public bool climbing = false;
    public bool wallFront = false;
    private float wallLookAngle = 0.0f;
    private RaycastHit frontWallhit;
    private float climbTimer;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandlerRB>();
        playerController = GetComponent<PlayerControllerRB>();
    }
    // Update is called once per frame
    void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing)
        {
            ClimbMovement();
        }
    }

    private void StateMachine()
    {
        // State 1 - Climbing
        if (wallFront && inputHandler.IsClimbingPressed)
        {
            if (!climbing && climbTimer > 0)
            {
                StartClimbing();
            }

            //timer
            if (climbTimer > 0)
            {
                climbTimer -= Time.deltaTime;
            }
            else
            {
                StopClimbing();
            }
        }
        else
        {
            if (climbing)
            {
                StopClimbing();
            }
        }

    }
    private void WallCheck()
    {
        Vector3 sphereCastOrigin = transform.position + orientation.forward * sphereCastRadius;
        wallFront = Physics.SphereCast(sphereCastOrigin, sphereCastRadius, orientation.forward, out frontWallhit, detectionLength, whatIsWall);

        // Debugging: Draw the SphereCast path
        Debug.DrawLine(sphereCastOrigin, sphereCastOrigin + orientation.forward * detectionLength, Color.red);

        // Debugging: Draw a ray from the hit point
        if (wallFront)
        {
            Debug.DrawRay(frontWallhit.point, frontWallhit.normal, Color.green);
        }

        // Debugging: Draw spheres at the start and end points of the SphereCast
        //DrawDebugSpheres(sphereCastOrigin, sphereCastOrigin + orientation.forward * detectionLength);

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallhit.normal);

        if (playerController.grounded)
        {
            climbTimer = maxClimbTime;
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 sphereCastOrigin = transform.position + orientation.forward * sphereCastRadius;
        Vector3 sphereCastEnd = sphereCastOrigin + orientation.forward * detectionLength;

        // Draw spheres at the start and end points of the SphereCast
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(sphereCastOrigin, sphereCastRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(sphereCastEnd, sphereCastRadius);
    }

    private void StartClimbing()
    {
        climbing = true;
        playerController.isClimbing = true;
    }

    private void StopClimbing()
    {
        climbing = false;
        playerController.isClimbing = false;
    }

    private void ClimbMovement()
    {

        // Reduce horizontal movement speed by a factor (e.g., 0.8 for reduced speed)
        float speedReductionFactor = 0.8f;

        // Apply the speed reduction factor to the x and z components of moveVelocity
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Check if the player is looking towards the wall
        if (wallLookAngle < maxWallLookAngle)
        {
            // Check if the up key is pressed using moveInput from playerController
            if (inputHandler.MoveInput.y > 0)
            {
                // Apply the climbing speed to the vertical component
                rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
            }
            else if (inputHandler.MoveInput.y < 0)
            {
                // Apply the climbing speed to the vertical component
                rb.velocity = new Vector3(rb.velocity.x, -climbSpeed, rb.velocity.z);
            }
        }
    }
}
