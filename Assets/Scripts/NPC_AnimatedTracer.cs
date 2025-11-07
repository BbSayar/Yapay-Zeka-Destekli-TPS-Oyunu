using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class NPC_AnimatedTracer : MonoBehaviour
{
    public float speed = 750f;

    private float damageToDeal;
    private Vector3 targetPoint;
    private GameObject impactEffectPrefab;
    private GameObject bulletHolePrefab;
    private Transform hitTransform;
    private Vector3 hitNormal;

    public void Initialize(Vector3 target, float damage, GameObject impactVFX, GameObject holeVFX, Transform hitObject, Vector3 hitSurfaceNormal)
    {
        this.targetPoint = target;
        this.damageToDeal = damage;
        this.impactEffectPrefab = impactVFX;
        this.bulletHolePrefab = holeVFX;
        this.hitTransform = hitObject;
        this.hitNormal = hitSurfaceNormal;

        transform.LookAt(targetPoint);
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPoint);
        if (distanceToTarget <= 0.5f)
        {
            SpawnEffects();
            Destroy(gameObject);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);
        }
    }


    private void SpawnEffects()
    {
        if (hitTransform != null)
        {
            Debug.Log("NPC_Tracer -> HEDEFE ULAÞTI: Çarptýðý obje: " + hitTransform.name);
            PlayerHitbox playerHitbox = hitTransform.GetComponent<PlayerHitbox>(); 

            if (playerHitbox != null) 
            {
                playerHitbox.ApplyDamage(damageToDeal);
            }
            else
            {

                Health targetHealth = hitTransform.GetComponent<Health>(); // <-- DÜZELTME 3

                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damageToDeal);
                }
            }
        }

        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, targetPoint, Quaternion.LookRotation(hitNormal));
        }

        if (bulletHolePrefab != null && hitTransform != null)
        {
            Vector3 holePosition = targetPoint + hitNormal * 0.01f;
            Quaternion holeRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
            GameObject hole = Instantiate(bulletHolePrefab, holePosition, holeRotation);

            PlayerHitbox playerHitbox = hitTransform.GetComponent<PlayerHitbox>();

            if (playerHitbox == null)
            {
                hole.transform.SetParent(hitTransform);
            }
            Destroy(hole, 5f);
        }
    }
}