// CameraController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Oyuncunun yukar�/a�a�� bakaca�� kamera objesinin Transform'u.")]
    [SerializeField] private Transform cameraPitchController; // Inspector'dan 'PlayerCamera' objesini buraya atayaca��z.

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

    // Bu metodu d��ar�dan (�rne�in bir Input okuyucu script'ten) �a��raca��z.
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

        // Yatay D�n�� (Yaw): T�m oyuncu bedenini d�nd�r�r.
        float yaw = lookInput.x * lookSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up, yaw);

        // Dikey D�n�� (Pitch): Sadece kamera objesini e�er.
        currentPitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraPitchController.localRotation = Quaternion.Euler(currentPitch, 0, 0);
    }
}