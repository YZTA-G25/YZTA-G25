using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public class EyeInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [Tooltip("Kolun hassasiyeti - Yatay fare hareketi ile kontrol edilir")]
    [SerializeField] private float leverSensitivity = 2f;

    [Header("Component References")]
    [Tooltip("Etkileşim ışınının başlayacağı nokta. Genellikle PF'in kamera objesi")]
    [SerializeField] private Transform raycastOrigin;

    private LeverController leverInRange;
    private LeverController controlledLever;

    private bool canInteract = false; // Yeni bayrak değişkeni

    private void Start()
    {
        // Sahne yükleme event'ini dinle
        SceneLoadManager.OnGameSceneLoaded += OnGameSceneReady;
    }

    private void OnDestroy()
    {
        SceneLoadManager.OnGameSceneLoaded -= OnGameSceneReady;
    }
    private void Update()
    {
        // Sahne henüz hazır değilse veya fare bağlı değilse, hiçbir işlem yapma.
        if (!canInteract || Mouse.current == null) return;

        // Eğer şu an bir kol kontrol ETMİYORSAK, etrafta bir kol ara.
        if (controlledLever == null)
        {
            FindLever();
        }
        // Eğer bir kol kontrol EDİYORSAK, fare hareketini o kola gönder.
        else
        {
            // Farenin yatay hareketini al ve hassasiyetle çarparak kola ilet.
            float mouseXInput = Mouse.current.delta.x.ReadValue();
            controlledLever.UpdateRotation(mouseXInput * leverSensitivity * Time.deltaTime);
        }
    }

    private void FindLever()
    {
        if (raycastOrigin == null) return;

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            leverInRange = hit.collider.GetComponent<LeverController>();
            
            if (leverInRange != null)
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
            }
        }
        else
        {
            leverInRange = null;
            CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        try
        {
            if (controlledLever != null)
            {
                controlledLever.Release();
                controlledLever = null;
                Debug.Log("Lever released.");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else if (leverInRange != null)
            {
                controlledLever = leverInRange;
                controlledLever.Grab();
                Debug.Log($"{controlledLever.GetControlDescription()} lever grabbed: {controlledLever.name}");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Grabbed);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnInteract: " + e.Message);
        }
    }


    private void OnGameSceneReady()
    {
        // GameScene yüklendiğinde, artık etkileşime girebiliriz.
        canInteract = true;
    }
}
