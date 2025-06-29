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
        // Bu k�s�m zaten do�ru �al���yor
        if (cameraController != null && cameraController.enabled)
        {
            cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Yorum sat�rlar�n� kald�r�p kodu aktive ediyoruz
        Vector2 input = context.ReadValue<Vector2>();

        // E�er CameraController aktifse, hareketi ona yolla
        if (cameraController != null && cameraController.enabled)
        {
            cameraController.SetMoveInput(input);
        }

        // E�er HandController aktifse, hareketi ona yolla
        // (HandController'da da SetMoveInput ad�nda bir public metot olu�turman�z gerekir)
        if (handController != null && handController.enabled)
        {
            // handController.SetMoveInput(input);
        }
    }
}