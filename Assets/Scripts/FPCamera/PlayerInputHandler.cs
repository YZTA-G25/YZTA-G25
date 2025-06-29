// PlayerInputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private CameraController _cameraController;
    private HandInteractor _handInteractor; // El script'inin ad�n�n bu oldu�unu varsay�yoruz

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
        // Hand objesindeki HandInteractor'� bul. Hiyerar�iye g�re d�zenlenebilir.
        _handInteractor = GetComponentInChildren<HandInteractor>();
    }

    // Input Actions'dan "Look" eylemi geldi�inde �al���r.
    public void OnLook(InputAction.CallbackContext context)
    {
        if (_cameraController != null && _cameraController.enabled)
        {
            _cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    // Input Actions'dan "Move" eylemi geldi�inde �al���r.
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // G�z oyuncusu aktifse hareketi ona yolla
        if (_cameraController != null && _cameraController.enabled)
        {
            _cameraController.SetMoveInput(input);
        }

        // El oyuncusu aktifse hareketi ona yolla
        // (HandController'da da SetMoveInput ad�nda bir public metot olmal�)
        // if (_handController != null && _handController.enabled)
        // {
        //     _handController.SetMoveInput(input);
        // }
    }

    // Input Actions'dan "Grab" eylemi geldi�inde �al���r.
    public void OnGrab(InputAction.CallbackContext context)
    {
        // Tutma i�lemini do�rudan eldeki HandInteractor'a y�nlendir.
        if (_handInteractor != null && _handInteractor.enabled)
        {
            _handInteractor.OnGrab(context);
        }
    }
}