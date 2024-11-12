using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public Transform player; // Reference to the player
    private NavMeshAgent agent;
    private Animator animator;

    public TextMeshProUGUI killsText;

    public float speed;

    public bool isAlive = true;
    public float dyingDelay = 2f;

    public int maxHealth = 100; // Maximum health of the enemy
    private int currentHealth;
    public int hitAmount = 20; // Amount of health lost per hit

    public float attackRange = 2f; // Range within which the enemy can attack
    public int attackDamage = 10; // Damage dealt by the enemy per attack
    public float attackCooldown = 1.5f; // Time between attacks
    private float lastAttackTime;

    public string[] attackTriggers = { "attack1" }; // Animation triggers for attacks

    private bool isAttacking = false; // Flag to indicate if an attack is in progress
    private bool isScreaming = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth; // Initialize current health
        killsText.text = "Kills: 0"; // Initialize kills text

        isScreaming = true;
        animator.SetTrigger("scream");

        // Disable automatic movement of the NavMeshAgent
        agent.updatePosition = false;
        agent.updateRotation = true;

    }

    void Update()
    {
        if (player != null && isAlive && !isScreaming)
        {
            agent.SetDestination(player.position);

            // Control animation based on speed
            speed = agent.velocity.magnitude / agent.speed; // Normalize speed between 0 and 1
            animator.SetFloat("speed", speed); // Update the speed parameter in the blend tree

            if (speed > 0f)
            {
                animator.SetBool("isMoving", true); // Set isMoving to true when the enemy is moving
            }
            else
            {
                animator.SetBool("isMoving", false); // Set isMoving to false when the enemy is idle
            }

            // Check if the enemy is within attack range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    }

    // Method to be called by the animation event to handle scream finished
    void HandleScreamFinished()
    {
        isScreaming = false;
    }

    void AttackPlayer()
    {
        // Randomly select one of the attack triggers
        int randomIndex = Random.Range(0, attackTriggers.Length);
        string selectedTrigger = attackTriggers[randomIndex];

        // Trigger the selected attack animation
        animator.SetTrigger(selectedTrigger);
        isAttacking = true; // Set the flag to indicate an attack is in progress
    }

    // Method to be called by the animation event to apply damage
    public void ApplyDamage()
    {
        if (isAttacking)
        {
            // Check if the player has a PlayerHealth component and apply damage
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            isAttacking = false; // Reset the flag after applying damage
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && isAlive)
        {
            Debug.Log("Enemy hit by projectile!");
            TakeDamage(hitAmount); // Reduce health by hit amount
        }
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetTrigger("dying"); // Start dying animation
        isAlive = false; // Set the enemy to not alive
        killsText.text = "Kills: " + (int.Parse(killsText.text.Split(' ')[1]) + 1); // Increment kills count

        // Start the coroutine to destroy the enemy after a delay
        StartCoroutine(DestroyAfterDelay(dyingDelay)); // Example delay of 2 seconds
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    void OnAnimatorMove()
    {
        Vector3 position = animator.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
        agent.nextPosition = transform.position;
    }
}
