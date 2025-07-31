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

    /// <summary>
    /// Bu obje (GameManager) ağ üzerinde spawn olduğunda çalışır.
    /// Tüm oyuncu spawn etme mantığını sadece sunucu yönetir.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[GameManager] OnNetworkSpawn çağrıldı! IsServer: {IsServer}");

        // Eğer bu kod sunucuda çalışmıyorsa, hiçbir işlem yapma.
        if (!IsServer) return;

        // GameManager başladığı anda, o an bağlı olan TÜM client'ları döngüye al
        // ve her biri için bir oyuncu spawn et. Bu, zamanlama sorununu çözer.
        Debug.Log("[GameManager] Mevcut bağlı client'lar için oyuncular spawn ediliyor...");
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }

        // Oyuna sonradan katılan bir oyuncu olursa onu da spawn etmek için bu olayı dinle.
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    /// <summary>
    /// Obje ağdan kaldırıldığında, hafıza sızıntısını önlemek için event aboneliğini iptal et.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    /// <summary>
    /// Oyuna sonradan bağlanan bir client olursa bu metot tetiklenir.
    /// </summary>
    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"[GameManager] Yeni bir client bağlandı: {clientId}. Oyuncu spawn ediliyor...");
        SpawnPlayerForClient(clientId);
    }

    /// <summary>
    /// Verilen clientId için doğru oyuncu prefab'ını, doğru rolde spawn eder.
    /// </summary>
    private void SpawnPlayerForClient(ulong clientId)
    {
        // Bu client için zaten bir oyuncu objesi spawn edilmiş mi diye kontrol et.
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var networkedClient) && networkedClient.PlayerObject != null)
        {
            Debug.Log($"[GameManager] Client {clientId} için zaten bir oyuncu spawn edilmiş. Atlanıyor.");
            return;
        }

        // Rol belirleme: Host (sunucu olan) EyePlayer'dır, diğeri HandPlayer'dır.
        bool isHost = clientId == NetworkManager.Singleton.LocalClientId;
        GameObject playerPrefab = isHost ? eyePlayerPF : handPlayerPF;
        Transform spawnPoint = isHost ? eyePlayerSpawnPoint : handPlayerSpawnPoint;

        Debug.Log($"[GameManager] Client {clientId} için spawn işlemi hazırlanıyor. Rol: {(isHost ? "EyePlayer" : "HandPlayer")}");

        if (playerPrefab == null || spawnPoint == null)
        {
            Debug.LogError($"[GameManager] {(isHost ? "Eye" : "Hand")} Player prefab veya spawn noktası atanmamış! Spawn işlemi başarısız.");
            return;
        }

        // Prefab'ı sahnede oluştur.
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Oluşturulan objeyi, doğru client'ın sahibi olacağı şekilde ağ üzerinde spawn et.
        try
        {
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
            Debug.Log($"[GameManager] Client {clientId} için oyuncu başarıyla spawn edildi. NetworkObjectId: {networkObject.NetworkObjectId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] Client {clientId} için oyuncu spawn edilirken hata oluştu: {e.Message}");
            Destroy(playerInstance);
        }
    }
}