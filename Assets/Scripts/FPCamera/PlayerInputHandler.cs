// PlayerInputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private CameraController cameraController;
    private HandController handController;

    private void Awake()
    {
        cameraController = GetComponent<CameraController>();
        handController = GetComponent<HandController>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Bu kýsým zaten doðru çalýþýyor
        if (cameraController != null && cameraController.enabled)
        {
            cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Yorum satýrlarýný kaldýrýp kodu aktive ediyoruz
        Vector2 input = context.ReadValue<Vector2>();

        // Eðer CameraController aktifse, hareketi ona yolla
        if (cameraController != null && cameraController.enabled)
        {
            cameraController.SetMoveInput(input);
        }

        // Eðer HandController aktifse, hareketi ona yolla
        // (HandController'da da SetMoveInput adýnda bir public metot oluþturmanýz gerekir)
        if (handController != null && handController.enabled)
        {
            // handController.SetMoveInput(input);
        }
    }
}