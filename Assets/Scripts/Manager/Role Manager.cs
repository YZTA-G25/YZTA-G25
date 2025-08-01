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

        // Kamera arama kodunu buradan kaldırıyoruz.
        // Yeni event'i dinlemeye başlıyoruz.
        SceneLoadManager.OnGameSceneLoaded += InitializeCamera;
    }

    // Obje yok olduğunda event aboneliğini iptal etmeyi unutmayın.
    public override void OnNetworkDespawn()
    {
        SceneLoadManager.OnGameSceneLoaded -= InitializeCamera;
    }

    // Bu metot sadece GameScene yüklendiğinde çağrılacak.
    private void InitializeCamera()
    {
        if (IsOwner && isHandPlayer)
        {
            var cameraObject = GameObject.FindGameObjectWithTag("EyePlayer Feed CM");
            if (cameraObject != null)
            {
                eyePlayerFeedCamera = cameraObject.GetComponent<CinemachineCamera>();
                if (eyePlayerFeedCamera != null)
                {
                    eyePlayerFeedCamera.Follow = this.transform;
                }
            }
        }
    }
}
