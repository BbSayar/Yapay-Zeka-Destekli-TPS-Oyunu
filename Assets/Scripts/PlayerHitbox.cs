
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public Health healthController; 

    [Tooltip("Kafa: 3, Gövde: 1, Bacak: 0.75 gibi")]
    public float damageMultiplier = 1.0f;

    public void ApplyDamage(float baseDamage)
    {
        if (healthController != null)
        {
            float finalDamage = baseDamage * damageMultiplier;

            healthController.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogWarning("Bu hitbox için 'Health' (healthController) referansý atanmamýþ!");
        }
    }
}