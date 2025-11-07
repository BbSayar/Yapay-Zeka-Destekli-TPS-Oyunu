using UnityEngine;
using UnityEngine.AI;

public class NPC_Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead { get; private set; } = false;

    [Header("Death Settings")]
    public Animator animator;
    public GameObject deathEffectPrefab;

    private NPC_AI npcAiController;
    private NavMeshAgent navAgent;
    private CapsuleCollider capsuleCollider; 

    void Awake()
    {
        currentHealth = maxHealth;

        npcAiController = GetComponent<NPC_AI>();
        navAgent = GetComponent<NavMeshAgent>();
        capsuleCollider = GetComponent<CapsuleCollider>(); 
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || damageAmount <= 0) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(gameObject.name + " " + damageAmount + " hasar aldý. Kalan Can: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (npcAiController != null && npcAiController.currentState != NPC_AI.AIState.Attack)
            {
                npcAiController.currentState = NPC_AI.AIState.Chase;
            }
        }
    }

    private void Die()
    {
        if (isDead) return; 
        isDead = true;

        Debug.Log(gameObject.name + " ÖLDÜ!");

        if (npcAiController != null)
        {
            npcAiController.enabled = false; 
        }
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = false; 
        }

        if (animator != null)
        {

            if (npcAiController != null)
            {
                animator.SetLayerWeight(npcAiController.aimLayerIndex, 0f);
            }

            animator.SetTrigger("Die"); 
        }

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 5f);
    }
}