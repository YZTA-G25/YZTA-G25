using Unity.Netcode;
using UnityEngine;

public class RemoteCameraControl : NetworkBehaviour
{
    [SerializeField] private Transform cameraToControl; // EyePlayer VCam'in Transformu

    // Ağ üzerinden senkronize olacak rotasyon değeri
    // Sadece sunucu değiştirebilir, tüm client'lar okuyabilir
    private NetworkVariable<Quaternion> cameraRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Eğer biz Client isek (El oyuncusu), kamera rotasyonunu buna göre ayarla
        if (!IsServer)
        {
            cameraRotation.OnValueChanged += (prev, next) =>
            {
                cameraToControl.localRotation = next;
            };
        }
    }

    // Bu metot, Göz oyuncusu'nun kolundan ServerRpc ile çağırılacak
    [ServerRpc(RequireOwnership = false)]
    public void UpdateRotationServerRpc(Quaternion newRotation)
    {
        // Sunucu, yeni rotasyon değerini NetworkVariable'a yazar.
        // Bu değişiklik otomatik olarak tüm client'lara dağıtılır.
        cameraRotation.Value = newRotation;
    }
}
