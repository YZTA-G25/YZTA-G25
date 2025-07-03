using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutacaðý nokta.")]
    [SerializeField] private Transform handHoldPoint;

    // Elin etkileþim alanýndaki objeleri tutar
    private CookingStation _stationInRange;
    private CabinetController _cabinetInRange;
    private GrabbableItem _grabbableInRange;

    // Elin þu anda tuttuðu obje
    private GameObject _heldItem;
    private Rigidbody _heldItemRb;

    // El bir objenin etkileþim alanýna girdiðinde...
    private void OnTriggerEnter(Collider other)
    {
        // Girdiði obje bir dolap mý?
        if (other.TryGetComponent(out CabinetController cabinet))
        {
            _cabinetInRange = cabinet;
            Debug.Log("Dolap alanýna girildi: " + cabinet.gameObject.name);
        }
        // Girdiði obje yerden alýnabilir bir malzeme mi?
        else if (other.TryGetComponent(out GrabbableItem item))
        {
            _grabbableInRange = item;
            Debug.Log("Yerden alýnabilir obje algýlandý: " + item.gameObject.name);
        }
        else if (other.TryGetComponent(out CookingStation station))
        {
            // VE O ANDA ELÝMÝZ DOLU MU?
            if (_heldItem != null)
            {
                // O ZAMAN HÝÇ BEKLEMEDEN OTOMATÝK ETKÝLEÞÝME GÝR!
                Debug.Log("Ýstasyonla otomatik etkileþim tetiklendi.");
                station.Interact(this); // CookingStation'a görevi devret
            }
            else
            {
                // Elimiz boþsa, gelecekteki etkileþimler için sadece menzilde olduðunu kaydet
                _stationInRange = station;
                Debug.Log("Piþirme istasyonu alanýna girildi (el boþ).");
            }
        }
    }

    // El etkileþim alanýndan çýktýðýnda...
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet) && _cabinetInRange == cabinet)
        {
            _cabinetInRange = null;
            Debug.Log("Dolap alanýndan çýkýldý.");
        }
        else if (other.TryGetComponent(out GrabbableItem item) && _grabbableInRange == item)
        {
            _grabbableInRange = null;
            Debug.Log("Yerden alýnabilir obje menzilden çýktý.");
        }
        else if (other.TryGetComponent(out CookingStation station) && _stationInRange == station)
        {
            _stationInRange = null;
            Debug.Log("Piþirme istasyonu alanýndan çýkýldý.");
        }
    }

    // Elimizdeki objenin pozisyonunu her frame sonunda güncelleyerek takýlmayý önler.
    private void LateUpdate()
    {
        if (_heldItem != null && handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
    }

    // Input'tan gelen "Grab" eylemi bu metodu çaðýrýr.
    // HandInteractor.cs içindeki OnGrab metodunu bu kodla tamamen deðiþtirin

    public void OnGrab(InputAction.CallbackContext context)
    {
        // --- EYLEM 1: TUÞA BASILDIÐINDA (El Boþken) ---
        // Tuþa basýldýðýnda ve elimiz boþsa, bir þey almayý deneriz.
        if (context.performed && _heldItem == null)
        {
            // Öncelik 1: Yerde alýnabilir bir obje varsa onu al.
            if (_grabbableInRange != null)
            {
                _grabbableInRange.Interact(this);
            }
            // Öncelik 2: Yerde bir þey yoksa ve dolap menzilindeysek, dolaptan malzeme iste.
            else if (_cabinetInRange != null)
            {
                _cabinetInRange.RequestItem(this);
            }
        }
        // --- EYLEM 2: TUÞ BIRAKILDIÐINDA (El Doluyken) ---
        // Tuþ býrakýldýðýnda ve elimiz doluysa, elimizdekini bir yere býrakmayý/koymayý deneriz.
        else if (context.canceled && _heldItem != null)
        {
            // Öncelik 1: Býrakma anýnda bir piþirme istasyonunun menzilinde miyiz?
            if (_stationInRange != null)
            {
                // Evet, o zaman istasyonla etkileþime gir (malzemeyi istasyona koy).
                _stationInRange.Interact(this);
            }
            // Öncelik 2: Bir istasyon menzilinde deðilsek, objeyi normal bir þekilde yere býrak.
            else
            {
                ReleaseItem();
            }
        }
    }

    // Diðer script'lerin (Cabinet, GrabbableItem) eline obje vermesi için kullandýðý metot
    public void HoldItem(GameObject item)
    {
        _heldItem = item;
        _heldItemRb = _heldItem.GetComponent<Rigidbody>();

        // Fiziðini kapat
        if (_heldItemRb != null) _heldItemRb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;

        // Anýnda elin pozisyonuna ýþýnla
        if (handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }

        Debug.Log(_heldItem.name + " tutuluyor.");
    }

    // Elimizdeki objeyi býrakma metodu
    private void ReleaseItem()
    {
        if (_heldItem == null) return;
        Debug.Log(_heldItem.name + " býrakýldý.");

        // Fiziðini tekrar aç
        if (_heldItemRb != null) _heldItemRb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        // Referanslarý temizle
        _heldItem = null;
        _heldItemRb = null;
    }

    public GameObject GetHeldItem()
    {
        return _heldItem;
    }

    public void ClearHeldItem()
    {
        _heldItem = null;
        _heldItemRb = null;
        Debug.Log("El referanslarý temizlendi.");
    }
}