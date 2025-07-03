using System;
using System.ComponentModel.Design;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [Tooltip("Bu PF'daki ana kontrolcü script'i (EyePlayerController OR HandController).")]
    [SerializeField] private MonoBehaviour mainController;

    [Tooltip("Bu PF'daki input'u alan bileşin.")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Bu PF'da el etkileşimi varsa (HandInteractor).")]
    [SerializeField] private HandInteractor handInteractor;

    [Header("Camera To Disable For Others")]
    [Tooltip("Diğer oyuncuların görmemesi gereken kamera.")]
    [SerializeField] private GameObject ownedCamera;

    public override void OnNetworkSpawn()
    {
        // Eğer obje bizim değilse (başka bir oyuncunun kopyası ise)
        if (!IsOwner)
        {
            // onu disable et
            if (ownedCamera != null)
            {
                ownedCamera.SetActive(false);
            }
            this.enabled = false;
            return;
        }

        // Eğer bu obje bizimse aktive et
        ActivateMyControls();
        LockCursor();
    }

    private void ActivateMyControls()
    {
        // Inspector ile atadığımız tüm bileşenleri aktive et
        if (mainController != null)
        {
            mainController.enabled = true;
        }

        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        if (handInteractor != null)
        {
            handInteractor.enabled = true;
        }
        
        Debug.Log(gameObject.name + " için kontroller aktive edildi");
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}