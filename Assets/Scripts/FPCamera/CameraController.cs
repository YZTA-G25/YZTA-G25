// CameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform cameraPitchController;

    [Header("Looking Settings")]
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float pitchLimit = 85f;

    [Header("Movement Settings")] // YENÝ BÖLÜM
    [SerializeField] private float moveSpeed = 5f; // YENÝ

    private CharacterController characterController;
    private Vector2 lookInput;
    private Vector2 moveInput; // YENÝ: Hareket input'unu tutacak deðiþken
    private float currentPitch = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Dýþarýdan Look input'unu almak için
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    // YENÝ METOT: Dýþarýdan Move input'unu almak için
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void Update()
    {
        // Artýk hem rotasyonu hem de hareketi güncelliyoruz
        HandleRotation();
        HandleMovement(); // YENÝ
    }

    private void HandleRotation()
    {
        if (cameraPitchController == null) return;

        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        currentPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraPitchController.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }

    // YENÝ METOT: Karakteri hareket ettirmek için
    private void HandleMovement()
    {
        if (characterController == null) return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }
}