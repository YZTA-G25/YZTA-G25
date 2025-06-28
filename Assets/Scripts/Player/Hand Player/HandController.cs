using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

// HandController, bir NetworkBehaviour'dur. Bu, onun
// ağ üzerinde bir kimliğe sahip olmasını sağlar.
public class HandController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float handSensitivity = 0.1f;
    [SerializeField] private float handVerticalSpeed = 2f;
    [SerializeField] private float handRotationSpeed = 45f;

    [Header("References")]
    [SerializeField] private Transform handTransform; // El pozisyonunu kontrol edeceğimiz obje
    [SerializeField] private CharacterController characterController; // Karakterin bedeni için

    private PlayerControls playerControls;
    private Vector2 moveInput;
    private Vector2 handMoveInput;
    private float handVerticalInput;
    private bool alternateModeActive;
    private bool grabActive;

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

        // Input'ları burada aktive et
        playerControls = new PlayerControls();
        playerControls.Player.Enable();

        // Her bir action için event'ler tanımlıyoruz
        playerControls.Player.Move.performed += OnMoveInput;
        playerControls.Player.Move.canceled += OnMoveInput;

        playerControls.Player.HandMove.performed += OnHandMoveInput;
        playerControls.Player.HandMove.canceled += OnHandMoveInput;

        playerControls.Player.HandVertical.performed += OnHandVerticalInput;
        playerControls.Player.HandVertical.canceled += OnHandVerticalInput;

        playerControls.Player.AlternateMode.performed += OnAlternateModeInput;
        playerControls.Player.AlternateMode.canceled += OnAlternateModeInput;

        playerControls.Player.Grab.performed += OnGrabInput;
        playerControls.Player.Grab.canceled += OnGrabInput;
    }

    // OnNetworkDespawn, obje ağdan kaldırıldığında çalışır.
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        // Input'ları devre dışı bırakarak hafıza sızıntısını önlüyoruz
        playerControls.Player.Disable();
    }

    // Her frame'de çalışacak olan ana güncelleme fonksiyonu
    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleHandControl();
    }

    #region Control Logic

    // Karakterin yürüme mantığı
    private void HandleMovement()
    {
        Vector3 _moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(transform.TransformDirection(_moveDirection) * moveSpeed * Time.deltaTime);

        // Normal mode'da karakterin dönmesi
        if (!alternateModeActive)
        {
            Vector3 _rotationAmount = new Vector3(0, handMoveInput.x, 0) * mouseSensitivity * Time.deltaTime;
            transform.Rotate(_rotationAmount, Space.Self);
        }
    }

    // El kontrol mantuğı
    // HandController.cs içindeki HandleHandControl metodu

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
            // --- DEĞİŞİKLİK BİTTİ ---
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
    }
    #endregion
}
