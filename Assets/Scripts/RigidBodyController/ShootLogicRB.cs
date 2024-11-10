using System;
using System.Collections;
using UnityEngine;

public class ShootLogicRB : MonoBehaviour
{
    private PlayerInputHandlerRB inputHandler;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootForce;

    public float shotDelay = .5f;

    // --- Muzzle ---
    public GameObject muzzlePrefab;
    public GameObject muzzlePosition;

    // --- Timing ---
    [SerializeField] private float timeLastFired;

    /// <summary>
    /// </summary>
    private void Start()
    {
        inputHandler = GetComponent<PlayerInputHandlerRB>();

        inputHandler.OnShoot += HandleShoot;
    }

    private void OnDestroy()
    {
        inputHandler.OnShoot -= HandleShoot;
    }

    private void HandleShoot()
    {
        // --- Fires the weapon if the delay time period has passed since the last shot ---
        if ((timeLastFired + shotDelay) <= Time.time)
        {
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // --- Keep track of when the weapon is being fired ---
            timeLastFired = Time.time;

            // --- Spawn muzzle flash ---
            var flash = Instantiate(muzzlePrefab, muzzlePosition.transform);

            // Instantiate the projectile at the shoot point
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            if (projectileRb != null)
            {
                // Set the projectile's rotation to match the shootPoint's forward direction
                projectile.transform.forward = shootPoint.forward;

                // Apply force to the projectile
                projectileRb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);

                // Draw a debug ray to visualize the shot direction
                Debug.DrawRay(shootPoint.position, shootPoint.forward * 10, Color.red, 2.0f); // Adjust length and duration as needed

            }
        }
    }
}
