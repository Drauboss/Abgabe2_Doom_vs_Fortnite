using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Climbing : MonoBehaviour
{

    [Header("References")]
    public Transform orientation;
    public LayerMask whatIsWall;
    public PlayerController playerController;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    private bool climbing;

    [Header("Detection")]
    public float detectionHeight;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallhit;
    private bool wallFront;

    private bool isClimbKeyPressed;


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
        if (wallFront && isClimbKeyPressed)
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
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallhit, detectionHeight, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallhit.normal);

        if (playerController.characterController.isGrounded)
        {
            climbTimer = maxClimbTime;
        }
    }

    private void StartClimbing()
    {
        climbing = true;
        playerController.isClimbing = true;
        Debug.Log("Start Climbing");
    }

    private void StopClimbing()
    {
        climbing = false;
        playerController.isClimbing = false;
        Debug.Log("Stop Climbing");
    }

    private void ClimbMovement()
    {
        Debug.Log("Climbing");

        // Reduce horizontal movement speed by a factor (e.g., 0.8 for reduced speed)
        float speedReductionFactor = 0.8f;

        // Apply the speed reduction factor to the x and z components of moveVelocity
        playerController.moveVelocity = new Vector3(
            playerController.moveVelocity.x * speedReductionFactor,
            0, // Set vertical velocity to zero by default
            playerController.moveVelocity.z * speedReductionFactor
        );

        // Check if the player is looking towards the wall
        if (wallLookAngle < maxWallLookAngle)
        {
            // Check if the up key is pressed using moveInput from playerController
            if (playerController.moveInput.y > 0)
            {
                // Apply the climbing speed to the vertical component
                playerController.moveVelocity = new Vector3(
                    playerController.moveVelocity.x,
                    climbSpeed,
                    playerController.moveVelocity.z
                );
            }
        }
    }

    // region Inputs

    public void HandleClimbInput(InputAction.CallbackContext context)
    {
        isClimbKeyPressed = context.ReadValueAsButton(); // Step 2: Update the input state
        Debug.Log("Climb Input: " + isClimbKeyPressed);

    }
    // endregion
}
