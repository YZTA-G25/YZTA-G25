// HandInteractor.cs - Manuel Takip Yöntemi
using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutacaðý nokta.")]
    [SerializeField] private Transform handHoldPoint;

    private Interactable _interactableInRange;
    private GameObject _heldItem;
    private Rigidbody _heldItemRb;

    // El bir objenin etkileþim alanýna girdiðinde...
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
        {
            _interactableInRange = interactable;
        }
    }

    // El etkileþim alanýndan çýktýðýnda...
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable) && _interactableInRange == interactable)
        {
            _interactableInRange = null;
        }
    }

    // Her frame'de çalýþýr
    private void LateUpdate()
    {
        // Elimizde bir obje varsa, onun pozisyonunu her frame'de
        // bizim tutma noktamýzýn pozisyonuna eþitle.
        if (_heldItem != null && handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
    }

    // Input'tan gelen "Grab" eylemi bu metodu çaðýracak
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed) // Tuþa basýldýðýnda
        {
            if (_heldItem == null && _interactableInRange != null)
            {
                _interactableInRange.Interact(this);
            }
        }
        else if (context.canceled) // Tuþ býrakýldýðýnda
        {
            if (_heldItem != null)
            {
                ReleaseItem();
            }
        }
    }

    public void HoldItem(GameObject item)
    {
        _heldItem = item;
        _heldItemRb = _heldItem.GetComponent<Rigidbody>();

        // Fiziðini kapatýyoruz ki elimizdeyken titremesin veya düþmesin.
        if (_heldItemRb != null) _heldItemRb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;

        // --- YENÝ EKLENEN ANAHTAR KISIM ---
        // Update'i beklemeden, objeyi tuttuðumuz ANDA pozisyonunu elinize ýþýnlýyoruz.
        // Bu, "ortada doðma" sorununu çözer.
        if (handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
        else
        {
            // Eðer handHoldPoint atanmamýþsa, doðrudan elin pozisyonuna ata.
            _heldItem.transform.position = this.transform.position;
            _heldItem.transform.rotation = this.transform.rotation;
            Debug.LogWarning("HandInteractor'da HandHoldPoint atanmamýþ! Obje doðrudan ele yerleþtirildi.");
        }
        // --- YENÝ KISIM BÝTTÝ ---

        Debug.Log(_heldItem.name + " tutuluyor.");
    }

    private void ReleaseItem()
    {
        Debug.Log(_heldItem.name + " býrakýldý.");

        // Fiziðini tekrar aktive ediyoruz.
        if (_heldItemRb != null) _heldItemRb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        // Referanslarý temizle. Artýk SetParent(null) yapmamýza gerek yok çünkü hiç parent yapmadýk.
        _heldItem = null;
        _heldItemRb = null;
    }
}