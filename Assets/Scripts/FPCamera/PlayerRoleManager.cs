// PlayerRoleManager.cs (N�HA� VE TAM HAL�)
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private FPController fpController; // De�i�iklik: Art�k FPController kullan�yoruz
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private HandInteractor handInteractor;

    [Header("Camera System")]
    [SerializeField] private GameObject cameraSystemObject;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (cameraSystemObject != null) cameraSystemObject.SetActive(false);
            this.enabled = false;
            return;
        }

        playerInput.enabled = true;
        playerInputHandler.enabled = true;

        AssignRole();
        LockCursor();
    }

    private void AssignRole()
    {
        if (IsHost) // G�z Oyuncusu ise...
        {
            Debug.Log("Rol Atand�: G�Z OYUNCUSU (Host)");
            cameraSystemObject.SetActive(true);
            fpController.enabled = true; // De�i�iklik: FPController'� aktive et

            handController.enabled = true;
            handInteractor.enabled = true;
        }
        else // El Oyuncusu ise...
        {
            Debug.Log("Rol Atand�: EL OYUNCUSU (Client)");
            cameraSystemObject.SetActive(false);
            fpController.enabled = false; // De�i�iklik: FPController'� devre d��� b�rak

            handController.enabled = true;
            handInteractor.enabled = true;
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}