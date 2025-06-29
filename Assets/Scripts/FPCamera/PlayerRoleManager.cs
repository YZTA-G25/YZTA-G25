// PlayerRoleManager.cs (N�HA� VE TAM HAL�)
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private HandInteractor handInteractor; // YEN�: HandInteractor referans� eklendi.

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
            cameraController.enabled = true;

            // El Oyuncusu'nun kontrolc�lerini KAPAT.
            handController.enabled = false;
            handInteractor.enabled = false; // YEN�: HandInteractor'� G�z Oyuncusu i�in kapat�yoruz.
        }
        else // El Oyuncusu ise...
        {
            Debug.Log("Rol Atand�: EL OYUNCUSU (Client)");
            cameraSystemObject.SetActive(false);
            cameraController.enabled = false;

            // El Oyuncusu'nun kontrolc�lerini A�.
            handController.enabled = true;
            handInteractor.enabled = true; // YEN�: HandInteractor'� El Oyuncusu i�in a��yoruz.
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}