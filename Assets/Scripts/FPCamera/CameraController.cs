// CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Sadece yukarý/aþaðý eðilecek olan kamera objesi.")]
    [SerializeField] private Transform cameraPitchController;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float pitchLimit = 85f;

    private CharacterController characterController;
    private Vector2 lookInput;
    private Vector2 moveInput;
    private float currentPitch = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (characterController == null) return;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (cameraPitchController == null) return;

        // Yatay Dönüþ (Yaw): Tüm oyuncu bedenini (bu script'in olduðu obje) döndürür.
        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        // Dikey Dönüþ (Pitch): Sadece kamera objesini eðer.
        currentPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraPitchController.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }
}