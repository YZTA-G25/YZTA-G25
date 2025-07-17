using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.VisualScripting;

// HandController, bir NetworkBehaviour'dur. Bu, onun
// ağ üzerinde bir kimliğe sahip olmasını sağlar.
public class HandController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float pitchLimit = 85f;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float handSensitivity = 0.1f;
    [SerializeField] private float handVerticalSpeed = 2f;
    [SerializeField] private float handRotationSpeed = 45f;
    
    [Header("Gravity & Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("References")]
    [SerializeField] private Transform handTransform; // El pozisyonunu kontrol edeceğimiz obje
    [SerializeField] private CharacterController characterController; // Karakterin bedeni için
    [SerializeField] private HandInteractor handInteractor; // El etkileşim sistemi

    private PlayerControls playerControls;
    private Vector2 moveInput;
    private Vector2 lookInput; // For mouse look input like EyePlayer
    private Vector2 handMoveInput;
    private float handVerticalInput;
    private bool alternateModeActive;
    private bool grabActive;
    private float currentPitch = 0f; // Track vertical rotation like EyePlayer
    
    // Gravity and jumping variables
    private Vector3 velocity;

    private CinemachineCamera eyePlayerFeedCm;

    // Bu fonksiyon, obje ağ üzerinde spawn olduğunda Netcode tarafından çağrılır.
    // Bir objenin ağ üzerindeki yaşam döngüsünün başlangıcıdır.
    public override void OnNetworkSpawn()
    {
        // Kodun sadece bu objenin "sahibi" olan client'ta çalışmasını sağlar
        // Böylece bir oyuncu, diğer oyuncunun karakterini kontrol edemez.
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        // Sahibi olan client için yapılacak başlangıç ayarları

        // Auto-detect HandInteractor if not assigned
        if (handInteractor == null)
        {
            handInteractor = GetComponent<HandInteractor>();
            if (handInteractor == null)
            {
                handInteractor = GetComponentInChildren<HandInteractor>();
            }
        }

        // Input'ları burada aktive et
        playerControls = new PlayerControls();
        // PlayerInputManager.instance.JoinPlayer(); // REMOVED - This was causing NullReference
        playerControls.HandPlayer.Enable();
        playerControls.EyePlayer.Disable();

        // Her bir action için event'ler tanımlıyoruz
        playerControls.HandPlayer.Move.performed += OnMoveInput;
        playerControls.HandPlayer.Move.canceled += OnMoveInput;

        // Add Look input binding (assuming it exists in HandPlayer input actions)
        playerControls.HandPlayer.Look.performed += OnLookInput;
        playerControls.HandPlayer.Look.canceled += OnLookInput;

        playerControls.HandPlayer.HandMove.performed += OnHandMoveInput;
        playerControls.HandPlayer.HandMove.canceled += OnHandMoveInput;

        playerControls.HandPlayer.HandVertical.performed += OnHandVerticalInput;
        playerControls.HandPlayer.HandVertical.canceled += OnHandVerticalInput;

        playerControls.HandPlayer.AlternateMode.performed += OnAlternateModeInput;
        playerControls.HandPlayer.AlternateMode.canceled += OnAlternateModeInput;

        playerControls.HandPlayer.Grab.performed += OnGrabInput;
        playerControls.HandPlayer.Grab.canceled += OnGrabInput;
        
        // Add jump input if you have a jump action in your input actions
        playerControls.HandPlayer.Jump.performed += OnJumpInput;

        eyePlayerFeedCm = GameObject.FindGameObjectWithTag("EyePlayer Feed CM").GetComponent<CinemachineCamera>();
        eyePlayerFeedCm.Follow = this.gameObject.transform;
    }

    // OnNetworkDespawn, obje ağdan kaldırıldığında çalışır.
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        // Input'ları devre dışı bırakarak hafıza sızıntısını önlüyoruz
        playerControls.HandPlayer.Disable();
    }

    // Every frame'de çalışacak olan ana güncelleme fonksiyonu
    private void Update()
    {
        if (!IsOwner) return;
        if (!Application.isFocused) return;

        HandleMovement();
        HandleLook();
        HandleHandControl();
    }

    #region Control Logic

    // Karakterin yürüme mantığı
    private void HandleMovement()
    {
        Vector3 _moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        
        // Apply horizontal movement
        Vector3 horizontalMovement = transform.TransformDirection(_moveDirection) * moveSpeed * Time.deltaTime;
        
        // Apply gravity
        if (!characterController.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = -0.1f; // Small downward force to stay grounded
        }
        
        // Combine horizontal movement with vertical velocity
        Vector3 finalMovement = horizontalMovement + new Vector3(0, velocity.y * Time.deltaTime, 0);
        
        // Move the character
        characterController.Move(finalMovement);
    }

    // Karakterin bakış/dönüş mantığı (Similar to EyePlayerController)
    private void HandleLook()
    {
        // Normal mode'da karakterin dönmesi
        if (!alternateModeActive)
        {
            // Horizontal rotation (Y-axis) - frame rate independent
            float yaw = lookInput.x * mouseSensitivity;
            transform.Rotate(0, yaw, 0, Space.Self);

            // Vertical rotation (X-axis) - using pitch tracking with limits
            currentPitch += lookInput.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);

            // Calculate look target position based on clamped pitch (if lookAtTarget is assigned)
            if (lookAtTarget != null)
            {
                const float fixedDistance = 10f; // Use a fixed distance to avoid NaN issues
                float radianPitch = currentPitch * Mathf.Deg2Rad;
                
                Vector3 forward = transform.forward;
                Vector3 targetPosition = transform.position + forward * fixedDistance;
                
                // Use Sin instead of Tan to avoid infinite values at extreme angles
                float heightOffset = Mathf.Sin(radianPitch) * fixedDistance;
                targetPosition.y = transform.position.y + heightOffset;
                
                lookAtTarget.position = targetPosition;
            }
        }
    }

    // El kontrol mantuğı
    private void HandleHandControl()
    {
        // Eğer el objesi atanmamışsa hata vermemesi için
        if (handTransform == null)
        {
            Debug.LogError("handTransform is NULL in HandController");
            return;
        }

        // Sağ tuşa basılı tutuluyorsa (Alternate Mode)
        if (alternateModeActive)
        {
            // Elin Z ekseninde dönmesini sağla (Bu kısım doğru, kendi ekseninde döner)
            float _rotationAmount = handVerticalInput * handRotationSpeed * Time.deltaTime;
            handTransform.Rotate(0, 0, _rotationAmount, Space.Self);

            // --- DEĞİŞİKLİK BURADA ---
            // Mouse hareketinden yerel bir yön vektörü oluştur
            Vector3 _localHandMovement = new Vector3(handMoveInput.x, 0, handMoveInput.y);

            // Bu yerel yönü, oyuncunun mevcut dönüşüne göre dünya yönüne çevir
            Vector3 _worldSpaceMovement = transform.TransformDirection(_localHandMovement);

            // Elin pozisyonunu bu yeni, doğru yönde güncelle
            handTransform.position += _worldSpaceMovement * handSensitivity * Time.deltaTime;
        }
        else // Normal Mode
        {
            // Elin Y ekseninde hareketi (Yukarı/Aşağı) genellikle dünya koordinatında kalabilir, bu doğrudur.
            float _verticalMovement = handVerticalInput * handVerticalSpeed * Time.deltaTime;
            handTransform.Translate(0, _verticalMovement, 0, Space.World);
        }
    }
    #endregion

    #region Input Events
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnHandMoveInput(InputAction.CallbackContext context)
    {
        handMoveInput = context.ReadValue<Vector2>();
    }

    private void OnHandVerticalInput(InputAction.CallbackContext context)
    {
        handVerticalInput = context.ReadValue<float>();
    }

    private void OnAlternateModeInput(InputAction.CallbackContext context)
    {
        alternateModeActive = context.ReadValueAsButton();
    }

    private void OnGrabInput(InputAction.CallbackContext context)
    {
        grabActive = context.ReadValueAsButton();
        if (grabActive)
        {
            Debug.Log("Grab Pressed!");
        }
        
        // Forward grab input to HandInteractor
        if (handInteractor != null)
        {
            handInteractor.OnGrab(context);
        }
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump performed!");
        }
    }

    #endregion
}
