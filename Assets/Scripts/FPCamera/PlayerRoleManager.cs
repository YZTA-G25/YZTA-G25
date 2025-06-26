using Unity.Netcode;
using UnityEngine;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private FPController fpController;
    [SerializeField] private FPCamera fpCamera;
    [SerializeField] private GameObject cameraSystem; // Kamera objesini içeren parent obje

    public override void OnNetworkSpawn()
    {
        // Bu kodun sadece bu bilgisayardaki oyuncu için çalýþmasýný saðla
        if (!IsOwner)
        {
            // Eðer bu nesne bize ait deðilse, kamerasýný kapatalým ki
            // diðer oyuncunun gözünden görmeyelim.
            if (cameraSystem != null)
            {
                cameraSystem.SetActive(false);
            }
            return;
        }

        // Rol atamasýný yap
        AssignRole();
    }

    private void AssignRole()
    {
        // Eðer bu oyuncu HOST ise, Göz Oyuncusu'dur.
        if (IsHost)
        {
            Debug.Log("Assigning Role: EYE PLAYER (Host)");

            // Göz Oyuncusu'nun bileþenlerini aktive et
            if (fpController) fpController.enabled = true;
            if (fpCamera) fpCamera.enabled = true;

            // El Oyuncusu'nun bileþenlerini devre dýþý býrak
            if (handController) handController.enabled = false;
        }
        // Eðer HOST deðilse, o zaman bir CLIENT'tir ve El Oyuncusu'dur.
        else
        {
            Debug.Log("Assigning Role: HAND PLAYER (Client)");

            // El Oyuncusu'nun bileþenlerini aktive et
            if (handController) handController.enabled = true;

            // Göz Oyuncusu'nun bileþenlerini devre dýþý býrak
            if (fpController) fpController.enabled = false;
            if (fpCamera) fpCamera.enabled = false;
        }
    }
}