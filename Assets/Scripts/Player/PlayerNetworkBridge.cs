using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkBridge : NetworkBehaviour
{
    //CookingStation eþyayý destroy edebilmesi için böyle bir kod yaptým
    [ServerRpc]
    public void RequestDestroyObjectServerRpc(NetworkObjectReference objectToDespawnRef)
    {
        if (objectToDespawnRef.TryGet(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
    }

    // Gelecekte dolaptan obje isteme gibi diðer RPC'ler de buraya eklenebilir.
}