// PlayerInputHandler.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private CameraController _cameraController;
    private HandInteractor _handInteractor; // El script'inin adýnýn bu olduðunu varsayýyoruz

    private void Awake()
    {
        _cameraController = GetComponent<CameraController>();
        // Hand objesindeki HandInteractor'ý bul. Hiyerarþiye göre düzenlenebilir.
        _handInteractor = GetComponentInChildren<HandInteractor>();
    }

    // Input Actions'dan "Look" eylemi geldiðinde çalýþýr.
    public void OnLook(InputAction.CallbackContext context)
    {
        if (_cameraController != null && _cameraController.enabled)
        {
            _cameraController.SetLookInput(context.ReadValue<Vector2>());
        }
    }

    // Input Actions'dan "Move" eylemi geldiðinde çalýþýr.
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        // Göz oyuncusu aktifse hareketi ona yolla
        if (_cameraController != null && _cameraController.enabled)
        {
            _cameraController.SetMoveInput(input);
        }

        // El oyuncusu aktifse hareketi ona yolla
        // (HandController'da da SetMoveInput adýnda bir public metot olmalý)
        // if (_handController != null && _handController.enabled)
        // {
        //     _handController.SetMoveInput(input);
        // }
    }

    // Input Actions'dan "Grab" eylemi geldiðinde çalýþýr.
    public void OnGrab(InputAction.CallbackContext context)
    {
        // Tutma iþlemini doðrudan eldeki HandInteractor'a yönlendir.
        if (_handInteractor != null && _handInteractor.enabled)
        {
            _handInteractor.OnGrab(context);
        }
    }
}