// CameraController.cs
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Sadece yukar�/a�a�� e�ilecek olan kamera objesi.")]
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

        // Yatay D�n�� (Yaw): T�m oyuncu bedenini (bu script'in oldu�u obje) d�nd�r�r.
        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        // Dikey D�n�� (Pitch): Sadece kamera objesini e�er.
        currentPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraPitchController.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }
}