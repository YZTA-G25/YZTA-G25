using UnityEngine;
using UnityEngine.InputSystem; // PlayerInput için gerekli
using Unity.Netcode;

// Bu script'in çalýþmasý için bir PlayerInput bileþeni zorunludur.
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputBlocker : NetworkBehaviour
{
    private PlayerInput playerInput;

    // Script ilk yüklendiðinde çalýþýr
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // Obje aðda spawn olduðunda
    public override void OnNetworkSpawn()
    {
        // Sadece bu objenin sahibi olan oyuncu için event'i dinle
        if (IsOwner)
        {
            InGameUIManager.OnGamePaused += HandleGamePaused;
        }
    }

    // Obje aðdan kaldýrýldýðýnda
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            InGameUIManager.OnGamePaused -= HandleGamePaused;
        }
    }

    /// <summary>
    /// OnGamePaused event'i tetiklendiðinde çalýþýr.
    /// </summary>
    private void HandleGamePaused(bool isPaused)
    {
        // Eðer oyun duraklatýldýysa (isPaused = true), PlayerInput'u devre dýþý býrak.
        // Eðer oyun devam ediyorsa (isPaused = false), PlayerInput'u etkinleþtir.
        if (isPaused)
        {
            playerInput.DeactivateInput();
        }
        else
        {
            playerInput.ActivateInput();
        }
    }
}