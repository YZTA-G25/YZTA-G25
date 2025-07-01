using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private HandController handController;
    private HandInteractor handInteractor;

    private void Awake()
    {
        handController = GetComponent<HandController>();
        handInteractor = GetComponentInChildren<HandInteractor>(); // El bir child object
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // HandController aktifse hareket verisi yolla
        if (handController != null && handController.enabled)
        {
            Vector2 input = context.ReadValue<Vector2>();
            handController.SetMoveInput(input);
        }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (handInteractor != null && handInteractor.enabled)
        {
            handInteractor.OnGrab(context);
        }
    }
}