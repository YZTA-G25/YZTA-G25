// HandInteractor.cs - Manuel Takip Y�ntemi
using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutaca�� nokta.")]
    [SerializeField] private Transform handHoldPoint;

    private Interactable _interactableInRange;
    private GameObject _heldItem;
    private Rigidbody _heldItemRb;

    // El bir objenin etkile�im alan�na girdi�inde...
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable))
        {
            _interactableInRange = interactable;
        }
    }

    // El etkile�im alan�ndan ��kt���nda...
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Interactable interactable) && _interactableInRange == interactable)
        {
            _interactableInRange = null;
        }
    }

    // Her frame'de �al���r
    private void LateUpdate()
    {
        // Elimizde bir obje varsa, onun pozisyonunu her frame'de
        // bizim tutma noktam�z�n pozisyonuna e�itle.
        if (_heldItem != null && handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
    }

    // Input'tan gelen "Grab" eylemi bu metodu �a��racak
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed) // Tu�a bas�ld���nda
        {
            if (_heldItem == null && _interactableInRange != null)
            {
                _interactableInRange.Interact(this);
            }
        }
        else if (context.canceled) // Tu� b�rak�ld���nda
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

        // Fizi�ini kapat�yoruz ki elimizdeyken titremesin veya d��mesin.
        if (_heldItemRb != null) _heldItemRb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;

        // --- YEN� EKLENEN ANAHTAR KISIM ---
        // Update'i beklemeden, objeyi tuttu�umuz ANDA pozisyonunu elinize ���nl�yoruz.
        // Bu, "ortada do�ma" sorununu ��zer.
        if (handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
        else
        {
            // E�er handHoldPoint atanmam��sa, do�rudan elin pozisyonuna ata.
            _heldItem.transform.position = this.transform.position;
            _heldItem.transform.rotation = this.transform.rotation;
            Debug.LogWarning("HandInteractor'da HandHoldPoint atanmam��! Obje do�rudan ele yerle�tirildi.");
        }
        // --- YEN� KISIM B�TT� ---

        Debug.Log(_heldItem.name + " tutuluyor.");
    }

    private void ReleaseItem()
    {
        Debug.Log(_heldItem.name + " b�rak�ld�.");

        // Fizi�ini tekrar aktive ediyoruz.
        if (_heldItemRb != null) _heldItemRb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        // Referanslar� temizle. Art�k SetParent(null) yapmam�za gerek yok ��nk� hi� parent yapmad�k.
        _heldItem = null;
        _heldItemRb = null;
    }
}