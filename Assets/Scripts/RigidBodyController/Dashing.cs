using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private Rigidbody rb;
    private PlayerControllerRB playerController;

    [Header("Dashing")]
    public float dashForce;
    public float dashDuration;

    [Header("CameraEffects")]
    public CameraControllerRB cam;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    private PlayerInputHandlerRB inputHandler;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerControllerRB>();
        inputHandler = GetComponent<PlayerInputHandlerRB>();
    }

    private void Update()
    {
        if (inputHandler.IsDashingPressed)
            Dash();

        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        playerController.dashing = true;
        // playerController.maxYSpeed = maxDashYSpeed;

        cam.DoFov();

        Vector3 forceToApply = orientation.forward * dashForce;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        rb.velocity = Vector3.zero; // reset velocity

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        playerController.dashing = false;

        cam.ResetFov();

    }

}