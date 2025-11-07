using UnityEngine;

public class AnimatedTracer : MonoBehaviour
{
    [Tooltip("Merminin saniyedeki hýzý (çok yüksek olmalý)")]
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
            Hitbox hitbox = hitTransform.GetComponent<Hitbox>();

            if (hitbox != null) 
            {
                hitbox.ApplyDamage(damageToDeal);
            }
            else
            {
                    Health targetHealth = hitTransform.GetComponent<Health>();
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
            Debug.Log("MERMÝ DELÝÐÝ OLUÞTURULUYOR!");

            Vector3 holePosition = targetPoint + hitNormal * 0.01f;
            Quaternion holeRotation = Quaternion.LookRotation(-hitNormal, Vector3.up);
            GameObject hole = Instantiate(bulletHolePrefab, holePosition, holeRotation);

            Hitbox npcHitbox = hitTransform.GetComponent<Hitbox>();

            if (npcHitbox == null)
            {
                hole.transform.SetParent(hitTransform);
            }

            Destroy(hole, 5f);
        }
    }
}