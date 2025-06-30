// PlayerRoleManager.cs (NÝHAÝ VE TAM HALÝ)
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private FPController fpController; // Deðiþiklik: Artýk FPController kullanýyoruz
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
        if (IsHost) // Göz Oyuncusu ise...
        {
            Debug.Log("Rol Atandý: GÖZ OYUNCUSU (Host)");
            cameraSystemObject.SetActive(true);
            fpController.enabled = true; // Deðiþiklik: FPController'ý aktive et

            handController.enabled = true;
            handInteractor.enabled = true;
        }
        else // El Oyuncusu ise...
        {
            Debug.Log("Rol Atandý: EL OYUNCUSU (Client)");
            cameraSystemObject.SetActive(false);
            fpController.enabled = false; // Deðiþiklik: FPController'ý devre dýþý býrak

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