using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneLoadManager : NetworkBehaviour
{
    // HATA BURADAYDI: Event'in adý "OnGameSceneLoaded" olmalý ve Action<bool> deðil, sadece Action olmalý.
    // Çünkü sadece bir olay olduðunu bildiriyoruz, bir deðer (true/false) göndermiyoruz.
    public static event Action OnGameSceneLoaded;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
    }

    public override void OnNetworkDespawn()
    {
        // OnDestroy veya OnDisable gibi bir yerde aboneliði kaldýrmak daha güvenli olabilir
        // Ama þimdilik burada kalabilir.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
        }
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // Eðer yüklenen sahne GameScene ise ve bu olay bu client'ta gerçekleþiyorsa...
        if (sceneName == "GameScene" && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // ...doðru event'i çaðýr.
            OnGameSceneLoaded?.Invoke();
        }
    }
}