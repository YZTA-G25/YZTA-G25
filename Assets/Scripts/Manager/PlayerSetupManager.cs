using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerSetupManager : NetworkBehaviour
{
    [Header("Player Type")]
    [SerializeField] private bool isEyePlayer = true;

    [Header("Camera Configuration")]
    [SerializeField] private Unity.Cinemachine.OutputChannels cameraOutputChannel;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable components for non-owners to prevent conflicts
            DisableNonOwnerComponents();
            return;
        }
        
        SetupCameras();
        // Remove input setup - controllers handle their own input
        SetupNetworkOwnership();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] {(isEyePlayer ? "Eye" : "Hand")} Player setup complete for Client {OwnerClientId}");
        }
    }
    
    private void DisableNonOwnerComponents()
    {
        // Don't disable PlayerInput - controllers handle their own input
        
        // Disable cameras for non-owners
        var cameras = GetComponentsInChildren<Camera>();
        foreach (var cam in cameras)
        {
            cam.enabled = false;
        }
        
        // Disable virtual cameras for non-owners (Unity 6 Cinemachine)
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        foreach (var vcam in virtualCameras)
        {
            vcam.enabled = false;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] Disabled components for non-owner of {(isEyePlayer ? "Eye" : "Hand")} Player");
        }
    }
    
    private void SetupCameras()
    {
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        
        foreach (var vcam in virtualCameras)
        {
            vcam.OutputChannel = cameraOutputChannel;
            
            if (enableDebugLogs)
                {
                    Debug.Log($"[PlayerSetup] Set {(isEyePlayer ? "Eye" : "Hand")} Player camera '{vcam.name}' to output channel {cameraOutputChannel}");
                }
        }
    }
    
    // Input system setup removed - controllers handle their own input
    // private void SetupInputSystem() { ... }
    
    private void SetupNetworkOwnership()
    {
        // Ensure proper controller setup based on player type
        var eyeController = GetComponent<EyePlayerController>();
        var handController = GetComponent<HandController>();
        
        if (isEyePlayer)
        {
            if (eyeController == null)
            {
                Debug.LogError("[PlayerSetup] Eye Player prefab missing EyePlayerController!");
            }
            if (handController != null)
            {
                handController.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log("[PlayerSetup] Disabled HandController on Eye Player prefab");
                }
            }
        }
        else
        {
            if (handController == null)
            {
                Debug.LogError("[PlayerSetup] Hand Player prefab missing HandController!");
            }
            if (eyeController != null)
            {
                eyeController.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log("[PlayerSetup] Disabled EyePlayerController on Hand Player prefab");
                }
            }
        }
    }
    
    /// <summary>
    /// Call this method to manually reconfigure the player (useful for testing)
    /// </summary>
    [ContextMenu("Reconfigure Player")]
    public void ReconfigurePlayer()
    {
        if (IsOwner)
        {
            SetupCameras();
            // SetupInputSystem(); // Removed - controllers handle their own input
            SetupNetworkOwnership();
        }
    }
}
