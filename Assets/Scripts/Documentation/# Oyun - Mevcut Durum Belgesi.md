Friendslop - Mevcut Durum Belgesi



1\. Ana Konsept (High-Level Concept)



&nbsp;   Asimetrik co-op bir oyundur. Sahnede tek bir karakter bedeni bulunur. Bir oyuncu (Host/Göz) bu bedenin kafasını, diğeri (Client/El) ise bedenin geri kalanını ve ellerini kontrol eder.



2\. Oyuncu Rolleri ve Kontrolleri



El Oyuncusu (Client)



&nbsp;   Sorumlulukları:



&nbsp;       Ana karakter bedeninin yürümesinden (WASD) sorumludur.



&nbsp;       Ana karakter bedeninin sağa/sola dönüşünden (Mouse X) sorumludur.



&nbsp;       Fiziksel elin hassas hareketlerinden, obje alıp bırakmaktan (Grab tuşu) sorumludur.



&nbsp;   Görüşü:



&nbsp;       Dünyayı doğrudan görmez. Sadece elini ve özel efektleri (halüsinasyon vb.) gösteren kendine ait bir kamerası vardır.



    Yapısı:

 	Hand Player

 	|

 	|\_Hand

 	|    |\_HandHoldPoint

&nbsp;	|

 	|\_Camera Rig

&nbsp;	     |\_Hand Player CM (Outputs to `Channel01`)

 	|\_Eye Level

 	|\_Target Group



    Component'lar:

 	Hand Player -> Capsule Collider, Network Object, Network Transform, Character Controller, Player Input, Hand Controller, Role Manager

 	Hand -> Capsule Collider, Rigidbody, Hand Interactor

&nbsp;	Hand Player CM -> Cinemachine Camera

 	Target Group -> Cinemachine Target Group	



Göz Oyuncusu (Host)



&nbsp;   Sorumlulukları:



&nbsp;       Oyun dünyasında değil, ayrı bir "kontrol odasında" bulunan kendi avatarını (EyeAvatar) yönetir.



&nbsp;       Bu odadaki kollarla (levers) etkileşime girerek, ana karakter bedeninin kafasının rotasyonunu (yukarı/aşağı/yanlara bakma) kontrol eder.



&nbsp;   Görüşü:



&nbsp;       Kendi avatarının birinci şahıs kamerasıyla control odasını görür.



&nbsp;       Kontrol odasındaki bir ekran (RenderTexture) üzerinden, ana karakter bedeninin gözünden dış dünyayı izler.



    Yapısı:

 	Eye Player 

 	|

 	|\_Hand

 	|    |\_HandHoldPoint

 	|

 	|\_Eye Level

 	|\_Target Group

 	|\_Camera Rig

 	     |\_Eye Player CM (Outputs to `Channel02`)



&nbsp;   Component'lar:

&nbsp;	Eye Player -> Capsule Collider, Network Object, Network Transform, Character Controller, Player Input, Eye Player Controller, Role Manager

&nbsp;	Hand -> Capsule Collider, Rigidbody

&nbsp;	Target Group -> Cinemachine Target Group

&nbsp;	Eye Player CM -> Cinemachine Camera

 Cameras

 &nbsp; Görevi: Ana hiyerarşide bulunur, prefablarde değil, Oyunda bir tane camera yerine farklı channel'lar üzerinden farklı görüntü alan iki adet camera'yı bulundurmak ve EyePlayer Feed'i HandPlayer a bir Eye Player Feed CM koymadan kontrol etmek.

 &nbsp; Child Objeler:
 &nbsp; 1\. Hand Player Camera (CM Brain)
 &nbsp; 2\. Eye Player Camera (CM Brain)
 &nbsp; 3\. Eye Player Feed Rig
                            |\_Eye Player Feed Cam (CM Brain)
                            |\_Eye Player Feed CM (Outputs to `Channel03`)


&nbsp;	

3\. Temel Mimari Kararları



&nbsp;   Ağ Yapısı: Host-Client modeli kullanılır.



&nbsp;   Oyuncu Oluşturma (Spawning): Bir ConnectionApprovalManager veya GameManager script'i, NetworkManager'ın bağlantı onayı mekanizmasını kullanarak Host için eyePlayerPF'i ve Client için handPlayerPF'i spawn eder.



&nbsp;   Etkileşim Sistemi: Fiziksel etkileşimler, elin üzerindeki HandInteractor script'i ve OnTriggerEnter ile yapılır. Interactable adında bir temel sınıf kullanılır.



4\. Kilit Script'lerin 



Lütfen projenizdeki bu script dosyalarının içeriğini açıp, kodların tamamını kopyalayarak ilgili başlığın altına yapıştırın.



GameManager.cs

C#

