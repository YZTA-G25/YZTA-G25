using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [Tooltip("El Oyuncusu'nun kontrollerini yöneten script.")]
    [SerializeField] private HandController handController;

    [Tooltip("Göz Oyuncusu'nun kamera dönüþünü yöneten script.")]
    [SerializeField] private CameraController cameraController;

    [Tooltip("Tüm inputlarý alýp ilgili controllere daðýtan script.")]
    [SerializeField] private PlayerInputHandler playerInputHandler;

    [Tooltip("Unity'nin Input System bileþeni.")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Camera System")]
    [Tooltip("Ýçinde Camera ve Cinemachine Brain olan ana kamera objesi.")]
    [SerializeField] private GameObject cameraSystemObject;

    // OnNetworkSpawn, obje að üzerinde oluþtuðunda sadece bir kez çalýþýr.
    public override void OnNetworkSpawn()
    {
        // Eðer bu script'in üzerinde olduðu Player objesi BÝZE AÝT DEÐÝLSE (yani diðer oyuncunun kopyasýysa):
        if (!IsOwner)
        {
            // Diðer oyuncunun gözünden görmemek için onun kamera sistemini kapat.
            if (cameraSystemObject != null) cameraSystemObject.SetActive(false);

            // Bu script'in gereksiz yere çalýþmasýný engelle.
            this.enabled = false;
            return;
        }

        // Eðer obje BÝZE AÝTSE, rol atamasýný yap ve imleci ayarla.
        AssignRole();
        LockCursor();
    }

    private void AssignRole()
    {
        // Eðer bu oyuncu oyunu kuran kiþi ise (HOST), o zaman GÖZ OYUNCUSU'dur.
        if (IsHost)
        {
            Debug.Log("Rol Atandý: GÖZ OYUNCUSU (Host)");

            // Göz Oyuncusu için gerekli bileþenleri AÇ:
            cameraSystemObject.SetActive(true);      // Kamerasý görsün
            playerInput.enabled = true;              // Inputlarý alsýn
            playerInputHandler.enabled = true;       // Inputlarý daðýtsýn
            cameraController.enabled = true;         // Kamera dönsün

            // El Oyuncusu'na ait bileþeni KAPAT:
            handController.enabled = false;
        }
        // Eðer bu oyuncu oyuna sonradan katýlan kiþi ise (CLIENT), o zaman EL OYUNCUSU'dur.
        else
        {
            Debug.Log("Rol Atandý: EL OYUNCUSU (Client)");

            // El Oyuncusu için gerekli bileþeni AÇ:
            handController.enabled = true;

            // Göz Oyuncusu'na ait bileþenleri ve görme yeteneðini KAPAT:
            cameraSystemObject.SetActive(false);      // Kamerasý görmesin (ekraný kararacak)
            playerInput.enabled = false;
            playerInputHandler.enabled = false;
            cameraController.enabled = false;
        }
    }

    private void LockCursor()
    {
        // Rolü ne olursa olsun, oyuna giren oyuncunun imleci kilitlenir.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}