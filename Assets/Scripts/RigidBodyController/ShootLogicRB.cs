using System;
using System.Collections;
using UnityEngine;

public class ShootLogicRB : MonoBehaviour
{
    private PlayerInputHandlerRB inputHandler;
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootForce;

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
        Debug.Log("Player shot!");
        ShootProjectile();
    }

    private void ShootProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // Instantiate the projectile at the shoot point
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            if (projectileRb != null)
            {
                // Set the projectile's rotation to match the shootPoint's forward direction
                projectile.transform.forward = shootPoint.forward;

                // Apply force to the projectile
                projectileRb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);

            }
        }
    }
}
