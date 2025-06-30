using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private HandController handController;
    private HandInteractor handInteractor;

    private void Awake()
    {
        handController = GetComponent<HandController>();
        handInteractor = GetComponentInChildren<HandInteractor>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // E�er FPController aktifse, Move verisini ona yolla.
        if (handController != null && handController.enabled)
        {
            handController.SetMoveInput(input);
        }

        // El oyuncusu aktifse hareketi ona yolla (Bu k�s�m HandController'da d�zenlenmeli)
        // if (_handController != null && _handController.enabled)
        // {
        //     _handController.SetMoveInput(input);
        // }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (handInteractor != null && handInteractor.enabled)
        {
            handInteractor.OnGrab(context);
        }
    }
}