// PlayerInputHandler.cs (G�ncellenmi� Hali)
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // Art�k CameraController'a de�il, FPController'a referans tutuyoruz.
    private FPController _fpController;
    private HandInteractor _handInteractor;

    private void Awake()
    {
        // Referanslar� al�yoruz.
        _fpController = GetComponent<FPController>();
        _handInteractor = GetComponentInChildren<HandInteractor>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // E�er FPController aktifse, Look verisini ona yolla.
        if (_fpController != null && _fpController.enabled)
        {
            _fpController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // E�er FPController aktifse, Move verisini ona yolla.
        if (_fpController != null && _fpController.enabled)
        {
            _fpController.SetMoveInput(input);
        }

        // El oyuncusu aktifse hareketi ona yolla (Bu k�s�m HandController'da d�zenlenmeli)
        // if (_handController != null && _handController.enabled)
        // {
        //     _handController.SetMoveInput(input);
        // }
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (_handInteractor != null && _handInteractor.enabled)
        {
            _handInteractor.OnGrab(context);
        }
    }
}