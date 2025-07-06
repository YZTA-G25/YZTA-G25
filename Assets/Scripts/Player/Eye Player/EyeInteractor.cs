using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class EyeInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float leverSensitivity = 2f; // Kol Hassasiyeti
    private Camera camera;
    private LeverController leverInRange;
    private LeverController controlledLever; // Kontrol edilen kol
    private void Start()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        // Eğer bir kol kontrol etmiyorsak, kontrol edecek kol ara
        if (controlledLever == null)
        {
            FindLever();
        }
        else // Eğer kontrol ediyorsak
        {
            // Mouse'un Y eksenindeki hareketini al ve kola gönder
            float mouseYInput = Mouse.current.delta.y.ReadValue();
            controlledLever.UpdateRotation(mouseYInput * leverSensitivity * Time.deltaTime);
        }
    }

    private void FindLever()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            hit.collider.TryGetComponent(out leverInRange);
        }
        else
        {
            leverInRange = null;
        }
    }

    // Input Actions'dan "Interact" veya "Grab" eylemi geldiğinde çalışır
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Sadece tuşa basıldığında çalıştır
        if (!context.performed) return;

        // Eğer bir kolu zaten kontrol ediyorsak, bırak
        if (controlledLever != null)
        {
            controlledLever.Release();
            controlledLever = null;
            Debug.Log("Kol Bırakıldı.");
        }
        else if (leverInRange != null) // Eğer bir kol menzilindeysek ve bir şey kontrol etmiyorsak onu tut
        {
            controlledLever = leverInRange;
            controlledLever.Grab();
            Debug.Log("Kol tutuldu", controlledLever);
        }
    }
}
