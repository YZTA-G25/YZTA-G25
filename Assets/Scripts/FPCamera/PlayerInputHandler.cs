// PlayerInputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private CameraController cameraController;
    private HandController handController; // Ýleride gerekebilir diye referans

    private void Awake()
    {
        cameraController = GetComponent<CameraController>();
        handController = GetComponent<HandController>();
    }

    // PlayerInput bileþeninin "Player" event'lerinden "Look" eylemine baðlanýr.
    public void OnLook(InputAction.CallbackContext context)
    {
        if (cameraController != null)
        {
            cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    // PlayerInput bileþeninin "Player" event'lerinden "Move" eylemine baðlanýr.
    public void OnMove(InputAction.CallbackContext context)
    {
        // HandController'a hareket verisini bu þekilde yollayabiliriz.
        // Bunun için HandController'da public bir metot oluþturmak gerekir.
        // handController.SetMoveInput(context.ReadValue<Vector2>());
    }
}