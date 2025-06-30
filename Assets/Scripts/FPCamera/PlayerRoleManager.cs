using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private HandInteractor handInteractor;

    [Header("Camera System")]
    [SerializeField] private GameObject handPlayerCameraObject;
    [SerializeField] private GameObject eyePlayerVCamObject;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (handPlayerCameraObject != null) handPlayerCameraObject.SetActive(false);
            if (eyePlayerVCamObject != null) eyePlayerVCamObject.SetActive(false);
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
        if (IsHost) // Göz Oyuncusu
        {
            handController.enabled = false;
            handInteractor.enabled = false;
            handPlayerCameraObject.SetActive(false);
            eyePlayerVCamObject.SetActive(true);
        }
        else // El Oyuncusu
        {
            handController.enabled = true;
            handInteractor.enabled = true;
            handPlayerCameraObject.SetActive(true);
            eyePlayerVCamObject.SetActive(false);
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}