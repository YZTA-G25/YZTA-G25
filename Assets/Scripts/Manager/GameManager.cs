using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject handPlayerPF;
    [SerializeField] private GameObject eyePlayerPF;

    [Header("Spawn Points")]
    [SerializeField] private Transform handPlayerSpawnPoint;
    [SerializeField] private Transform eyePlayerSpawnPoint;

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Only server handles spawning

        // Handle player spawning based on client connection
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Spawn host player immediately as Eye Player
        SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId, true);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return; // Skip host

        // First client becomes Hand Player
        SpawnPlayerForClient(clientId, false);
    }

    private void SpawnPlayerForClient(ulong clientId, bool isHost)
    {
        GameObject playerPrefab;
        Transform spawnPoint;

        if (isHost)
        {
            // Host = Eye Player
            playerPrefab = eyePlayerPF;
            spawnPoint = eyePlayerSpawnPoint;
            Debug.Log($"[GameManager] Spawning Eye Player for Host (Client {clientId})");
        }
        else
        {
            // Client = Hand Player
            playerPrefab = handPlayerPF;
            spawnPoint = handPlayerSpawnPoint;
            Debug.Log($"[GameManager] Spawning Hand Player for Client {clientId}");
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        
        // Spawn with proper ownership
        networkObject.SpawnAsPlayerObject(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
