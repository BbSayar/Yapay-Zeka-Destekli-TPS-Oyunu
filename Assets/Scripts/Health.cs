using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public bool isDead { get; private set; } = false;

    [Header("UI Settings")]
    public Slider healthBarSlider;
    public Image healthBarFillImage;
    public Color highHealthColor = Color.green;
    public Color mediumHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Health Text")]
    public TextMeshProUGUI healthText;
    public string healthTextFormat = "{0} / {1}";

    [Header("Death Settings")]
    public Animator animator; 
    public GameObject deathEffectPrefab;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PlayerController playerController;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!isDead && Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            TakeDamage(20);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || damageAmount <= 0) return; 

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log(gameObject.name + " " + damageAmount + " hasar ald�. Kalan Can: " + currentHealth);

        UpdateHealthBar(); 

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider == null) return;
        float healthPercentage = currentHealth / maxHealth;
        healthBarSlider.value = healthPercentage;
        if (healthBarFillImage != null)
        {
            if (healthPercentage > 0.7f)
                healthBarFillImage.color = highHealthColor;
            else if (healthPercentage > 0.3f)
                healthBarFillImage.color = mediumHealthColor;
            else
                healthBarFillImage.color = lowHealthColor;
        }
        if (healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, Mathf.CeilToInt(currentHealth), maxHealth);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        GameManager.Instance.ShowGameOverScreen();
        Debug.Log(gameObject.name + " öldü!");

        if (animator != null)
        {
            if (playerController != null) 
            {
                animator.SetLayerWeight(playerController.aimLayerIndex, 0f);
            }


            animator.SetTrigger("Die");
        }

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (playerController != null)
            playerController.enabled = false;

    }
}