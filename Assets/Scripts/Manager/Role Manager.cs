using Unity.Cinemachine;
using UnityEngine;
using Unity.Netcode;

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
        if (eyePlayerFeedCamera == null) eyePlayerFeedCamera = GameObject.FindGameObjectWithTag("EyePlayer Feed CM").GetComponent<CinemachineCamera>();
        if (isHandPlayer) eyePlayerFeedCamera.Follow = this.gameObject.transform;
    }
}
