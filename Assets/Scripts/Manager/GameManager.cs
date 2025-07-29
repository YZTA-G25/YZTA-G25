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
        Debug.Log($"[GameManager] OnNetworkSpawn called! IsServer: {IsServer}, LocalClientId: {NetworkManager.LocalClientId}");
        
        if (!IsServer) 
        {
            Debug.Log("[GameManager] Not server, skipping player spawning logic");
            return; // Only server handles spawning
        }

        Debug.Log("[GameManager] Setting up client connection callback");
        
        // Handle player spawning based on client connection
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Spawn host player immediately as Eye Player
        Debug.Log("[GameManager] About to spawn host player");
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
        Debug.Log($"[GameManager] SpawnPlayerForClient called for clientId: {clientId}, isHost: {isHost}");
        
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
            Debug.Log($"[GameManager] Spawning Hand Player for Client {clientId})");
        }

        // Validate the prefab has a NetworkObject component
        if (playerPrefab == null)
        {
            Debug.LogError($"[GameManager] {(isHost ? "Eye" : "Hand")} Player prefab is NULL! Cannot spawn player.");
            return;
        }
        
        if (playerPrefab.GetComponent<NetworkObject>() == null)
        {
            Debug.LogError($"[GameManager] {(isHost ? "Eye" : "Hand")} Player prefab is missing NetworkObject component! Cannot spawn network player.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError($"[GameManager] {(isHost ? "Eye" : "Hand")} Player spawn point is NULL! Cannot spawn player.");
            return;
        }

        Debug.Log($"[GameManager] Instantiating {(isHost ? "Eye" : "Hand")} Player at position {spawnPoint.position}");
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        
        if (networkObject == null)
        {
            Debug.LogError($"[GameManager] Instantiated {(isHost ? "Eye" : "Hand")} Player instance is missing NetworkObject component!");
            Destroy(playerInstance);
            return;
        }
        
        // Check NetworkManager state before spawning
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[GameManager] NetworkManager.Singleton is null! Cannot spawn player.");
            Destroy(playerInstance);
            return;
        }
        
        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("[GameManager] NetworkManager is not listening! Cannot spawn player.");
            Destroy(playerInstance);
            return;
        }
        
        // Check if the prefab is registered in NetworkManager
        var networkPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs;
        
        // Check if prefab is in the spawnable prefabs list
        if (networkPrefabs.Contains(playerPrefab))
        {
            Debug.Log($"[GameManager] {(isHost ? "Eye" : "Hand")} Player prefab is registered in NetworkManager ✓");
        }
        else
        {
            Debug.LogError($"[GameManager] {(isHost ? "Eye" : "Hand")} Player prefab is NOT registered in NetworkManager! Add it to the Network Prefabs list in NetworkManager.");
            Destroy(playerInstance);
            return;
        }
        
        // Spawn with proper ownership
        Debug.Log($"[GameManager] About to spawn {(isHost ? "Eye" : "Hand")} Player NetworkObject for Client {clientId}");
        
        try
        {
            networkObject.SpawnAsPlayerObject(clientId);
            Debug.Log($"[GameManager] Successfully called SpawnAsPlayerObject for Client {clientId}");
            Debug.Log($"[GameManager] NetworkObject state - IsSpawned: {networkObject.IsSpawned}, NetworkObjectId: {networkObject.NetworkObjectId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Failed to spawn player: {e.Message}\n{e.StackTrace}");
            Destroy(playerInstance);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}
