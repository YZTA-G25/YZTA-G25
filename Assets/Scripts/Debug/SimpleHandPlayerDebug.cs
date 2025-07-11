using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

/// <summary>
/// Simple HandPlayer movement diagnostic
/// Add this to HandPlayer prefab temporarily to debug movement issues
/// </summary>
public class SimpleHandPlayerDebug : NetworkBehaviour
{
    private HandController handController;
    private CharacterController characterController;
    private PlayerInput playerInput;
    
    private void Start()
    {
        handController = GetComponent<HandController>();
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        Debug.Log($"=== HandPlayer Debug Start ===");
        Debug.Log($"IsOwner: {IsOwner}");
        Debug.Log($"HandController: {(handController != null ? "Found" : "MISSING")}");
        Debug.Log($"CharacterController: {(characterController != null ? "Found" : "MISSING")}");
        Debug.Log($"PlayerInput: {(playerInput != null ? "Found" : "MISSING")}");
        Debug.Log($"HandController Enabled: {(handController != null ? handController.enabled.ToString() : "N/A")}");
        
        if (playerInput != null)
        {
            Debug.Log($"Current Action Map: {playerInput.currentActionMap?.name}");
            Debug.Log($"PlayerInput Enabled: {playerInput.enabled}");
        }
        
        if (characterController != null)
        {
            Debug.Log($"CharacterController Enabled: {characterController.enabled}");
            Debug.Log($"Move Speed (if HandController exists): {(handController != null ? "Check HandController Inspector" : "N/A")}");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Check for WASD input directly
        if (Keyboard.current != null)
        {
            bool wPressed = Keyboard.current.wKey.isPressed;
            bool aPressed = Keyboard.current.aKey.isPressed;
            bool sPressed = Keyboard.current.sKey.isPressed;
            bool dPressed = Keyboard.current.dKey.isPressed;

            if (wPressed || aPressed || sPressed || dPressed)
            {
                Debug.Log($"[HandDebug] WASD Input Detected: W={wPressed} A={aPressed} S={sPressed} D={dPressed}");
            }
        }

        // Check if HandController is processing movement
        if (handController != null && handController.enabled)
        {
            // This will show if Update is being called on HandController
            if (Time.frameCount % 60 == 0) // Every 60 frames (roughly once per second)
            {
                Debug.Log($"[HandDebug] HandController is enabled and Update should be running");
            }
        }
        // a
    }
}
