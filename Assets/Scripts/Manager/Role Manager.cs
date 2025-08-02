using Unity.Cinemachine;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

using UnityEngine.InputSystem;


public class RoleManager : NetworkBehaviour
{
    [Header("Managed Component")]
    [Tooltip("Bu prefab'in input'unu yöneten bileşen")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("Bu prefab'in ana karakter kontrolcüsü.")]
    [SerializeField] private MonoBehaviour characterControllerScript;

    [Header("Camera ayarlaması için")]
    public bool isHandPlayer;
    [SerializeField] private CinemachineCamera eyePlayerFeedCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerInput != null) playerInput.enabled = false;
            if (characterControllerScript != null) characterControllerScript.enabled = false;
        }

        // Kamera arama kodunu buradan kaldırıyoruz.
        // Yeni event'i dinlemeye başlıyoruz.
        InitializeCamera();
    }

    // Bu metot sadece GameScene yüklendiğinde çağrılacak.
    private void InitializeCamera()
    {
        if (IsOwner && isHandPlayer)
        {
            StartCoroutine(InitializeCameraCoroutine());
        }
    }

    private IEnumerator InitializeCameraCoroutine()
    {
        // Wait until the EyePlayer Feed CM camera is found
        while (GameObject.FindGameObjectWithTag("EyePlayer Feed CM") == null)
        {
            yield return null;
        }

        var cameraObject = GameObject.FindGameObjectWithTag("EyePlayer Feed CM");
        if (cameraObject != null)
        {
            eyePlayerFeedCamera = cameraObject.GetComponent<CinemachineCamera>();
            if (eyePlayerFeedCamera != null)
            {
                // Use RPC to ensure network synchronization
                SetCameraFollowServerRpc(NetworkObjectId);
                Debug.Log($"[RoleManager] HandPlayer requested camera follow via RPC");
            }
            else
            {
                Debug.LogError("[RoleManager] Found EyePlayer Feed CM but missing CinemachineCamera component!");
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetCameraFollowServerRpc(ulong handPlayerNetworkId)
    {
        // Call ClientRpc to update all clients
        SetCameraFollowClientRpc(handPlayerNetworkId);
    }

    [ClientRpc]
    private void SetCameraFollowClientRpc(ulong handPlayerNetworkId)
    {
        // Find the HandPlayer by NetworkObjectId
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(handPlayerNetworkId, out NetworkObject handPlayerNetworkObject))
        {
            var cameraObject = GameObject.FindGameObjectWithTag("EyePlayer Feed CM");
            if (cameraObject != null)
            {
                var feedCamera = cameraObject.GetComponent<CinemachineCamera>();
                if (feedCamera != null)
                {
                    // Debug camera state before assignment
                    Debug.Log($"[RoleManager] Camera before assignment - Follow: {feedCamera.Follow}, Priority: {feedCamera.Priority}, Enabled: {feedCamera.enabled}");
                    Debug.Log($"[RoleManager] Camera GameObject active: {cameraObject.activeInHierarchy}");
                    Debug.Log($"[RoleManager] HandPlayer transform: {handPlayerNetworkObject.transform.name} at position: {handPlayerNetworkObject.transform.position}");
                    
                    // Check for other components that might interfere
                    var networkTransform = cameraObject.GetComponent<NetworkTransform>();
                    var networkObject = cameraObject.GetComponent<NetworkObject>();
                    Debug.Log($"[RoleManager] Camera has NetworkTransform: {networkTransform != null}, NetworkObject: {networkObject != null}");
                    
                    feedCamera.Follow = handPlayerNetworkObject.transform;
                    
                    // Debug camera state after assignment
                    Debug.Log($"[RoleManager] Camera after assignment - Follow: {feedCamera.Follow}");
                    Debug.Log($"[RoleManager] Camera follow set to HandPlayer on client via RPC");
                }
                else
                {
                    Debug.LogError("[RoleManager] CinemachineCamera component not found on EyePlayer Feed CM!");
                }
            }
            else
            {
                Debug.LogError("[RoleManager] EyePlayer Feed CM GameObject not found!");
            }
        }
        else
        {
            Debug.LogError($"[RoleManager] Could not find HandPlayer with NetworkObjectId: {handPlayerNetworkId}");
        }
    }
}
