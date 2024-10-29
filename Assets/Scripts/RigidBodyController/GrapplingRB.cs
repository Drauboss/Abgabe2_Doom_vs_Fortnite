using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRB : MonoBehaviour
{
    [Header("References")]
    private PlayerControllerRB playerController;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    private PlayerInputHandlerRB inputHandler;

    private bool grappling;

    private void Start()
    {
        playerController = GetComponent<PlayerControllerRB>();
        inputHandler = GetComponent<PlayerInputHandlerRB>();
    }

    private void Update()
    {
        if (inputHandler.IsGrapplingPressed) StartGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        inputHandler.IsGrapplingPressed = false;
    }

    private void LateUpdate()
    {
        if (grappling)
            lr.SetPosition(0, gunTip.position);
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;

        // playerController.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            // Draw a line from the camera to the hit point
            Debug.DrawLine(cam.position, grapplePoint, Color.red, 2f);

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            // Draw a line from the camera to the max grapple distance point
            Debug.DrawLine(cam.position, grapplePoint, Color.red, 2f);

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        // playerController.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        playerController.JumpToPosition(grapplePoint, grapplePoint.y);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        // playerController.freeze = false;

        grappling = false;
        playerController.activeGrapple = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}