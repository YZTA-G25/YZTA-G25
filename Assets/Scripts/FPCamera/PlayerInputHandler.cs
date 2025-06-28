// PlayerInputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private CameraController cameraController;
    private HandController handController; // �leride gerekebilir diye referans

    private void Awake()
    {
        cameraController = GetComponent<CameraController>();
        handController = GetComponent<HandController>();
    }

    // PlayerInput bile�eninin "Player" event'lerinden "Look" eylemine ba�lan�r.
    public void OnLook(InputAction.CallbackContext context)
    {
        if (cameraController != null)
        {
            cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    // PlayerInput bile�eninin "Player" event'lerinden "Move" eylemine ba�lan�r.
    public void OnMove(InputAction.CallbackContext context)
    {
        // HandController'a hareket verisini bu �ekilde yollayabiliriz.
        // Bunun i�in HandController'da public bir metot olu�turmak gerekir.
        // handController.SetMoveInput(context.ReadValue<Vector2>());
    }
}