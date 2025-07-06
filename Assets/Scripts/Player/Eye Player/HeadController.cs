using UnityEngine;
using Unity.Netcode;

public class HeadController : NetworkBehaviour
{
    [SerializeField] private Transform headToRotate;

    // RPC'nin adını ve parametrelerini değiştiriyoruz
    [ServerRpc(RequireOwnership = false)]
    public void SetHeadRotationServerRpc(Vector3 axis, float targetAngle)
    {
        if (headToRotate != null)
        {
            // Dış dünyadaki kafanın rotasyonunu, kolun açısına doğrudan eşitliyoruz.
            // (Eksenlerin modele göre değişmesi gerekebilir)
            if (axis == Vector3.right) // Kol yukarı/aşağı
            {
                headToRotate.localRotation = Quaternion.Euler(targetAngle, headToRotate.localEulerAngles.y, headToRotate.localEulerAngles.z);
            }
            else if (axis == Vector3.up) // Kol sola/sağa
            {
                headToRotate.localRotation = Quaternion.Euler(headToRotate.localEulerAngles.x, targetAngle, headToRotate.localEulerAngles.z);
            }
        }
    }
}
