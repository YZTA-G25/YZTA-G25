using UnityEngine;
using Unity.Netcode;

public class HeadController : NetworkBehaviour
{   
    [SerializeField] private Transform headToRotate;
    
    // Network variables for separate axis control
    private NetworkVariable<float> horizontalAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> verticalAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> rollAngle = new NetworkVariable<float>(0f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        horizontalAngle.OnValueChanged += OnRotationChanged;
        verticalAngle.OnValueChanged += OnRotationChanged;
        rollAngle.OnValueChanged += OnRotationChanged;
    }

    public override void OnNetworkDespawn()
    {
        horizontalAngle.OnValueChanged -= OnRotationChanged;
        verticalAngle.OnValueChanged -= OnRotationChanged;
        rollAngle.OnValueChanged -= OnRotationChanged;
    }

    private void OnRotationChanged(float previousValue, float newValue)
    {
        UpdateHeadRotation();
    }

    private void UpdateHeadRotation()
    {
        if (headToRotate != null)
        {
            // Combine all three angles into final rotation
            headToRotate.localRotation = Quaternion.Euler(
                verticalAngle.Value,    // X axis (up/down)
                horizontalAngle.Value,  // Y axis (left/right)
                rollAngle.Value         // Z axis (roll)
            );
        }
    }

    // RPC'nin adını ve parametrelerini değiştiriyoruz
    [ServerRpc(RequireOwnership = false)]
    public void SetHeadRotationServerRpc(Vector3 axis, float targetAngle)
    {
        if (headToRotate != null)
        {
            // Update the appropriate axis based on input
            if (axis == Vector3.right) // Vertical rotation (up/down)
            {
                verticalAngle.Value = targetAngle;
            }
            else if (axis == Vector3.up) // Horizontal rotation (left/right)
            {
                horizontalAngle.Value = targetAngle;
            }
            else if (axis == Vector3.forward) // Roll rotation
            {
                rollAngle.Value = targetAngle;
            }
        }
    }

    // Convenience methods for direct axis control
    [ServerRpc(RequireOwnership = false)]
    public void SetHorizontalRotationServerRpc(float angle)
    {
        horizontalAngle.Value = angle;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetVerticalRotationServerRpc(float angle)
    {
        verticalAngle.Value = angle;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRollRotationServerRpc(float angle)
    {
        rollAngle.Value = angle;
    }

    // Get current rotation values for debugging or external systems
    public Vector3 GetCurrentRotation()
    {
        return new Vector3(verticalAngle.Value, horizontalAngle.Value, rollAngle.Value);
    }
}
