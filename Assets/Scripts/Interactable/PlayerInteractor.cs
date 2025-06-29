// Scripts/Player/PlayerInteractor.cs (YEN� VE TEM�Z HAL�)
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayer;
    private Camera _camera;
    private Interactable _itemInRange;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        // Bu script art�k sadece "bakarak etkile�im" i�in kullan�lacak.
        // �imdilik sadece objeyi bulsun ama bir �ey yapmas�n.
        // Fiziksel etkile�imleri buradan Y�NETM�YORUZ.
        FindInteractable();
    }

    void FindInteractable()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            hit.collider.TryGetComponent(out _itemInRange);
            if (_itemInRange != null)
            {
                // Sadece ne g�rd���m�z� bilsin, bir �ey yapmas�n.
                // Debug.Log("Kamera g�r�yor: " + _itemInRange.gameObject.name);
            }
        }
        else
        {
            _itemInRange = null;
        }
    }
}