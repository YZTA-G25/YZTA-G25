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
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; //Bu kod sadece host için çalışır

        // 1. Ana karakter bedenini oluştur
        // Bu objenin sahibi sunucudur ve tüm client'lar tarafından görülür.
        GameObject handPlayerInstance = Instantiate(handPlayerPF, handPlayerSpawnPoint);
        handPlayerInstance.GetComponent<NetworkObject>().Spawn();

        // 2. Göz Oyuncusu'nun bedenini oluştur
        // Bu obje sadece Host için oluşturulur ve sahibi de odur.
        // Diğer Client'ların bunu görmesine gerek yok, bu yüzden Spawn(true) yerine
        // sadece Host'a özel spawn ediyoruz.
        GameObject eyePlayerInstance = Instantiate(eyePlayerPF, eyePlayerSpawnPoint);
        eyePlayerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
    }
}
