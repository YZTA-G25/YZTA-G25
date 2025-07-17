using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class PlayerSetupManager : NetworkBehaviour
{
    [Header("Player Type")]
    [SerializeField] private bool isEyePlayer = true;

    [Header("Camera Configuration")]
    [SerializeField] private Unity.Cinemachine.OutputChannels cameraOutputChannel;
    private GameObject eyePlayerCamera;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;


    public void Start()
    {
        // Camera setup moved to OnNetworkSpawn for proper network ownership handling
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable components for non-owners to prevent conflicts
            DisableNonOwnerComponents();
            return;
        }

        // Handle specific camera setup for local player only
        HandleLocalCameraSetup();
        
        SetupCameras();
        // Remove input setup - controllers handle their own input
        SetupNetworkOwnership();

        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] {(isEyePlayer ? "Eye" : "Hand")} Player setup complete for Client {OwnerClientId}");
        }
    }

    private void HandleLocalCameraSetup()
    {
        // Only affect cameras when this is a HandPlayer owner
        if (!isEyePlayer)
        {
            // Find Eye Player Camera only if this is the HandPlayer
            eyePlayerCamera = GameObject.FindGameObjectWithTag("Eye Player Camera");
            
            if (eyePlayerCamera != null) 
            {
                eyePlayerCamera.SetActive(false);
                if (enableDebugLogs)
                {
                    Debug.Log($"[PlayerSetup] Local HandPlayer (Client {OwnerClientId}) disabled Eye Player Camera");
                }
            }
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
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Disabled camera '{cam.name}' for non-owner");
            }
        }
        
        // Disable virtual cameras for non-owners (Unity 6 Cinemachine)
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        foreach (var vcam in virtualCameras)
        {
            vcam.enabled = false;
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Disabled virtual camera '{vcam.name}' for non-owner");
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] Disabled {cameras.Length} cameras and {virtualCameras.Length} virtual cameras for non-owner of {(isEyePlayer ? "Eye" : "Hand")} Player");
        }
    }
    
    private void SetupCameras()
    {
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerSetup] Found {virtualCameras.Length} cameras on {(isEyePlayer ? "Eye" : "Hand")} Player");
        }
        
        foreach (var vcam in virtualCameras)
        {
            vcam.OutputChannel = cameraOutputChannel;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Set {(isEyePlayer ? "Eye" : "Hand")} Player camera '{vcam.name}' to output channel {cameraOutputChannel}");
            }
        }
        
        // Also set up regular cameras if any
        var cameras = GetComponentsInChildren<Camera>();
        foreach (var cam in cameras)
        {
            // Enable the camera for the owner
            cam.enabled = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerSetup] Enabled camera '{cam.name}' for {(isEyePlayer ? "Eye" : "Hand")} Player owner");
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
    
    /// <summary>
    /// Validate camera setup and output channel configuration
    /// </summary>
    [ContextMenu("Validate Camera Setup")]
    public void ValidateCameraSetup()
    {
        Debug.Log($"[PlayerSetup] === Camera Validation for {(isEyePlayer ? "Eye" : "Hand")} Player ===");
        Debug.Log($"[PlayerSetup] Configured Output Channel: {cameraOutputChannel}");
        Debug.Log($"[PlayerSetup] Is Owner: {IsOwner}");
        
        var virtualCameras = GetComponentsInChildren<CinemachineCamera>();
        Debug.Log($"[PlayerSetup] Found {virtualCameras.Length} virtual cameras:");
        
        for (int i = 0; i < virtualCameras.Length; i++)
        {
            var vcam = virtualCameras[i];
            Debug.Log($"[PlayerSetup]   {i + 1}. '{vcam.name}' - Output Channel: {vcam.OutputChannel}, Enabled: {vcam.enabled}");
        }
        
        var cameras = GetComponentsInChildren<Camera>();
        Debug.Log($"[PlayerSetup] Found {cameras.Length} regular cameras:");
        
        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            Debug.Log($"[PlayerSetup]   {i + 1}. '{cam.name}' - Enabled: {cam.enabled}");
        }
        
        Debug.Log($"[PlayerSetup] === End Camera Validation ===");
    }
}
