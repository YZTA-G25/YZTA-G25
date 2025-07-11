using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

/// <summary>
/// Diagnostic script to debug HandPlayer movement issues
/// Attach this to Hand Player prefab temporarily for debugging
/// Compatible with Unity 6 and latest Input System
/// </summary>
public class HandPlayerMovementDiagnostic : NetworkBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableVerboseLogging = true;
    [SerializeField] private bool showInputValues = true;
    [SerializeField] private bool showMovementCalculations = true;
    
    private HandController handController;
    private CharacterController characterController;
    private PlayerInput playerInput;
    private PlayerControls playerControls;
    
    private void Start()
    {
        handController = GetComponent<HandController>();
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        if (enableVerboseLogging)
        {
            Debug.Log($"[HandDiagnostic] Starting diagnostic for HandPlayer - IsOwner: {IsOwner}");
            Debug.Log($"[HandDiagnostic] HandController found: {handController != null}");
            Debug.Log($"[HandDiagnostic] CharacterController found: {characterController != null}");
            Debug.Log($"[HandDiagnostic] PlayerInput found: {playerInput != null}");
            
            if (playerInput != null)
            {
                Debug.Log($"[HandDiagnostic] Current Action Map: {playerInput.currentActionMap?.name}");
            }
        }
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        if (showInputValues)
        {
            CheckInputValues();
        }
        
        if (showMovementCalculations)
        {
            CheckMovementState();
        }
    }
    
    private void CheckInputValues()
    {
        if (playerInput != null && playerInput.currentActionMap != null)
        {
            var moveAction = playerInput.currentActionMap.FindAction("Move");
            if (moveAction != null)
            {
                var moveValue = moveAction.ReadValue<Vector2>();
                if (moveValue.magnitude > 0.1f)
                {
                    Debug.Log($"[HandDiagnostic] Input detected - Move: {moveValue}");
                }
            }
        }
        
        // Check keyboard directly (Unity 6 enhanced input)
        if (Keyboard.current != null)
        {
            bool w = Keyboard.current.wKey.isPressed;
            bool a = Keyboard.current.aKey.isPressed;
            bool s = Keyboard.current.sKey.isPressed;
            bool d = Keyboard.current.dKey.isPressed;
            
            if (w || a || s || d)
            {
                Debug.Log($"[HandDiagnostic] Direct keyboard input - W:{w} A:{a} S:{s} D:{d}");
                
                // Unity 6: Additional input state information
                var wState = Keyboard.current.wKey.ReadValue();
                var aState = Keyboard.current.aKey.ReadValue();
                var sState = Keyboard.current.sKey.ReadValue();
                var dState = Keyboard.current.dKey.ReadValue();
                Debug.Log($"[HandDiagnostic] Key states - W:{wState:F2} A:{aState:F2} S:{sState:F2} D:{dState:F2}");
            }
        }
    }
    
    private void CheckMovementState()
    {
        if (characterController != null)
        {
            var velocity = characterController.velocity;
            if (velocity.magnitude > 0.1f)
            {
                Debug.Log($"[HandDiagnostic] Character velocity: {velocity} (magnitude: {velocity.magnitude})");
            }
        }
        
        if (handController != null)
        {
            var position = transform.position;
            Debug.Log($"[HandDiagnostic] Position: {position}");
        }
    }
    
    [ContextMenu("Check HandController Status")]
    public void CheckHandControllerStatus()
    {
        if (handController == null)
        {
            Debug.LogError("[HandDiagnostic] HandController is null!");
            return;
        }
        
        Debug.Log($"[HandDiagnostic] HandController enabled: {handController.enabled}");
        Debug.Log($"[HandDiagnostic] HandController IsOwner: {handController.IsOwner}");
        Debug.Log($"[HandDiagnostic] HandController NetworkObject: {handController.NetworkObject != null}");
        
        if (handController.NetworkObject != null)
        {
            Debug.Log($"[HandDiagnostic] NetworkObject IsOwner: {handController.NetworkObject.IsOwner}");
            Debug.Log($"[HandDiagnostic] NetworkObject OwnerClientId: {handController.NetworkObject.OwnerClientId}");
            
            // Unity 6 Netcode: Additional network state information
            Debug.Log($"[HandDiagnostic] NetworkObject IsSpawned: {handController.NetworkObject.IsSpawned}");
            Debug.Log($"[HandDiagnostic] NetworkObject NetworkObjectId: {handController.NetworkObject.NetworkObjectId}");
            Debug.Log($"[HandDiagnostic] NetworkManager Connected: {NetworkManager.Singleton?.IsConnectedClient ?? false}");
        }
    }
    
    [ContextMenu("Check Input System Status")]
    public void CheckInputSystemStatus()
    {
        if (playerInput == null)
        {
            Debug.LogError("[HandDiagnostic] PlayerInput is null!");
            return;
        }
        
        Debug.Log($"[HandDiagnostic] PlayerInput enabled: {playerInput.enabled}");
        Debug.Log($"[HandDiagnostic] Current action map: {playerInput.currentActionMap?.name}");
        Debug.Log($"[HandDiagnostic] Default action map: {playerInput.defaultActionMap}");
        
        if (playerInput.actions != null)
        {
            Debug.Log($"[HandDiagnostic] Input actions asset: {playerInput.actions.name}");
            
            foreach (var actionMap in playerInput.actions.actionMaps)
            {
                Debug.Log($"[HandDiagnostic] Available action map: {actionMap.name} (enabled: {actionMap.enabled})");
            }
        }
        
        // Check specific actions
        var moveAction = playerInput.currentActionMap?.FindAction("Move");
        if (moveAction != null)
        {
            Debug.Log($"[HandDiagnostic] Move action found - enabled: {moveAction.enabled}");
            Debug.Log($"[HandDiagnostic] Move action bindings count: {moveAction.bindings.Count}");
            
            for (int i = 0; i < moveAction.bindings.Count; i++)
            {
                var binding = moveAction.bindings[i];
                Debug.Log($"[HandDiagnostic] Binding {i}: {binding.effectivePath}");
            }
        }
        else
        {
            Debug.LogError("[HandDiagnostic] Move action not found in current action map!");
        }
    }
}
