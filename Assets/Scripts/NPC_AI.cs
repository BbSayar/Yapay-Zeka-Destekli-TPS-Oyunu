using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    public enum AIState
    {
        Idle, Patrol, Chase, Attack, Dead
    }

    [Header("Hýz Ayarlarý")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 10f;

    [Header("Durum Bilgisi")]
    public AIState currentState;

    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private Animator animator;
    private NPC_Health health;

    [Header("Animasyon Katmaný")]
    public int aimLayerIndex = 1;

    [Header("Devriye (Patrol) Ayarlarý")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 3f;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;

    [Header("Görüþ ve Saldýrý Ayarlarý")]
    public float patrolSightRange = 20f; 
    public float chaseSightRange = 40f;  
    public float attackRange = 5f;
    public float attackCooldown = 1.5f; 
    private float attackTimer = 0f;

    [Header("NPC Savaþ Ayarlarý")]
    public float npcDamage = 10f;
    public Transform gunMuzzle;
    public GameObject npcTracerPrefab;
    public GameObject impactEffectPrefab;
    public GameObject bulletHolePrefab;
    public LayerMask playerShootableLayers;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<NPC_Health>();
        animator = GetComponentInChildren<Animator>();

        if (health == null)
        {
            Debug.LogError(gameObject.name + " üzerinde NPC_Health.cs script'i bulunamadý!");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (animator != null)
        {
            animator.SetBool("isAiming", false);
            animator.SetLayerWeight(aimLayerIndex, 0f);
        }

        navAgent.speed = patrolSpeed;
        currentState = AIState.Patrol;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (health != null && health.isDead)
        {
            return;
        }

        if (playerTransform == null || animator == null) return;

        switch (currentState)
        {
            case AIState.Idle: Idle(); break;
            case AIState.Patrol: Patrol(); break;
            case AIState.Chase: Chase(); break;
            case AIState.Attack: Attack(); break;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        UpdateAnimationLayerWeights();
    }


    void Idle()
    {
        animator.SetBool("isMoving", false);
        waitTimer += Time.deltaTime;
        if (waitTimer >= patrolWaitTime)
        {
            GoToNextPatrolPoint();
            currentState = AIState.Patrol;
        }

        if (IsPlayerInPatrolRange())
        {
            currentState = AIState.Chase;
        }
    }

    void Patrol()
    {
        navAgent.speed = patrolSpeed; 
        animator.SetBool("isMoving", true);

        if (IsPlayerInPatrolRange())
        {
            currentState = AIState.Chase;
            return;
        }

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            currentState = AIState.Idle;
            waitTimer = 0f;
        }
    }

    void Chase()
    {
        navAgent.speed = chaseSpeed;
        animator.SetBool("isMoving", true);
        navAgent.SetDestination(playerTransform.position);

        if (IsPlayerInAttackRange())
        {
            currentState = AIState.Attack;
        }
        else if (!IsPlayerInChaseRange())
        {
            currentState = AIState.Patrol;
            GoToNextPatrolPoint();
        }
    }

    void Attack()
    {
        animator.SetBool("isMoving", false);
        navAgent.SetDestination(transform.position); 

        Vector3 lookPos = playerTransform.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");

            Vector3 targetCenter = playerTransform.position + Vector3.up * 1.5f;
            Vector3 direction = (targetCenter - gunMuzzle.position).normalized;

            RaycastHit hit;
            Vector3 targetPoint;
            Transform hitTransform = null;
            Vector3 hitNormal = Vector3.zero;

            // Ateþ ederken LayerMask'ý kullan
            if (Physics.Raycast(gunMuzzle.position, direction, out hit, 100f, playerShootableLayers))
            {
                targetPoint = hit.point;
                hitTransform = hit.transform;
                hitNormal = hit.normal;

                Debug.LogWarning("NPC_AI -> ATEÞ ETTÝ (HIT): Raycast '" + hit.collider.name + "' objesine çarptý. Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            }
            else
            {
                targetPoint = gunMuzzle.position + direction * 100f;
                Debug.LogError("NPC_AI -> ATEÞ ETTÝ (MISS): Raycast 'playerShootableLayers' katmanýndaki HÝÇBÝR ÞEYE çarpmadý.");
            }

            Debug.DrawRay(gunMuzzle.position, direction * 100f, Color.magenta, 5.0f);

            if (Physics.Raycast(gunMuzzle.position, direction, out hit, 100f, playerShootableLayers))
            {
                targetPoint = hit.point;
                hitTransform = hit.transform;
                hitNormal = hit.normal;
            }
            else
            {
                targetPoint = gunMuzzle.position + direction * 100f;
            }

            Debug.DrawRay(gunMuzzle.position, direction * 100f, Color.magenta, 5f); 

            if (npcTracerPrefab != null)
            {
                GameObject tracerObj = Instantiate(npcTracerPrefab, gunMuzzle.position, Quaternion.identity);
                NPC_AnimatedTracer tracer = tracerObj.GetComponent<NPC_AnimatedTracer>();
                if (tracer != null)
                {
                    tracer.Initialize(targetPoint, npcDamage, impactEffectPrefab, bulletHolePrefab, hitTransform, hitNormal);
                }
            }
            attackTimer = attackCooldown;
        }

        if (!IsPlayerInAttackRange())
        {
            currentState = AIState.Chase;
        }


    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
        {
            currentState = AIState.Idle; return;
        }
        navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    bool IsPlayerInPatrolRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= patrolSightRange;
    }

    bool IsPlayerInChaseRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= chaseSightRange;
    }

    bool IsPlayerInAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= attackRange;
    }

    private void UpdateAnimationLayerWeights()
    {
        if (animator == null) return;

        if (currentState == AIState.Chase || currentState == AIState.Attack)
        {
            animator.SetLayerWeight(aimLayerIndex, Mathf.Lerp(animator.GetLayerWeight(aimLayerIndex), 1f, Time.deltaTime * 10f));
            animator.SetBool("isAiming", true);
        }
        else
        {
            animator.SetLayerWeight(aimLayerIndex, Mathf.Lerp(animator.GetLayerWeight(aimLayerIndex), 0f, Time.deltaTime * 10f));
            animator.SetBool("isAiming", false);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator != null && (health == null || !health.isDead))
        {
            if (playerTransform != null && (currentState == AIState.Chase || currentState == AIState.Attack))
            {
                Vector3 lookAtPoint = playerTransform.position + Vector3.up * 1.5f;
                animator.SetLookAtWeight(1f, 0.5f, 1f);
                animator.SetLookAtPosition(lookAtPoint);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }
        }
        else if (animator != null)
        {
            animator.SetLookAtWeight(0);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // Tespit menzili
        Gizmos.DrawWireSphere(transform.position, patrolSightRange);

        Gizmos.color = Color.yellow; // Takip menzili
        Gizmos.DrawWireSphere(transform.position, chaseSightRange);

        Gizmos.color = Color.red; // Saldýrý menzili
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}