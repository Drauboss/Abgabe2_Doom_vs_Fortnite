using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player; // Referenz auf den Spieler
    private NavMeshAgent agent;
    private Animator animator;

    public float speed;
    public bool isAlive = true;
    public float dyingDelay = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player != null && isAlive)
        {
            agent.SetDestination(player.position);

            // Animation basierend auf der Geschwindigkeit steuern
            speed = agent.velocity.magnitude;
            if (speed > 4f) // Adjusted threshold for running
            {
                animator.SetBool("isRunning", true); // Startet die Laufanimation
                animator.SetBool("isWalking", false); // Stoppt die Geh-Animation
            }
            else if (speed > 0f && speed <= 4f) // Adjusted threshold for walking
            {
                animator.SetBool("isRunning", false); // Stoppt die Laufanimation
                animator.SetBool("isWalking", true); // Startet die Geh-Animation
            }
            else
            {
                animator.SetBool("isRunning", false); // Stoppt die Laufanimation
                animator.SetBool("isWalking", false); // Startet die Idle-Animation
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            Debug.Log("Enemy hit by projectile!");
            animator.SetTrigger("dying"); // Start the hit animation
            isAlive = false; // Set the enemy to not alive

            // Start the coroutine to destroy the enemy after a delay
            StartCoroutine(DestroyAfterDelay(dyingDelay)); // Example delay of 2 seconds
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


}
