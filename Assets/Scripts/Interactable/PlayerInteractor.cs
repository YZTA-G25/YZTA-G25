// Scripts/Player/PlayerInteractor.cs (YENÝ VE TEMÝZ HALÝ)
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
        // Bu script artýk sadece "bakarak etkileþim" için kullanýlacak.
        // Þimdilik sadece objeyi bulsun ama bir þey yapmasýn.
        // Fiziksel etkileþimleri buradan YÖNETMÝYORUZ.
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
                // Sadece ne gördüðümüzü bilsin, bir þey yapmasýn.
                // Debug.Log("Kamera görüyor: " + _itemInRange.gameObject.name);
            }
        }
        else
        {
            _itemInRange = null;
        }
    }
}