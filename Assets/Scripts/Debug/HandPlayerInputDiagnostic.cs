using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

/// <summary>
/// Comprehensive HandPlayer input diagnostic
/// This will help us find exactly why WASD isn't working
/// </summary>
public class HandPlayerInputDiagnostic : NetworkBehaviour
{
    private HandController handController;
    private PlayerControls testPlayerControls;
    private Vector2 detectedMoveInput;
    
    private void Start()
    {
        handController = GetComponent<HandController>();
        
        if (!IsOwner) return;
        
        Debug.Log("=== HandPlayer Input Diagnostic ===");
        Debug.Log($"IsOwner: {IsOwner}");
        Debug.Log($"HandController found: {handController != null}");
        Debug.Log($"HandController enabled: {(handController != null ? handController.enabled.ToString() : "N/A")}");
        
        // Test if we can create our own PlayerControls
        try
        {
            testPlayerControls = new PlayerControls();
            testPlayerControls.HandPlayer.Enable();
            testPlayerControls.HandPlayer.Move.performed += OnTestMoveInput;
            testPlayerControls.HandPlayer.Move.canceled += OnTestMoveInput;
            
            Debug.Log("[InputDiag] Successfully created test PlayerControls");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[InputDiag] Failed to create PlayerControls: {e.Message}");
        }
    }
    
    private void OnTestMoveInput(InputAction.CallbackContext context)
    {
        detectedMoveInput = context.ReadValue<Vector2>();
        Debug.Log($"[InputDiag] Test PlayerControls detected move input: {detectedMoveInput}");
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        // Check raw keyboard input
        if (Keyboard.current != null)
        {
            bool wPressed = Keyboard.current.wKey.isPressed;
            bool aPressed = Keyboard.current.aKey.isPressed;
            bool sPressed = Keyboard.current.sKey.isPressed;
            bool dPressed = Keyboard.current.dKey.isPressed;
            
            if (wPressed || aPressed || sPressed || dPressed)
            {
                Debug.Log($"[InputDiag] Raw Keyboard: W={wPressed} A={aPressed} S={sPressed} D={dPressed}");
                Debug.Log($"[InputDiag] Test move input: {detectedMoveInput}");
                
                // Check if there's a conflicting PlayerInput component
                var playerInput = GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    Debug.LogWarning($"[InputDiag] Found PlayerInput component! This might be conflicting. Enabled: {playerInput.enabled}, Action Map: {playerInput.currentActionMap?.name}");
                }
            }
        }
        
        // Check if HandController's PlayerControls is working
        if (handController != null && Time.frameCount % 120 == 0) // Every 2 seconds
        {
            Debug.Log($"[InputDiag] HandController status check - Enabled: {handController.enabled}");
            
            // Use reflection to check HandController's internal state
            try
            {
                var playerControlsField = typeof(HandController).GetField("playerControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerControlsField != null)
                {
                    var playerControls = playerControlsField.GetValue(handController);
                    Debug.Log($"[InputDiag] HandController PlayerControls: {(playerControls != null ? "Initialized" : "NULL")}");
                }
                
                var moveInputField = typeof(HandController).GetField("moveInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (moveInputField != null)
                {
                    var moveInput = (Vector2)moveInputField.GetValue(handController);
                    Debug.Log($"[InputDiag] HandController moveInput value: {moveInput}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InputDiag] Reflection error: {e.Message}");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (testPlayerControls != null)
        {
            testPlayerControls.HandPlayer.Move.performed -= OnTestMoveInput;
            testPlayerControls.HandPlayer.Move.canceled -= OnTestMoveInput;
            testPlayerControls.Dispose();
        }
    }
}
