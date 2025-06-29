// PlayerRoleManager.cs (NÝHAÝ VE TAM HALÝ)
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
    [SerializeField] private HandInteractor handInteractor; // YENÝ: HandInteractor referansý eklendi.

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
            cameraController.enabled = true;

            // El Oyuncusu'nun kontrolcülerini KAPAT.
            handController.enabled = false;
            handInteractor.enabled = false; // YENÝ: HandInteractor'ý Göz Oyuncusu için kapatýyoruz.
        }
        else // El Oyuncusu ise...
        {
            Debug.Log("Rol Atandý: EL OYUNCUSU (Client)");
            cameraSystemObject.SetActive(false);
            cameraController.enabled = false;

            // El Oyuncusu'nun kontrolcülerini AÇ.
            handController.enabled = true;
            handInteractor.enabled = true; // YENÝ: HandInteractor'ý El Oyuncusu için açýyoruz.
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}