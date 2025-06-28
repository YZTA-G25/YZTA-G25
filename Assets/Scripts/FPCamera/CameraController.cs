// CameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Oyuncunun yukarý/aþaðý bakacaðý kamera objesinin Transform'u.")]
    [SerializeField] private Transform cameraPitchController; // Inspector'dan 'PlayerCamera' objesini buraya atayacaðýz.

    [Header("Looking Settings")]
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float pitchLimit = 85f;

    private CharacterController characterController;
    private Vector2 lookInput;
    private float currentPitch = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Bu metodu dýþarýdan (örneðin bir Input okuyucu script'ten) çaðýracaðýz.
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (cameraPitchController == null) return;

        // Yatay Dönüþ (Yaw): Tüm oyuncu bedenini döndürür.
        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        // Dikey Dönüþ (Pitch): Sadece kamera objesini eðer.
        currentPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraPitchController.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }
}