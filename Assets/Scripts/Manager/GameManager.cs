using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject handPlayerPF;
    [SerializeField] private GameObject eyePlayerPF;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            // Host'un kendisi için EyePlayer'ı spawn et
            SpawnPlayer(NetworkManager.Singleton.LocalClientId, eyePlayerPF);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SpawnPlayer(clientId, handPlayerPF);
        }
    }

    private void SpawnPlayer(ulong clientId, GameObject prefab)
    {
        GameObject playerInstance = Instantiate(prefab);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
