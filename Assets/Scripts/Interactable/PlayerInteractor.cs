// Scripts/Player/PlayerInteractor.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Transform handHoldPoint; // Malzemenin tutulaca�� nokta

    private Camera _camera;
    private Interactable _itemInRange;
    private GameObject _heldItem;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        // Elimiz bir �ey tutmuyorsa, etkile�imli nesne ara.
        if (_heldItem == null)
        {
            FindInteractable();
        }
        else
        {
            _itemInRange = null; // Elimiz doluyken ba�ka nesneyle etkile�ime giremeyiz.
        }
        // TODO: UI'da _itemInRange.interactionPrompt'u g�ster.
    }

    void FindInteractable()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            hit.collider.TryGetComponent(out _itemInRange);

            // E�ER B�R �EY G�R�YORSA KONSOLA YAZDIR
            if (_itemInRange != null)
            {
                Debug.Log("G�r�len obje: " + _itemInRange.gameObject.name);
            }
        }
        else
        {
            _itemInRange = null;
        }
    }

    // PlayerInteractor.cs - Yeni Hali
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Tu�a bas�ld���nda (performed)...
        if (context.performed)
        {
            // ...ve elimiz bo�sa ve bir nesnenin menzilindeysek, onu tut.
            if (_heldItem == null && _itemInRange != null)
            {
                _itemInRange.Interact(this);
            }
        }
        // Tu� b�rak�ld���nda (canceled)...
        else if (context.canceled)
        {
            // ...ve elimiz doluysa, onu b�rak.
            if (_heldItem != null)
            {
                ReleaseItem();
            }
        }
    }

    // PlayerInteractor.cs i�indeki HoldItem metodu

    public void HoldItem(GameObject item)
    {
        _heldItem = item;

        // ANAHTAR SATIR: Tuttu�umuz objenin ebeveynini (parent),
        // Inspector'dan az �nce atad���m�z 'handHoldPoint' olarak ayarl�yoruz.
        _heldItem.transform.SetParent(handHoldPoint);

        // Objenin pozisyonunu ve rotasyonunu tutma noktas�na g�re s�f�rl�yoruz ki tam otursun.
        _heldItem.transform.localPosition = Vector3.zero;
        _heldItem.transform.localRotation = Quaternion.identity;

        // Fizi�ini kapat�yoruz ki elimizdeyken titremesin veya d��mesin.
        if (_heldItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;
    }

    private void ReleaseItem()
    {
        if (_heldItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        _heldItem.transform.SetParent(null);
        _heldItem = null;
    }
}