using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
using Unity.VisualScripting;
using UnityEngine.Events;

#endif


#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[RequireComponent(typeof(CharacterController))]
public class EyePlayerController : NetworkBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float footstepInterval = 0.5f; // Adım sesleri arasındaki saniye
    private float footstepTimer = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float pitchLimit = 85f;
    [SerializeField] private Transform lookAtTarget;

    [Header("Gravity & Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;


    [Header("Component References")]
    [Tooltip("Eye Player'ın kamerasının olduğu transform")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private PlayerControls playerControls;
    private float currentPitch = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float grabActive;

    // Gravity and jumping variables
    private Vector3 velocity;
    private bool isGrounded;

    private float jumpCooldown = 0.05f;

    public static UnityEvent OnSpawned = new UnityEvent();

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // İmleci Kitle
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public override void OnNetworkSpawn()
    {
        OnSpawned.Invoke();

        if (IsOwner)
        {
            InGameUIManager.OnGamePaused += HandleGamePaused;
        }
    }
    public override void OnNetworkDespawn()
    {
        // ... mevcut OnNetworkDespawn kodlarınızın sonuna ekleyin ...

        if (IsOwner)
        {
            InGameUIManager.OnGamePaused -= HandleGamePaused;
        }
    }
    public void Start()
    {
        // Only initialize input for the owner
        if (!GetComponent<NetworkObject>().IsOwner) return;

        try
        {
            playerControls = new PlayerControls();
            playerControls.EyePlayer.Enable();
            playerControls.HandPlayer.Disable();

            playerControls.EyePlayer.Move.performed += OnMoveInput;
            playerControls.EyePlayer.Move.canceled += OnMoveInput;

            playerControls.EyePlayer.Look.performed += OnLookInput;
            playerControls.EyePlayer.Look.canceled += OnLookInput;

            playerControls.EyePlayer.Interact.performed += OnGrabInput;
            playerControls.EyePlayer.Interact.canceled += OnGrabInput;

            playerControls.EyePlayer.Jump.performed += OnJumpInput;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error initializing PlayerControls in EyePlayerController: " + e.Message);
        }
    }

    private void Update()
    {
        // Only process input for the owner of this network object
        if (!IsOwner) return;
        if (!Application.isFocused) return;

        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y); 
        
        // Apply horizontal movement
        Vector3 horizontalMovement = transform.TransformDirection(moveDirection) * moveSpeed;
        
        // Update grounded state
        isGrounded = characterController.isGrounded;

        jumpCooldown -= Time.deltaTime;

        // Apply gravity
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            jumpCooldown = 0.03f;
        }
        else
        {
            // Only reset velocity if we're not in a jump cooldown
            if (jumpCooldown <= 0f)
            {
                velocity.y = -0.1f; // Small downward force to stay grounded
            }
        }
        
        // Combine horizontal movement with vertical velocity
        Vector3 finalMovement = horizontalMovement + new Vector3(0, velocity.y, 0);
        
        // Move the character
        characterController.Move(finalMovement * Time.deltaTime);

        footstepTimer -= Time.deltaTime;

        // Yatay hareketin büyüklüğünü hesapla (y eksenini yok sayarak)
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);

        // Eğer oyuncu yerde, yatay olarak hareket ediyor ve zamanlayıcı sıfırlandıysa...
        if (characterController.isGrounded && horizontalVelocity.magnitude > 0.1f && footstepTimer <= 0f)
        {
            // ...ayak sesini çal.
            SoundManager.PlaySound(SoundType.FOOTSTEP);

            // ...ve zamanlayıcıyı yeniden başlat.
            footstepTimer = footstepInterval;
        }
    }

    private void HandleLook()
    {
        // Horizontal rotation (Y-axis) - frame rate independent
        float yaw = lookInput.x * mouseSensitivity;
        transform.Rotate(0, yaw, 0, Space.Self);

        // Vertical rotation (X-axis) - using pitch tracking with limits
        currentPitch += lookInput.y * mouseSensitivity;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);

        // Calculate look target position based on clamped pitch
        const float fixedDistance = 10f; // Use a fixed distance to avoid NaN issues
        float radianPitch = currentPitch * Mathf.Deg2Rad;
        
        Vector3 forward = transform.forward;
        Vector3 targetPosition = transform.position + forward * fixedDistance;
        
        // Use Sin instead of Tan to avoid infinite values at extreme angles
        float heightOffset = Mathf.Sin(radianPitch) * fixedDistance;
        targetPosition.y = transform.position.y + heightOffset;
        
        lookAtTarget.position = targetPosition;
    }

    #region Input Events
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnGrabInput(InputAction.CallbackContext context)
    {
        grabActive = context.ReadValue<float>();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        Debug.Log($"Jump input detected - isGrounded: {isGrounded}, characterController.isGrounded: {characterController.isGrounded}, velocity.y: {velocity.y}");

        if (context.performed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"EyePlayer Jump performed! New velocity.y: {velocity.y}");
        }
        else if (context.performed && !isGrounded)
        {
            Debug.Log("Jump input received but player is not grounded!");
        }
    }
    #endregion

    public override void OnDestroy()
    {
        if (playerControls != null)
        {
            playerControls.EyePlayer.Move.performed -= OnMoveInput;
            playerControls.EyePlayer.Move.canceled -= OnMoveInput;

            playerControls.EyePlayer.Look.performed -= OnLookInput;
            playerControls.EyePlayer.Look.canceled -= OnLookInput;

            playerControls.EyePlayer.Interact.performed -= OnGrabInput;
            playerControls.EyePlayer.Interact.canceled -= OnGrabInput;
            
            playerControls.EyePlayer.Jump.performed -= OnJumpInput;
            
            playerControls.Dispose();
        }
        
        base.OnDestroy();
    }

    private void HandleGamePaused(bool isPaused)
    {
        // isPaused true ise bu script'i devre dışı bırak, değilse etkinleştir.
        this.enabled = !isPaused;
    }
}
