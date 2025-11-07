using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;
using Unity.VisualScripting; 

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))] 
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 2.5f;
    public float crouchSpeed = 2.5f;
    public float turnSpeed = 15f;
    public float sprintSpeed = 10f;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera freeLookCamera;
    public CinemachineCamera aimCamera;

    [Header("UI Settings")]
    public Image crosshairImage;

    [Header("Animation Settings")]
    [Tooltip("Animator'deki 'Aiming' katmanının indeksi (Genellikle 1)")]
    public int aimLayerIndex = 1;

    [Header("VFX Settings")]
    public Transform gunMuzzle;
    public GameObject animatedTracerPrefab;
    public GameObject impactEffectPrefab;
    public GameObject bulletHolePrefab;

    [Header("Animation Smoothing")]
    [Tooltip("Animasyon geçişlerinin ne kadar yumuşak olacağı (düşük değer = daha hızlı)")]
    public float animationSmoothTime = 0.1f;

    private float animationSpeed;  
    private float animationVelocity;

    [Header("Combat Settings")]
    [Tooltip("Saniyelik atış sayısı (Örn: 10 = saniyede 10 mermi)")]
    public float fireRate = 10f;

    [Tooltip("Her merminin vereceği temel hasar miktarı")]
    public float bulletDamage = 25f;

    [Tooltip("Merminin çarpabileceği her şey. Kendi 'PlayerHitbox' katmanınız SEÇİLİ OLMAMALI.")]
    public LayerMask shootableLayers;
    private float nextFireTime = 0f;
    private bool isFiring = false;


    private Rigidbody rb;
    private Animator animator;
    private PlayerInputActions playerInputActions;
    [SerializeField] private Transform mainCameraTransform;
    private Health health; 

    private Vector2 moveInput;
    private bool isAiming;
    private bool isCrouching;
    private bool isGrounded = true;
    private bool isSprinting = false;
    private int normalPriority = 10;
    private int aimPriority = 20;


    private void Awake()
    {
        if (Time.timeScale < 1f)
        {
            Time.timeScale = 1f;
        }
        if (AudioListener.pause == true)
        {
            AudioListener.pause = false;
        }
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        playerInputActions = new PlayerInputActions();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Jump.performed += OnJump;
        playerInputActions.Player.Crouch.started += OnCrouch;
        playerInputActions.Player.Crouch.canceled += OnCrouch;
        playerInputActions.Player.Aim.started += OnAim;
        playerInputActions.Player.Aim.canceled += OnAim;
        playerInputActions.Player.Fire.started += OnFireStart;
        playerInputActions.Player.Fire.canceled += OnFireCancel;
        playerInputActions.Player.Sprint.started += OnSprint;
        playerInputActions.Player.Sprint.canceled += OnSprint;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Jump.performed -= OnJump;
        playerInputActions.Player.Crouch.started -= OnCrouch;
        playerInputActions.Player.Crouch.canceled -= OnCrouch;
        playerInputActions.Player.Aim.started -= OnAim;
        playerInputActions.Player.Aim.canceled -= OnAim;
        playerInputActions.Player.Fire.started -= OnFireStart;
        playerInputActions.Player.Fire.canceled -= OnFireCancel;
        playerInputActions.Player.Sprint.started -= OnSprint;
        playerInputActions.Player.Sprint.canceled -= OnSprint;
    }

    private void FixedUpdate()
    {
        if (health != null && health.isDead)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            return;
        }

        if (mainCameraTransform == null) return;
        rb.angularVelocity = Vector3.zero;
        MovePlayer();
        HandleRotation();
    }

    private void Update()
    {
        if (health != null && health.isDead)
        {
            if (crosshairImage != null && crosshairImage.enabled)
            {
                crosshairImage.enabled = false;
            }

            animator.SetBool("isDead", true);
            return;
        }

        HandleCameraPriorities();
        UpdateAnimatorParameters();
        HandleCrosshair();
        HandleAutomaticFire();
    }

    private void HandleAutomaticFire()
    {
        if (isAiming && isFiring)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
                PerformShot();
            }
        }
    }

    private void OnMove(InputAction.CallbackContext context) { moveInput = context.ReadValue<Vector2>(); }
    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            animator.SetTrigger("Jump");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }
    private void OnCrouch(InputAction.CallbackContext context) { isCrouching = context.ReadValueAsButton(); }
    private void OnAim(InputAction.CallbackContext context) { isAiming = context.ReadValueAsButton(); }
    private void OnSprint(InputAction.CallbackContext context){isSprinting = context.ReadValueAsButton();}
    private void OnFireStart(InputAction.CallbackContext context) { isFiring = true; }
    private void OnFireCancel(InputAction.CallbackContext context) { isFiring = false; }


    private void PerformShot()
    {
        if (animator != null)
        {

            animator.CrossFadeInFixedTime("RifleFire", 0.05f, aimLayerIndex, 0f);
        }

        if (gunMuzzle == null)
        {
            Debug.LogError("ATEŞ ETME HATASI: 'Gun Muzzle' slotu Inspector'da boş! Ateş edilemiyor.");
            return;
        }

        Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, shootableLayers))
        {
            Debug.Log("HEDEF BULUNDU: " + hit.collider.name);

            if (animatedTracerPrefab != null)
            {
                GameObject tracerObj = Instantiate(animatedTracerPrefab, gunMuzzle.position, Quaternion.identity);
                AnimatedTracer tracer = tracerObj.GetComponent<AnimatedTracer>();
                if (tracer != null)
                {
                    tracer.Initialize(hit.point, bulletDamage, impactEffectPrefab, bulletHolePrefab, hit.transform, hit.normal);
                }
                else
                {
                    Debug.LogError("ATEŞ ETME HATASI: 'AnimatedTracer' prefab'ınızın üzerinde 'AnimatedTracer.cs' script'i bulunmuyor!");
                }
            }
        }
        else
        {
            Debug.Log("HEDEF BULUNAMADI (Boşluğa ateş edildi)");

            if (animatedTracerPrefab != null)
            {
                Vector3 endPoint = ray.GetPoint(100f);
                GameObject tracerObj = Instantiate(animatedTracerPrefab, gunMuzzle.position, Quaternion.identity);
                AnimatedTracer tracer = tracerObj.GetComponent<AnimatedTracer>();
                if (tracer != null)
                {
                    tracer.Initialize(endPoint, bulletDamage, null, null, null, Vector3.zero);
                }
                else
                {
                    Debug.LogError("ATEŞ ETME HATASI: 'AnimatedTracer' prefab'ınızın üzerinde 'AnimatedTracer.cs' script'i bulunmuyor!");
                }
            }
        }
    }


    private void MovePlayer()
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        float currentSpeed;

        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting && !isAiming) 
        {
            currentSpeed = sprintSpeed; 
        }
        else
        {
            currentSpeed = moveSpeed; 
        }

        Vector3 targetVelocity = moveDirection * currentSpeed;

        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    private void HandleRotation()
    {
        Vector3 lookDirection;

        if (isAiming)
        {
            lookDirection = mainCameraTransform.forward;
        }
        else
        {
            Vector3 moveDirection = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (moveDirection == Vector3.zero) return;
            lookDirection = moveDirection;
        }

        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }

    private void HandleCameraPriorities()
    {
        if (freeLookCamera == null || aimCamera == null) return;

        if (isAiming)
        {
            freeLookCamera.Priority = normalPriority;
            aimCamera.Priority = aimPriority;
        }
        else
        {
            freeLookCamera.Priority = aimPriority;
            aimCamera.Priority = normalPriority;
        }
    }

    private void HandleCrosshair()
    {
        if (crosshairImage == null) return;
        crosshairImage.enabled = isAiming;
    }

    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        float targetSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        animationSpeed = Mathf.SmoothDamp(
            animationSpeed,       
            targetSpeed,         
            ref animationVelocity,
            animationSmoothTime   
        );

        animator.SetFloat("Speed", animationSpeed);


        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("isAiming", isAiming);
        animator.SetBool("isSprinting", isSprinting && !isAiming);

        if (isAiming)
        {
            animator.SetLayerWeight(aimLayerIndex, 1f);
        }
        else
        {
            animator.SetLayerWeight(aimLayerIndex, Mathf.Lerp(animator.GetLayerWeight(aimLayerIndex), 0f, Time.deltaTime * 10f));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (health != null && health.isDead) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (health != null && health.isDead) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (health != null && health.isDead)
        {
            if (animator != null)
            {
                animator.SetLookAtWeight(0);
            }
            return;
        }

        if (animator != null && isAiming)
        {
            animator.SetLookAtWeight(1f, 0.3f, 1f);
            Vector3 lookAtPoint = mainCameraTransform.position + mainCameraTransform.forward * 50f;
            animator.SetLookAtPosition(lookAtPoint);
        }
    }
}