using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [Tooltip("El Oyuncusu'nun kontrollerini y�neten script.")]
    [SerializeField] private HandController handController;

    [Tooltip("G�z Oyuncusu'nun kamera d�n���n� y�neten script.")]
    [SerializeField] private CameraController cameraController;

    [Tooltip("T�m inputlar� al�p ilgili controllere da��tan script.")]
    [SerializeField] private PlayerInputHandler playerInputHandler;

    [Tooltip("Unity'nin Input System bile�eni.")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Camera System")]
    [Tooltip("��inde Camera ve Cinemachine Brain olan ana kamera objesi.")]
    [SerializeField] private GameObject cameraSystemObject;

    // OnNetworkSpawn, obje a� �zerinde olu�tu�unda sadece bir kez �al���r.
    public override void OnNetworkSpawn()
    {
        // E�er bu script'in �zerinde oldu�u Player objesi B�ZE A�T DE��LSE (yani di�er oyuncunun kopyas�ysa):
        if (!IsOwner)
        {
            // Di�er oyuncunun g�z�nden g�rmemek i�in onun kamera sistemini kapat.
            if (cameraSystemObject != null) cameraSystemObject.SetActive(false);

            // Bu script'in gereksiz yere �al��mas�n� engelle.
            this.enabled = false;
            return;
        }

        // E�er obje B�ZE A�TSE, rol atamas�n� yap ve imleci ayarla.
        AssignRole();
        LockCursor();
    }

    private void AssignRole()
    {
        // E�er bu oyuncu oyunu kuran ki�i ise (HOST), o zaman G�Z OYUNCUSU'dur.
        if (IsHost)
        {
            Debug.Log("Rol Atand�: G�Z OYUNCUSU (Host)");

            // G�z Oyuncusu i�in gerekli bile�enleri A�:
            cameraSystemObject.SetActive(true);      // Kameras� g�rs�n
            playerInput.enabled = true;              // Inputlar� als�n
            playerInputHandler.enabled = true;       // Inputlar� da��ts�n
            cameraController.enabled = true;         // Kamera d�ns�n

            // El Oyuncusu'na ait bile�eni KAPAT:
            handController.enabled = false;
        }
        // E�er bu oyuncu oyuna sonradan kat�lan ki�i ise (CLIENT), o zaman EL OYUNCUSU'dur.
        else
        {
            Debug.Log("Rol Atand�: EL OYUNCUSU (Client)");

            // El Oyuncusu i�in gerekli bile�eni A�:
            handController.enabled = true;

            // G�z Oyuncusu'na ait bile�enleri ve g�rme yetene�ini KAPAT:
            cameraSystemObject.SetActive(false);      // Kameras� g�rmesin (ekran� kararacak)
            playerInput.enabled = false;
            playerInputHandler.enabled = false;
            cameraController.enabled = false;
        }
    }

    private void LockCursor()
    {
        // Rol� ne olursa olsun, oyuna giren oyuncunun imleci kilitlenir.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}