`using Unity.Netcode;

using UnityEngine;



public class GameManager : NetworkBehaviour

{

&nbsp;   \[Header("Prefabs")]

&nbsp;   \[SerializeField] private GameObject handPlayerPF;

&nbsp;   \[SerializeField] private GameObject eyePlayerPF;



&nbsp;   \[Header("Spawn Points")]

&nbsp;   \[SerializeField] private Transform handPlayerSpawnPoint;

&nbsp;   \[SerializeField] private Transform eyePlayerSpawnPoint;

&nbsp;   public override void OnNetworkSpawn()

&nbsp;   {

&nbsp;       if (!IsServer) return; //Bu kod sadece host için çalışır



&nbsp;       // 1. Ana karakter bedenini oluştur

&nbsp;       // Bu objenin sahibi sunucudur ve tüm client'lar tarafından görülür.

&nbsp;       GameObject handPlayerInstance = Instantiate(handPlayerPF, handPlayerSpawnPoint);

&nbsp;       handPlayerInstance.GetComponent<NetworkObject>().Spawn();



&nbsp;       // 2. Göz Oyuncusu'nun bedenini oluştur

&nbsp;       // Bu obje sadece Host için oluşturulur ve sahibi de odur.

&nbsp;       // Diğer Client'ların bunu görmesine gerek yok, bu yüzden Spawn(true) yerine

&nbsp;       // sadece Host'a özel spawn ediyoruz.

&nbsp;       GameObject eyePlayerInstance = Instantiate(eyePlayerPF, eyePlayerSpawnPoint);

&nbsp;       eyePlayerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);

&nbsp;   }

}

`

&nbsp;	



