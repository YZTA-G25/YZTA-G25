using Unity.Netcode;
using UnityEngine;

public class PlayerRoleManager : NetworkBehaviour
{
    [Header("Role Components")]
    [SerializeField] private HandController handController;
    [SerializeField] private FPController fpController;
    [SerializeField] private FPCamera fpCamera;
    [SerializeField] private GameObject cameraSystem; // Kamera objesini i�eren parent obje

    public override void OnNetworkSpawn()
    {
        // Bu kodun sadece bu bilgisayardaki oyuncu i�in �al��mas�n� sa�la
        if (!IsOwner)
        {
            // E�er bu nesne bize ait de�ilse, kameras�n� kapatal�m ki
            // di�er oyuncunun g�z�nden g�rmeyelim.
            if (cameraSystem != null)
            {
                cameraSystem.SetActive(false);
            }
            return;
        }

        // Rol atamas�n� yap
        AssignRole();
    }

    private void AssignRole()
    {
        // E�er bu oyuncu HOST ise, G�z Oyuncusu'dur.
        if (IsHost)
        {
            Debug.Log("Assigning Role: EYE PLAYER (Host)");

            // G�z Oyuncusu'nun bile�enlerini aktive et
            if (fpController) fpController.enabled = true;
            if (fpCamera) fpCamera.enabled = true;

            // El Oyuncusu'nun bile�enlerini devre d��� b�rak
            if (handController) handController.enabled = false;
        }
        // E�er HOST de�ilse, o zaman bir CLIENT'tir ve El Oyuncusu'dur.
        else
        {
            Debug.Log("Assigning Role: HAND PLAYER (Client)");

            // El Oyuncusu'nun bile�enlerini aktive et
            if (handController) handController.enabled = true;

            // G�z Oyuncusu'nun bile�enlerini devre d��� b�rak
            if (fpController) fpController.enabled = false;
            if (fpCamera) fpCamera.enabled = false;
        }
    }
}