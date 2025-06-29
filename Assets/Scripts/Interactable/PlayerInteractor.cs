// Scripts/Player/PlayerInteractor.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Transform handHoldPoint; // Malzemenin tutulacaðý nokta

    private Camera _camera;
    private Interactable _itemInRange;
    private GameObject _heldItem;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        // Elimiz bir þey tutmuyorsa, etkileþimli nesne ara.
        if (_heldItem == null)
        {
            FindInteractable();
        }
        else
        {
            _itemInRange = null; // Elimiz doluyken baþka nesneyle etkileþime giremeyiz.
        }
        // TODO: UI'da _itemInRange.interactionPrompt'u göster.
    }

    void FindInteractable()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            hit.collider.TryGetComponent(out _itemInRange);

            // EÐER BÝR ÞEY GÖRÜYORSA KONSOLA YAZDIR
            if (_itemInRange != null)
            {
                Debug.Log("Görülen obje: " + _itemInRange.gameObject.name);
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
        // Tuþa basýldýðýnda (performed)...
        if (context.performed)
        {
            // ...ve elimiz boþsa ve bir nesnenin menzilindeysek, onu tut.
            if (_heldItem == null && _itemInRange != null)
            {
                _itemInRange.Interact(this);
            }
        }
        // Tuþ býrakýldýðýnda (canceled)...
        else if (context.canceled)
        {
            // ...ve elimiz doluysa, onu býrak.
            if (_heldItem != null)
            {
                ReleaseItem();
            }
        }
    }

    // PlayerInteractor.cs içindeki HoldItem metodu

    public void HoldItem(GameObject item)
    {
        _heldItem = item;

        // ANAHTAR SATIR: Tuttuðumuz objenin ebeveynini (parent),
        // Inspector'dan az önce atadýðýmýz 'handHoldPoint' olarak ayarlýyoruz.
        _heldItem.transform.SetParent(handHoldPoint);

        // Objenin pozisyonunu ve rotasyonunu tutma noktasýna göre sýfýrlýyoruz ki tam otursun.
        _heldItem.transform.localPosition = Vector3.zero;
        _heldItem.transform.localRotation = Quaternion.identity;

        // Fiziðini kapatýyoruz ki elimizdeyken titremesin veya düþmesin.
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