HandController.cs (El Oyuncusu'nun beden hareketini yöneten script)

C#



`using UnityEngine;
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

`



EyePlayerController.cs (Göz Oyuncusu'nun odadaki hareketini yöneten script)

C#



`using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
using Unity.VisualScripting;
#endif


#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[RequireComponent(typeof(CharacterController))]
public class EyePlayerController : NetworkBehaviour
{
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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        // İmleci Kitle
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
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
}

`



LeverController.cs (Göz Oyuncusu'nun kullandığı kolun script'i)

C#



`using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public class LeverController : NetworkBehaviour
{
    [Header("Lever Type")]
    [Tooltip("Choose what this lever controls")]
    public LeverControlType controlType = LeverControlType.Horizontal;

    [Header("Lever Settings")]
    [Tooltip("Kolun dönebileceği minimum açı.")]
    [SerializeField] private float minAngle = -45f;

    [Tooltip("Kolun dönebileceği maksimum açı.")]
    [SerializeField] private float maxAngle = 45f;

    [Tooltip("Dönülecek eksen (Lever görsel rotasyonu için).")]
    [SerializeField] private Vector3 leverRotationAxis = Vector3.right;

    [Tooltip("Mouse input'un hassasiyeti.")]
    [SerializeField] private float inputSensitivity = 1f;

    public enum LeverControlType
    {
        Horizontal,  // Controls left/right head movement (Y-axis)
        Vertical,    // Controls up/down head movement (X-axis)
        Roll,        // Controls roll head movement (Z-axis)
        Custom       // Uses the leverRotationAxis for both visual and head rotation
    }

    // Network synchronized angle
    private NetworkVariable<float> networkAngle = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private float currentAngle = 0f;
    private HeadController headController;
    private bool isGrabbed = false;

    public override void OnNetworkSpawn()
    {
        // Subscribe to network angle changes
        networkAngle.OnValueChanged += OnAngleChanged;
        
        // Cache HeadController reference
        if (headController == null)
        {
            headController = FindFirstObjectByType<HeadController>();
        }
    }

    public override void OnNetworkDespawn()
    {
        networkAngle.OnValueChanged -= OnAngleChanged;
    }

    private void OnAngleChanged(float previousValue, float newValue)
    {
        currentAngle = newValue;
        UpdateVisualRotation();
    }

    public void Grab()
    {
        isGrabbed = true;
        Debug.Log($"{controlType} Lever {name} grabbed");
    }

    public void Release()
    {
        isGrabbed = false;
        Debug.Log($"{controlType} Lever {name} released");
    }

    // Called by EyeInteractor when mouse input is detected
    public void UpdateRotation(float input)
    {
        if (!isGrabbed) return;

        // Send input to server for processing
        if (IsOwner || IsServer)
        {
            UpdateRotationServerRpc(input);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(float input)
    {
        // Process input on server with direction based on control type
        float processedInput = GetProcessedInput(input);
        float newAngle = currentAngle + processedInput;
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);
        
        // Update network variable (this will trigger OnAngleChanged on all clients)
        networkAngle.Value = newAngle;
        
        // Send head rotation update
        SendHeadRotationUpdate(newAngle);
    }

    private float GetProcessedInput(float input)
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return -input * inputSensitivity; // Normal direction for horizontal
            case LeverControlType.Vertical:
                return -input * inputSensitivity; // Inverted for more intuitive vertical control
            case LeverControlType.Roll:
                return -input * inputSensitivity; // Normal direction for roll
            case LeverControlType.Custom:
                return -input * inputSensitivity; // User can adjust via inputSensitivity sign
            default:
                return -input * inputSensitivity;
        }
    }

    private void UpdateVisualRotation()
    {
        Vector3 visualAxis = GetVisualRotationAxis();
        transform.localRotation = Quaternion.Euler(visualAxis * currentAngle);
    }

    private void SendHeadRotationUpdate(float angle)
    {
        if (headController != null)
        {
            switch (controlType)
            {
                case LeverControlType.Horizontal:
                    headController.SetHorizontalRotationServerRpc(angle);
                    break;
                case LeverControlType.Vertical:
                    headController.SetVerticalRotationServerRpc(angle);
                    break;
                case LeverControlType.Roll:
                    headController.SetRollRotationServerRpc(angle);
                    break;
                case LeverControlType.Custom:
                    headController.SetHeadRotationServerRpc(leverRotationAxis, angle);
                    break;
            }
        }
    }

    private Vector3 GetVisualRotationAxis()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return Vector3.forward; // Z-axis for horizontal lever visual
            case LeverControlType.Vertical:
                return Vector3.right;   // X-axis for vertical lever visual
            case LeverControlType.Roll:
                return Vector3.up;      // Y-axis for roll lever visual
            case LeverControlType.Custom:
                return leverRotationAxis;
            default:
                return Vector3.right;
        }
    }

    // Initialize the lever angle if needed
    [ServerRpc(RequireOwnership = false)]
    public void InitializeLeverServerRpc(float angle = 0f)
    {
        networkAngle.Value = Mathf.Clamp(angle, minAngle, maxAngle);
    }

    // Get current angle for debugging or external systems
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // Check if this lever is currently being interacted with
    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    // Utility method to get recommended angle ranges for different lever types
    [ContextMenu("Apply Recommended Settings")]
    public void ApplyRecommendedSettings()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                minAngle = -45f;
                maxAngle = 45f;
                leverRotationAxis = Vector3.forward;
                break;
            case LeverControlType.Vertical:
                minAngle = -30f;
                maxAngle = 30f;
                leverRotationAxis = Vector3.right;
                break;
            case LeverControlType.Roll:
                minAngle = -30f;
                maxAngle = 30f;
                leverRotationAxis = Vector3.up;
                break;
            case LeverControlType.Custom:
                // Keep current settings
                break;
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    // Get the control type for external systems
    public LeverControlType GetControlType()
    {
        return controlType;
    }

    // Get a description of what this lever controls
    public string GetControlDescription()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return "Left/Right Head Rotation";
            case LeverControlType.Vertical:
                return "Up/Down Head Rotation";
            case LeverControlType.Roll:
                return "Head Roll Rotation";
            case LeverControlType.Custom:
                return "Custom Axis Rotation";
            default:
                return "Unknown";
        }
    }
}

`



HeadController.cs (Ana bedenin kafa dönüşünü yöneten script)

C#



`using UnityEngine;
using Unity.Netcode;

public class HeadController : NetworkBehaviour
{   
    [SerializeField] private Transform headToRotate;
    
    // Network variables for separate axis control
    private NetworkVariable<float> horizontalAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> verticalAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> rollAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        horizontalAngle.OnValueChanged += OnRotationChanged;
        verticalAngle.OnValueChanged += OnRotationChanged;
        rollAngle.OnValueChanged += OnRotationChanged;
    }

    public override void OnNetworkDespawn()
    {
        horizontalAngle.OnValueChanged -= OnRotationChanged;
        verticalAngle.OnValueChanged -= OnRotationChanged;
        rollAngle.OnValueChanged -= OnRotationChanged;
    }

    private void OnRotationChanged(float previousValue, float newValue)
    {
        UpdateHeadRotation();
    }

    private void UpdateHeadRotation()
    {
        if (headToRotate != null)
        {
            // Combine all three angles into final rotation
            headToRotate.localRotation = Quaternion.Euler(
                verticalAngle.Value,    // X axis (up/down)
                horizontalAngle.Value,  // Y axis (left/right)
                rollAngle.Value         // Z axis (roll)
            );
        }
    }

    // RPC'nin adını ve parametrelerini değiştiriyoruz
    [ServerRpc(RequireOwnership = false)]
    public void SetHeadRotationServerRpc(Vector3 axis, float targetAngle)
    {
        if (headToRotate != null)
        {
            // Update the appropriate axis based on input
            if (axis == Vector3.right) // Vertical rotation (up/down)
            {
                verticalAngle.Value = targetAngle;
            }
            else if (axis == Vector3.up) // Horizontal rotation (left/right)
            {
                horizontalAngle.Value = targetAngle;
            }
            else if (axis == Vector3.forward) // Roll rotation
            {
                rollAngle.Value = targetAngle;
            }
        }
    }

    // Convenience methods for direct axis control
    [ServerRpc(RequireOwnership = false)]
    public void SetHorizontalRotationServerRpc(float angle)
    {
        horizontalAngle.Value = angle;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetVerticalRotationServerRpc(float angle)
    {
        verticalAngle.Value = angle;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRollRotationServerRpc(float angle)
    {
        rollAngle.Value = angle;
    }

    // Get current rotation values for debugging or external systems
    public Vector3 GetCurrentRotation()
    {
        return new Vector3(verticalAngle.Value, horizontalAngle.Value, rollAngle.Value);
    }
}

`



HandInteractor.cs (Elin tutma/bırakma mantığını içeren script)

C#



`using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HandInteractor : MonoBehaviour
{
    [Header("Kinematic Grabbing Settings")]
    [Tooltip("Elin objeyi tutacağı nokta.")]
    [SerializeField] private Transform handHoldPoint;
    
    [Tooltip("Ne kadar uzağa erişebiliriz")]
    [SerializeField] private float grabRange = 1.5f;
    
    [Tooltip("Hangi layer'daki objeler tutulabilir")]
    [SerializeField] private LayerMask grabbableLayer = -1;
    
    [Tooltip("Fırlatma kuvveti çarpanı")]
    [SerializeField] private float throwForceMultiplier = 5f;
    
    [Tooltip("Bırakma/fırlatma hassasiyeti")]
    [SerializeField] private float velocityThreshold = 2f;
    
    [Tooltip("Hız takip örnek sayısı")]
    [SerializeField] private int velocitySamples = 5;

    // Current grabbed object
    private GameObject grabbedObject;
    private Rigidbody grabbedRigidbody;
    private Vector3 grabPointOffset;
    
    // Original object properties
    private bool originalKinematic;
    private bool originalGravity;
    private Transform originalParent;
    
    // Hand velocity tracking for throwing
    private List<Vector3> handPositions = new List<Vector3>();
    private List<float> handTimes = new List<float>();
    
    // Legacy interaction variables
    private GrabbableItem grabbableInRange;
    private PageTurnButton buttonInRange;
    private CabinetController cabinetInRange;

    private void Update()
    {
        // Track hand position for velocity calculation
        TrackHandVelocity();
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (grabbedObject == null)
            {
                // Try new grabbing system first
                TryGrabObject();
                
                // Fallback to legacy interactions
                if (grabbedObject == null)
                {
                    HandleLegacyInteractions();
                }
            }
        }
        else if (context.canceled)
        {
            if (grabbedObject != null)
            {
                ReleaseObject();
            }
        }
    }

    #region New Kinematic Grabbing System

    private void TryGrabObject()
    {
        // Find closest grabbable object
        GameObject targetObject = FindClosestGrabbableObject();
        
        if (targetObject != null)
        {
            // Get exact contact point via raycast
            Vector3 contactPoint = GetContactPoint(targetObject);
            
            if (contactPoint != Vector3.zero)
            {
                GrabObjectAtPoint(targetObject, contactPoint);
            }
        }
    }

    private GameObject FindClosestGrabbableObject()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(handHoldPoint.position, grabRange, grabbableLayer);
        
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyObjects)
        {
            // Must have rigidbody to be grabbable
            if (col.GetComponent<Rigidbody>() == null) continue;
            
            float distance = Vector3.Distance(handHoldPoint.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = col.gameObject;
            }
        }
        
        Debug.Log(closest != null ? $"Found closest object: {closest.name} at distance {closestDistance}" 
                                  : "No grabbable objects found in range");
        
        return closest;
    }

    private Vector3 GetContactPoint(GameObject targetObject)
    {
        // Raycast from hand to object for exact contact point
        Vector3 direction = (targetObject.transform.position - handHoldPoint.position).normalized;
        
        if (Physics.Raycast(handHoldPoint.position, direction, out RaycastHit hit, grabRange, grabbableLayer))
        {
            if (hit.collider.gameObject == targetObject)
            {
                Debug.Log($"Contact point found at: {hit.point}");
                
                // Draw debug visualization
                Debug.DrawRay(handHoldPoint.position, direction * hit.distance, Color.green, 1f);
                
                return hit.point;
            }
        }
        
        Debug.Log("No valid contact point found");
        return Vector3.zero;
    }

    private void GrabObjectAtPoint(GameObject targetObject, Vector3 contactPoint)
    {
        grabbedObject = targetObject;
        grabbedRigidbody = targetObject.GetComponent<Rigidbody>();
        
        // Store original properties
        originalKinematic = grabbedRigidbody.isKinematic;
        originalGravity = grabbedRigidbody.useGravity;
        originalParent = grabbedObject.transform.parent;
        
        // Calculate grab point offset in hand's local space
        grabPointOffset = handHoldPoint.InverseTransformPoint(contactPoint);
        
        // Make object kinematic and parent it to hand
        grabbedRigidbody.isKinematic = true;
        grabbedRigidbody.useGravity = false;
        grabbedObject.transform.SetParent(handHoldPoint);
        
        // Position object so contact point aligns with hand
        Vector3 desiredObjectPosition = handHoldPoint.position - (contactPoint - grabbedObject.transform.position);
        grabbedObject.transform.position = desiredObjectPosition;
        
        Debug.Log($"Grabbed {targetObject.name} at contact point {contactPoint}");
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;
        
        // Calculate release velocity
        Vector3 releaseVelocity = CalculateHandVelocity();
        bool shouldThrow = releaseVelocity.magnitude > velocityThreshold;
        
        // Restore original properties
        grabbedRigidbody.isKinematic = originalKinematic;
        grabbedRigidbody.useGravity = originalGravity;
        grabbedObject.transform.SetParent(originalParent);
        
        // Apply momentum if throwing
        if (shouldThrow && !originalKinematic)
        {
            grabbedRigidbody.linearVelocity = releaseVelocity * throwForceMultiplier;
            Debug.Log($"Threw {grabbedObject.name} with velocity: {releaseVelocity * throwForceMultiplier}");
        }
        else
        {
            // Just drop
            if (!originalKinematic)
            {
                grabbedRigidbody.linearVelocity = Vector3.zero;
                grabbedRigidbody.angularVelocity = Vector3.zero;
            }
            Debug.Log($"Dropped {grabbedObject.name}");
        }
        
        // Clear references
        grabbedObject = null;
        grabbedRigidbody = null;
    }

    #endregion

    #region Hand Velocity Tracking

    private void TrackHandVelocity()
    {
        // Add current position and time
        handPositions.Add(handHoldPoint.position);
        handTimes.Add(Time.time);
        
        // Remove old samples
        while (handPositions.Count > velocitySamples)
        {
            handPositions.RemoveAt(0);
            handTimes.RemoveAt(0);
        }
    }

    private Vector3 CalculateHandVelocity()
    {
        if (handPositions.Count < 2) return Vector3.zero;
        
        // Calculate average velocity over recent samples
        Vector3 totalVelocity = Vector3.zero;
        int velocityCount = 0;
        
        for (int i = 1; i < handPositions.Count; i++)
        {
            float deltaTime = handTimes[i] - handTimes[i - 1];
            if (deltaTime > 0)
            {
                Vector3 velocity = (handPositions[i] - handPositions[i - 1]) / deltaTime;
                totalVelocity += velocity;
                velocityCount++;
            }
        }
        
        return velocityCount > 0 ? totalVelocity / velocityCount : Vector3.zero;
    }

    #endregion

    #region Legacy Interaction System

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet))
        {
            cabinetInRange = cabinet;
            Debug.Log("Dolap alanına girildi: " + cabinet.gameObject.name);
        }
        else if (other.TryGetComponent(out GrabbableItem item))
        {
            grabbableInRange = item;
            Debug.Log("Yerden alınabilir obje algılandı: " + item.gameObject.name);
        }
        else if (other.TryGetComponent(out PageTurnButton button))
        {
            buttonInRange = button;
            Debug.Log("Defter butonu algılandı: " + button.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet) && cabinetInRange == cabinet)
        {
            cabinetInRange = null;
            Debug.Log("Dolap alanından çıkıldı.");
        }
        else if (other.TryGetComponent(out GrabbableItem item) && grabbableInRange == item)
        {
            grabbableInRange = null;
            Debug.Log("Yerden alınabilir obje menzilden çıktı.");
        }
        else if (other.TryGetComponent(out PageTurnButton button) && buttonInRange == button)
        {
            buttonInRange = null;
            Debug.Log("Defter butonu menzilden çıktı.");
        }
    }

    private void HandleLegacyInteractions()
    {
        if (buttonInRange != null)
        {
            buttonInRange.Interact(this);
        }
        else if (grabbableInRange != null)
        {
            grabbableInRange.Interact(this);
        }
    }

    // Legacy method for other scripts
    public void HoldItem(GameObject item)
    {
        if (grabbedObject != null) return;
        
        grabbedObject = item;
        grabbedRigidbody = item.GetComponent<Rigidbody>();
        
        if (grabbedRigidbody != null)
        {
            originalKinematic = grabbedRigidbody.isKinematic;
            originalGravity = grabbedRigidbody.useGravity;
            grabbedRigidbody.isKinematic = true;
            grabbedRigidbody.useGravity = false;
        }
        
        originalParent = item.transform.parent;
        item.transform.SetParent(handHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Holding {item.name} (legacy mode)");
    }

    #endregion

    #region Utility Methods

    public bool IsHoldingSomething()
    {
        return grabbedObject != null;
    }

    public GameObject GetHeldObject()
    {
        return grabbedObject;
    }

    public void ForceRelease()
    {
        if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw grab range
        if (handHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handHoldPoint.position, grabRange);
        }
    }
}`



Interactable.cs (Temel etkileşim sınıfı)

C#



`// Interactable.cs

using UnityEngine;



public abstract class Interactable : MonoBehaviour

{

&nbsp;   public string interactionPrompt = "Etkileime Gir";



&nbsp;   // Parametreyi PlayerInteractor'dan HandInteractor'a eviriyoruz.

&nbsp;   public abstract void Interact(HandInteractor interactor);

}`



CabinetController.cs (Dolapların mantığı)

C#



`// CabinetController.cs (SAĞLAM VE NİHAİ VERSİYON)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Contents")]
    [Tooltip("Bu dolabın içinde görünecek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> availableIngredients;

    [Tooltip("Her malzemenin duracağı yerlerin listesi. Sayısı malzeme listesiyle aynı olmalı.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    // Bu dolabın ana etkileşim alanı
    private Collider interactionTrigger;

    private void Awake()
    {
        // Dolabın üzerindeki ana Collider'ı al ve Trigger moduna ayarla
        interactionTrigger = GetComponent<Collider>();
        if (interactionTrigger != null)
        {
            interactionTrigger.isTrigger = true;
        }
    }

    private void Start()
    {
        CreateDisplayItems();
    }

    // Vitrin malzemelerini oluşturan metot
    private void CreateDisplayItems()
    {
        if (availableIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolabında malzeme sayısı ile spawn noktası sayısı uyuşmuyor!");
            return;
        }

        for (int i = 0; i < availableIngredients.Count; i++)
        {
            Ingredient ingredient = availableIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            GameObject displayItem = Instantiate(ingredient.prefab, spawnPoint.position, spawnPoint.rotation);
            displayItem.transform.SetParent(spawnPoint);
            
            // Convert LayerMask to layer index (get the first set bit)
            int layerIndex = 0;
            int layerMaskValue = ingredient.layerMask.value;
            while (layerMaskValue > 1)
            {
                layerMaskValue >>= 1;
                layerIndex++;
            }
            displayItem.gameObject.layer = layerIndex;
            
            // --- UPDATED PHYSICS SETUP FOR GRABBING ---

            // 1. Setup Rigidbody for physics-based grabbing
            Rigidbody itemRb = displayItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                // Make it kinematic and disable gravity so it stays in place as display
                itemRb.isKinematic = true;
                itemRb.useGravity = false;
            }

            // 2. Keep collider enabled but make it a trigger for display items
            // This allows raycast detection while preventing physics interference
            Collider itemCollider = displayItem.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;  // Keep enabled for raycast detection
                itemCollider.isTrigger = false;  // Not trigger so raycast can hit it
            }
            
            // 3. Add a tag to identify this as a display item (optional)
            displayItem.tag = "Interactable";
            // --- UPDATED SECTION END ---
        }
    }

    // HandInteractor bu metodu çağırarak bir item talep edecek
    public void RequestItem(HandInteractor interactor)
    {
        if (availableIngredients.Count == 0) return;

        // Elin pozisyonuna en yakın olan malzemeyi bul
        Ingredient closestIngredient = GetClosestIngredientTo(interactor.transform.position);

        if (closestIngredient != null)
        {
            Debug.Log($"{closestIngredient.ingredientName} için klonlama talebi alındı.");
            GameObject clone = Instantiate(closestIngredient.prefab);
            
            // Set up the clone for physics-based grabbing
            SetupGrabbableClone(clone, closestIngredient);
            
            interactor.HoldItem(clone);
        }
    }
    
    // Set up cloned objects for proper physics-based grabbing
    private void SetupGrabbableClone(GameObject clone, Ingredient ingredient)
    {
        // Convert LayerMask to layer index for the grabbable clone
        int layerIndex = 0;
        int layerMaskValue = ingredient.layerMask.value;
        while (layerMaskValue > 1)
        {
            layerMaskValue >>= 1;
            layerIndex++;
        }
        clone.gameObject.layer = layerIndex;
        
        // Ensure the clone has proper physics setup
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        if (cloneRb == null)
        {
            cloneRb = clone.AddComponent<Rigidbody>();
        }
        
        // Make it dynamic (not kinematic) so it can be grabbed
        cloneRb.isKinematic = false;
        cloneRb.useGravity = true;
        cloneRb.mass = 1f; // Set reasonable mass
        
        // Ensure collider is enabled and not a trigger
        Collider cloneCollider = clone.GetComponent<Collider>();
        if (cloneCollider != null)
        {
            cloneCollider.enabled = true;
            cloneCollider.isTrigger = false;
        }
        
        // Add GrabbableItem component for legacy compatibility
        if (clone.GetComponent<GrabbableItem>() == null)
        {
            clone.AddComponent<GrabbableItem>();
        }
    }

    // Elin pozisyonuna en yakın spawn noktasını ve dolayısıyla malzemeyi bulan metot
    private Ingredient GetClosestIngredientTo(Vector3 handPosition)
    {
        float closestDistance = float.MaxValue;
        Ingredient closestIngredient = null;

        for (int i = 0; i < itemDisplayPoints.Count; i++)
        {
            float distance = Vector3.Distance(handPosition, itemDisplayPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIngredient = availableIngredients[i];
            }
        }
        return closestIngredient;
    }
}`

RoleManager.cs (Rol ayrımı gerektiğinde bunu manage eden script)

`using Unity.Cinemachine;
using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

using UnityEngine.InputSystem;


public class RoleManager : NetworkBehaviour
{
    [Header("Managed Component")]
    [Tooltip("Bu prefab'in input'unu yöneten bileşen")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Bu prefab'in ana karakter kontrolcüsü.")]
    [SerializeField] private MonoBehaviour characterControllerScript;

    [Header("Camera ayarlaması için")]
    public bool isHandPlayer;
    [SerializeField] private CinemachineCamera eyePlayerFeedCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerInput != null) playerInput.enabled = false;
            if (characterControllerScript != null) characterControllerScript.enabled = false;
        }
        if (eyePlayerFeedCamera == null) eyePlayerFeedCamera = GameObject.FindGameObjectWithTag("EyePlayer Feed CM").GetComponent<CinemachineCamera>();
        if (isHandPlayer) eyePlayerFeedCamera.Follow = this.gameObject.transform;
    }
}`

PlayerSetupManager.cs
`
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerSetupManager : NetworkBehaviour
{
    [Header("Player Type")]
    [SerializeField] private bool isEyePlayer = true;

    [Header("Camera Configuration")]
    [SerializeField] private Unity.Cinemachine.OutputChannels cameraOutputChannel;
    private GameObject eyePlayerCamera;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;


    public void Start()
    {
        // Camera setup moved to OnNetworkSpawn for proper network ownership handling
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable components for non-owners to prevent conflicts
            DisableNonOwnerComponents();
            return;
        }

        // Handle specific camera setup for local player only
        HandleLocalCameraSetup();
        
        SetupCameras();
        // Remove input setup - controllers handle their own input
        SetupNetworkOwnership();

        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] {(isEyePlayer ? "Eye" : "Hand")} Player setup complete for Client {OwnerClientId}");
        }
    }

    private void HandleLocalCameraSetup()
    {
        // Only affect cameras when this is a HandPlayer owner
        if (!isEyePlayer)
        {
            // Find Eye Player Camera only if this is the HandPlayer
            eyePlayerCamera = GameObject.FindGameObjectWithTag("Eye Player Camera");
            
            if (eyePlayerCamera != null) 
            {
                eyePlayerCamera.SetActive(false);
                if (enableDebugLogs)
                {
                    Debug.Log($"[PlayerSetup] Local HandPlayer (Client {OwnerClientId}) disabled Eye Player Camera");
                }
            }
        }
    }
    
    private void DisableNonOwnerComponents()
    {
        // Don't disable PlayerInput - controllers handle their own input
        
        // Disable cameras for non-owners
        var cameras = GetComponentsInChildren<Camera>();
        foreach (var cam in cameras)
        {
            cam.enabled = false;
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Disabled camera '{cam.name}' for non-owner");
            }
        }
        
        // Disable virtual cameras for non-owners (Unity 6 Cinemachine)
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        foreach (var vcam in virtualCameras)
        {
            vcam.enabled = false;
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Disabled virtual camera '{vcam.name}' for non-owner");
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] Disabled {cameras.Length} cameras and {virtualCameras.Length} virtual cameras for non-owner of {(isEyePlayer ? "Eye" : "Hand")} Player");
        }
    }
    
    private void SetupCameras()
    {
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] Found {virtualCameras.Length} cameras on {(isEyePlayer ? "Eye" : "Hand")} Player");
        }
        
        foreach (var vcam in virtualCameras)
        {
            vcam.OutputChannel = cameraOutputChannel;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Set {(isEyePlayer ? "Eye" : "Hand")} Player camera '{vcam.name}' to output channel {cameraOutputChannel}");
            }
        }
        
        // Also set up regular cameras if any
        var cameras = GetComponentsInChildren<Camera>();
        foreach (var cam in cameras)
        {
            // Enable the camera for the owner
            cam.enabled = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Enabled camera '{cam.name}' for {(isEyePlayer ? "Eye" : "Hand")} Player owner");
            }
        }
    }
    
    // Input system setup removed - controllers handle their own input
    // private void SetupInputSystem() { ... }
    
    private void SetupNetworkOwnership()
    {
        // Ensure proper controller setup based on player type
        var eyeController = GetComponent<EyePlayerController>();
        var handController = GetComponent<HandController>();
        
        if (isEyePlayer)
        {
            if (eyeController == null)
            {
                Debug.LogError("[PlayerSetup] Eye Player prefab missing EyePlayerController!");
            }
            if (handController != null)
            {
                handController.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log("[PlayerSetup] Disabled HandController on Eye Player prefab");
                }
            }
        }
        else
        {
            if (handController == null)
            {
                Debug.LogError("[PlayerSetup] Hand Player prefab missing HandController!");
            }
            if (eyeController != null)
            {
                eyeController.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log("[PlayerSetup] Disabled EyePlayerController on Hand Player prefab");
                }
            }
        }
    }
    
    /// <summary>
    /// Call this method to manually reconfigure the player (useful for testing)
    /// </summary>
    [ContextMenu("Reconfigure Player")]
    public void ReconfigurePlayer()
    {
        if (IsOwner)
        {
            SetupCameras();
            // SetupInputSystem(); // Removed - controllers handle their own input
            SetupNetworkOwnership();
        }
    }
    
    /// <summary>
    /// Validate camera setup and output channel configuration
    /// </summary>
    [ContextMenu("Validate Camera Setup")]
    public void ValidateCameraSetup()
    {
        Debug.Log($"[PlayerSetup] === Camera Validation for {(isEyePlayer ? "Eye" : "Hand")} Player ===");
        Debug.Log($"[PlayerSetup] Configured Output Channel: {cameraOutputChannel}");
        Debug.Log($"[PlayerSetup] Is Owner: {IsOwner}");
        
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        Debug.Log($"[PlayerSetup] Found {virtualCameras.Length} virtual cameras:");
        
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            var vcam = virtualCameras[i];
            Debug.Log($"[PlayerSetup]   {i + 1}. '{vcam.name}' - Output Channel: {vcam.OutputChannel}, Enabled: {vcam.enabled}");
        }
        
        var cameras = GetComponentsInChildren<Camera>();
        Debug.Log($"[PlayerSetup] Found {cameras.Length} regular cameras:");
        
        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            Debug.Log($"[PlayerSetup]   {i + 1}. '{cam.name}' - Enabled: {cam.enabled}");
        }
        
        Debug.Log($"[PlayerSetup] === End Camera Validation ===");
    }
}`

GameManager.cs

`using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject handPlayerPF;
    [SerializeField] private GameObject eyePlayerPF;

    [Header("Spawn Points")]
    [SerializeField] private Transform handPlayerSpawnPoint;
    [SerializeField] private Transform eyePlayerSpawnPoint;

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only server handles spawning

        // Handle player spawning based on client connection
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Spawn host player immediately as Eye Player
        SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId, true);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return; // Skip host

        // First client becomes Hand Player
        SpawnPlayerForClient(clientId, false);
    }

    private void SpawnPlayerForClient(ulong clientId, bool isHost)
    {
        GameObject playerPrefab;
        Transform spawnPoint;

        if (isHost)
        {
            // Host = Eye Player
            playerPrefab = eyePlayerPF;
            spawnPoint = eyePlayerSpawnPoint;
            Debug.Log($"[GameManager] Spawning Eye Player for Host (Client {clientId})");
        }
        else
        {
            // Client = Hand Player
            playerPrefab = handPlayerPF;
            spawnPoint = handPlayerSpawnPoint;
            Debug.Log($"[GameManager] Spawning Hand Player for Client {clientId}");
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        
        // Spawn with proper ownership
        networkObject.SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
`
EyeInteractor.cs
`using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public class EyeInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [Tooltip("Kolun hassasiyeti - Yatay fare hareketi ile kontrol edilir")]
    [SerializeField] private float leverSensitivity = 2f;

    [Header("Component References")]
    [Tooltip("Etkileşim ışınının başlayacağı nokta. Genellikle PF'in kamera objesi")]
    [SerializeField] private Transform raycastOrigin;

    private LeverController leverInRange;
    private LeverController controlledLever;

    private void Update()
    {
        // Safety check for input system
        if (Mouse.current == null) return;

        if (controlledLever == null)
        {
            FindLever();
        }
        else
        {
            float mouseXInput = Mouse.current.delta.x.ReadValue();
            controlledLever.UpdateRotation(mouseXInput * leverSensitivity * Time.deltaTime);
        }
    }

    private void FindLever()
    {
        if (raycastOrigin == null) return;

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            leverInRange = hit.collider.GetComponent<LeverController>();
            
            if (leverInRange != null)
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
            }
        }
        else
        {
            leverInRange = null;
            CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        try
        {
            if (controlledLever != null)
            {
                controlledLever.Release();
                controlledLever = null;
                Debug.Log("Lever released.");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else if (leverInRange != null)
            {
                controlledLever = leverInRange;
                controlledLever.Grab();
                Debug.Log($"{controlledLever.GetControlDescription()} lever grabbed: {controlledLever.name}");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Grabbed);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnInteract: " + e.Message);
        }
    }
}
`

NotebookController.cs
`using UnityEngine;

public class NotebookController : MonoBehaviour
{

    [SerializeField] private GameObject[] pages;
    private int currentPageIndex = 0;

    void Start()
    {
        ShowPage(0);
    }

    public void ShowPage(int pageIndex)
    {
        currentPageIndex = Mathf.Clamp(pageIndex, 0, pages.Length - 1);
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
    }

    public void TurnToNextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    public void TurnToPreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }
}`

PageTurnButton.cs
`using UnityEngine;

public class PageTurnButton : Interactable
{
    [SerializeField] private NotebookController notebookController;
    [SerializeField] private bool isNextButton;

    public override void Interact(HandInteractor interactor)
    {
        if (notebookController == null) return;

        if (isNextButton)
        {
            notebookController.TurnToNextPage();
        }
        else
        {
            notebookController.TurnToPreviousPage();
        }
    }
}
`
RecipeValidator.cs
`using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class RecipeValidator
{
    public static bool ValidateRecipe(Recipe targetRecipe, List<Ingredient> submittedIngredients)
    {
        if (targetRecipe == null || targetRecipe.requiredIngredients == null || submittedIngredients == null)
        {
            Debug.LogError("Tarif veya malzeme listesi NULL olamaz.");
            return false;
        }

        if (targetRecipe.requiredIngredients.Count != submittedIngredients.Count)
        {
            return false;
        }

        var checklist = new List<Ingredient>(targetRecipe.requiredIngredients);

        foreach (var submittedIngredient in submittedIngredients)
        {
            if (checklist.Contains(submittedIngredient))
            {
                checklist.Remove(submittedIngredient);
            }
            else
            {
                return false;
            }
        }

        return checklist.Count == 0;
    }
}`

Ingredient.cs
`using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
    public GameObject prefab;
    public bool isSafe;
    public float cookTime;
    public LayerMask layerMask;
}
`

IngredientSpawner.cs
`using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    
    public Recipe currentRecipe;
    public IngredientSpawnPoint[] spawnPoints;


    void Start()
    {
        SpawnIngredients();
    }

    public void SpawnIngredients()
    {
        foreach (Ingredient ingredient in currentRecipe.requiredIngredients)
        {
            // Metoda art�k ingredient'in ismini de�il, kendisini yolluyoruz.
            Transform targetSpawnPoint = GetSpawnPointForIngredient(ingredient);
            if (targetSpawnPoint != null)
            {
                Instantiate(ingredient.prefab, targetSpawnPoint.position, Quaternion.identity);
            }
        }
    }

    // Metodun parametresini 'Ingredient' tipine �eviriyoruz.
    private Transform GetSpawnPointForIngredient(Ingredient targetIngredient)
    {
        foreach (var sp in spawnPoints)
        {
            // Art�k 'targetIngredient' de�i�keni tan�ml� ve kar��la�t�rma yapabiliriz.
            if (sp.ingredient == targetIngredient)
                return sp.spawnPoint;
        }
        return null;
    }


}

[System.Serializable]
public class IngredientSpawnPoint
{
    public Ingredient ingredient;
    public Transform spawnPoint;
}`

Recipe.cs
`using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> requiredIngredients;
    public Sprite recipeImage;
    [TextArea]
    public string cookingInstructions;
}
`