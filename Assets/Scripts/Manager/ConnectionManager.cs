using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConnectionManager : NetworkBehaviour
{
    [SerializeField] private InGameUIManager uiManager;
    [SerializeField] private float disconnectGracePeriod = 5f;

    public override void OnNetworkSpawn()
    {
        // Bu kýsým sunucu için çalýþýr: Bir client ayrýldýðýnda ne yapýlacaðý.
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect_Server;
        }

        // YENÝ EKLENEN KISIM:
        // Bu kýsým client için çalýþýr: Sunucudan atýldýðýnda ne yapýlacaðý.
        if (IsClient && !IsHost)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect_Client;
        }
    }

    // Sunucunun, bir client ayrýldýðýnda çalýþtýrdýðý metot.
    private void HandleClientDisconnect_Server(ulong clientId)
    {
        if (clientId == 0) return;
        Debug.Log($"Client {clientId} baðlantýsý koptu. Geri sayým baþlatýlýyor...");
        uiManager.ShowDisconnectPanel(disconnectGracePeriod);
        StartCoroutine(DisconnectCountdown());
    }

    // YENÝ EKLENEN METOT:
    // Client'ýn, sunucuyla baðlantýsý koptuðunda çalýþtýrdýðý metot.
    private void HandleDisconnect_Client(ulong clientId)
    {
        Debug.Log("Sunucuyla baðlantý koptu. Ana menüye dönülüyor.");

        // Kalýcý objeleri temizle
        if (SoundManager.Instance != null) Destroy(SoundManager.Instance.gameObject);
        if (ScoringManager.Instance != null) Destroy(ScoringManager.Instance.gameObject);

        // Ana menüye dön
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DisconnectCountdown()
    {
        yield return new WaitForSeconds(disconnectGracePeriod);
        Debug.Log("Süre doldu. Oturum sonlandýrýlýyor.");
        NetworkManager.Singleton.Shutdown();

        // Host ana menüye dönerken de kalýcý objeleri temizlemeli.
        if (SoundManager.Instance != null) Destroy(SoundManager.Instance.gameObject);
        if (ScoringManager.Instance != null) Destroy(ScoringManager.Instance.gameObject);

        SceneManager.LoadScene("MainMenu");
    }

    // Obje yok olduðunda event aboneliklerini kaldýrmak önemlidir.
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect_Server;
        }

        if (IsClient && !IsHost)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnect_Client;
        }
    }
}