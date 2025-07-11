using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public class LeverController : NetworkBehaviour
{
    [Header("Lever Type")]
    [Tooltip("Choose what this lever controls")]
    public LeverControlType controlType = LeverControlType.Horizontal;

    [Header("Lever Settings")]
    [Tooltip("Kolun dönebileceği minimum açı.")]
    [SerializeField] private float minAngle = -45f;

    [Tooltip("Kolun dönebileceği maksimum açı.")]
    [SerializeField] private float maxAngle = 45f;

    [Tooltip("Dönülecek eksen (Lever görsel rotasyonu için).")]
    [SerializeField] private Vector3 leverRotationAxis = Vector3.right;

    [Tooltip("Mouse input'un hassasiyeti.")]
    [SerializeField] private float inputSensitivity = 1f;

    public enum LeverControlType
    {
        Horizontal,  // Controls left/right head movement (Y-axis)
        Vertical,    // Controls up/down head movement (X-axis)
        Roll,        // Controls roll head movement (Z-axis)
        Custom       // Uses the leverRotationAxis for both visual and head rotation
    }

    // Network synchronized angle
    private NetworkVariable<float> networkAngle = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private float currentAngle = 0f;
    private HeadController headController;
    private bool isGrabbed = false;

    public override void OnNetworkSpawn()
    {
        // Subscribe to network angle changes
        networkAngle.OnValueChanged += OnAngleChanged;
        
        // Cache HeadController reference
        if (headController == null)
        {
            headController = FindFirstObjectByType<HeadController>();
        }
    }

    public override void OnNetworkDespawn()
    {
        networkAngle.OnValueChanged -= OnAngleChanged;
    }

    private void OnAngleChanged(float previousValue, float newValue)
    {
        currentAngle = newValue;
        UpdateVisualRotation();
    }

    public void Grab()
    {
        isGrabbed = true;
        Debug.Log($"{controlType} Lever {name} grabbed");
    }

    public void Release()
    {
        isGrabbed = false;
        Debug.Log($"{controlType} Lever {name} released");
    }

    // Called by EyeInteractor when mouse input is detected
    public void UpdateRotation(float input)
    {
        if (!isGrabbed) return;

        // Send input to server for processing
        if (IsOwner || IsServer)
        {
            UpdateRotationServerRpc(input);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(float input)
    {
        // Process input on server with direction based on control type
        float processedInput = GetProcessedInput(input);
        float newAngle = currentAngle + processedInput;
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);
        
        // Update network variable (this will trigger OnAngleChanged on all clients)
        networkAngle.Value = newAngle;
        
        // Send head rotation update
        SendHeadRotationUpdate(newAngle);
    }

    private float GetProcessedInput(float input)
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return -input * inputSensitivity; // Normal direction for horizontal
            case LeverControlType.Vertical:
                return -input * inputSensitivity; // Inverted for more intuitive vertical control
            case LeverControlType.Roll:
                return -input * inputSensitivity; // Normal direction for roll
            case LeverControlType.Custom:
                return -input * inputSensitivity; // User can adjust via inputSensitivity sign
            default:
                return -input * inputSensitivity;
        }
    }

    private void UpdateVisualRotation()
    {
        Vector3 visualAxis = GetVisualRotationAxis();
        transform.localRotation = Quaternion.Euler(visualAxis * currentAngle);
    }

    private void SendHeadRotationUpdate(float angle)
    {
        if (headController != null)
        {
            switch (controlType)
            {
                case LeverControlType.Horizontal:
                    headController.SetHorizontalRotationServerRpc(angle);
                    break;
                case LeverControlType.Vertical:
                    headController.SetVerticalRotationServerRpc(angle);
                    break;
                case LeverControlType.Roll:
                    headController.SetRollRotationServerRpc(angle);
                    break;
                case LeverControlType.Custom:
                    headController.SetHeadRotationServerRpc(leverRotationAxis, angle);
                    break;
            }
        }
    }

    private Vector3 GetVisualRotationAxis()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return Vector3.forward; // Z-axis for horizontal lever visual
            case LeverControlType.Vertical:
                return Vector3.right;   // X-axis for vertical lever visual
            case LeverControlType.Roll:
                return Vector3.up;      // Y-axis for roll lever visual
            case LeverControlType.Custom:
                return leverRotationAxis;
            default:
                return Vector3.right;
        }
    }

    // Initialize the lever angle if needed
    [ServerRpc(RequireOwnership = false)]
    public void InitializeLeverServerRpc(float angle = 0f)
    {
        networkAngle.Value = Mathf.Clamp(angle, minAngle, maxAngle);
    }

    // Get current angle for debugging or external systems
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    // Check if this lever is currently being interacted with
    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    // Utility method to get recommended angle ranges for different lever types
    [ContextMenu("Apply Recommended Settings")]
    public void ApplyRecommendedSettings()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                minAngle = -45f;
                maxAngle = 45f;
                leverRotationAxis = Vector3.forward;
                break;
            case LeverControlType.Vertical:
                minAngle = -30f;
                maxAngle = 30f;
                leverRotationAxis = Vector3.right;
                break;
            case LeverControlType.Roll:
                minAngle = -30f;
                maxAngle = 30f;
                leverRotationAxis = Vector3.up;
                break;
            case LeverControlType.Custom:
                // Keep current settings
                break;
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    // Get the control type for external systems
    public LeverControlType GetControlType()
    {
        return controlType;
    }

    // Get a description of what this lever controls
    public string GetControlDescription()
    {
        switch (controlType)
        {
            case LeverControlType.Horizontal:
                return "Left/Right Head Rotation";
            case LeverControlType.Vertical:
                return "Up/Down Head Rotation";
            case LeverControlType.Roll:
                return "Head Roll Rotation";
            case LeverControlType.Custom:
                return "Custom Axis Rotation";
            default:
                return "Unknown";
        }
    }
}
