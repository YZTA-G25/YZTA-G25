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

    [Header("Movement Settings")] // YEN� B�L�M
    [SerializeField] private float moveSpeed = 5f; // YEN�

    private CharacterController characterController;
    private Vector2 lookInput;
    private Vector2 moveInput; // YEN�: Hareket input'unu tutacak de�i�ken
    private float currentPitch = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // D��ar�dan Look input'unu almak i�in
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    // YEN� METOT: D��ar�dan Move input'unu almak i�in
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void Update()
    {
        // Art�k hem rotasyonu hem de hareketi g�ncelliyoruz
        HandleRotation();
        HandleMovement(); // YEN�
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

    // YEN� METOT: Karakteri hareket ettirmek i�in
    private void HandleMovement()
    {
        if (characterController == null) return;

        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }
}