using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject handPlayerPF;
    [SerializeField] private GameObject eyePlayerPF;

    [Header("Spawn Points")]
    [SerializeField] private Transform handPlayerSpawnPoint;
    [SerializeField] private Transform eyePlayerSpawnPoint;

    // Bu metot, GameManager'ýn bulunduðu "Game" sahnesi yüklendiðinde çalýþýr.
    public override void OnNetworkSpawn()
    {
        // Bu kodun sadece sunucuda (Host'ta) çalýþmasýný saðlarýz. Oyuncularý yaratma görevi Host'a aittir.
        if (IsServer)
        {
            // O an oyuna baðlý olan tüm client'larýn ID'lerini döngüye al
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                // Eðer client ID'si Host'un ID'si ile aynýysa, bu oyuncu Host'tur.
                bool isHost = clientId == NetworkManager.Singleton.LocalClientId;
                // Her bir client için doðru oyuncu prefab'ýný spawn et
                SpawnPlayerForClient(clientId, isHost);
            }
        }
    }

    // Bu metot, her bir oyuncu için doðru prefab'ý doðru yerde yaratýr.
    private void SpawnPlayerForClient(ulong clientId, bool isHost)
    {
        GameObject playerPrefab;
        Transform spawnPoint;

        if (isHost)
        {
            // Host ise Göz Oyuncusu'nu yarat
            playerPrefab = eyePlayerPF;
            spawnPoint = eyePlayerSpawnPoint;
            Debug.Log($"[GameManager] Göz Oyuncusu, Host (Client {clientId}) için yaratýlýyor.");
        }
        else
        {
            // Client ise El Oyuncusu'nu yarat
            playerPrefab = handPlayerPF;
            spawnPoint = handPlayerSpawnPoint;
            Debug.Log($"[GameManager] El Oyuncusu, Client {clientId} için yaratýlýyor.");
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        // Yaratýlan objenin að üzerinde bir kimliði olmasýný saðla ve sahibini ata
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}