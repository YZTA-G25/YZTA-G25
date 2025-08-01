using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    // NetworkVariable, sunucudan tüm client'lara senkronize edilen bir deðiþkendir.
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>("Oyuncu Bekleniyor...", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Bu obje aðda spawn olduðunda...
    public override void OnNetworkSpawn()
    {
        // Eðer bu script'in sahibi bensem (yani bu benim karakterimse)...
        if (IsOwner)
        {
            // ...sunucuya ismimi göndermek için bir komut yolla.
            // Gelecekte bu ismi bir input alanýndan veya oyuncu profilinden alabilirsiniz.
            SetPlayerNameServerRpc("Oyuncu " + (OwnerClientId + 1));
        }
    }

    // ServerRpc, bir client'tan sunucuya gönderilen bir komuttur.
    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        // Bu kod sadece sunucuda çalýþýr.
        // Client'tan gelen ismi, tüm client'lara senkronize edilecek olan NetworkVariable'a atar.
        PlayerName.Value = name;
    }
}