using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutaca�� nokta.")]
    [SerializeField] private Transform handHoldPoint;

    // Elin etkile�im alan�ndaki objeleri tutar
    private CookingStation _stationInRange;
    private CabinetController _cabinetInRange;
    private GrabbableItem _grabbableInRange;

    // Elin �u anda tuttu�u obje
    private GameObject _heldItem;
    private Rigidbody _heldItemRb;

    // El bir objenin etkile�im alan�na girdi�inde...
    private void OnTriggerEnter(Collider other)
    {
        // Girdi�i obje bir dolap m�?
        if (other.TryGetComponent(out CabinetController cabinet))
        {
            _cabinetInRange = cabinet;
            Debug.Log("Dolap alan�na girildi: " + cabinet.gameObject.name);
        }
        // Girdi�i obje yerden al�nabilir bir malzeme mi?
        else if (other.TryGetComponent(out GrabbableItem item))
        {
            _grabbableInRange = item;
            Debug.Log("Yerden al�nabilir obje alg�land�: " + item.gameObject.name);
        }
        else if (other.TryGetComponent(out CookingStation station))
        {
            // VE O ANDA EL�M�Z DOLU MU?
            if (_heldItem != null)
            {
                // O ZAMAN H�� BEKLEMEDEN OTOMAT�K ETK�LE��ME G�R!
                Debug.Log("�stasyonla otomatik etkile�im tetiklendi.");
                station.Interact(this); // CookingStation'a g�revi devret
            }
            else
            {
                // Elimiz bo�sa, gelecekteki etkile�imler i�in sadece menzilde oldu�unu kaydet
                _stationInRange = station;
                Debug.Log("Pi�irme istasyonu alan�na girildi (el bo�).");
            }
        }
    }

    // El etkile�im alan�ndan ��kt���nda...
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet) && _cabinetInRange == cabinet)
        {
            _cabinetInRange = null;
            Debug.Log("Dolap alan�ndan ��k�ld�.");
        }
        else if (other.TryGetComponent(out GrabbableItem item) && _grabbableInRange == item)
        {
            _grabbableInRange = null;
            Debug.Log("Yerden al�nabilir obje menzilden ��kt�.");
        }
        else if (other.TryGetComponent(out CookingStation station) && _stationInRange == station)
        {
            _stationInRange = null;
            Debug.Log("Pi�irme istasyonu alan�ndan ��k�ld�.");
        }
    }

    // Elimizdeki objenin pozisyonunu her frame sonunda g�ncelleyerek tak�lmay� �nler.
    private void LateUpdate()
    {
        if (_heldItem != null && handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
    }

    // Input'tan gelen "Grab" eylemi bu metodu �a��r�r.
    public void OnGrab(InputAction.CallbackContext context)
    {
        // Sadece tu�a tam bas�ld��� an� ('Performed' durumunu) dinle.
        // Tu�un b�rak�ld��� ('Canceled') veya yeni bas�ld��� ('Started') anlar� g�rmezden gel.
        if (!context.performed)
        {
            return;
        }

        // --- Durum 1: El Bo�sa ---
        // Elimiz bo�ken tu�a bas�ld�ysa, bir �ey ALMAYI deniyoruz.
        if (_heldItem == null)
        {
            // �ncelik s�ras�na g�re kontrol et:
            // �ncelik 1: Yerde al�nabilir bir obje var m�?
            if (_grabbableInRange != null)
            {
                _grabbableInRange.Interact(this);
            }
            // �ncelik 2: Dolap menzilinde miyiz?
            else if (_cabinetInRange != null)
            {
                _cabinetInRange.RequestItem(this);
            }
        }
        // --- Durum 2: El Doluysa ---
        // Elimiz doluyken tu�a bas�ld�ysa, elimizdekini bir yere BIRAKMAYI/KOYMAYI deniyoruz.
        else
        {
            // �ncelik s�ras�na g�re kontrol et:
            // �ncelik 1: Pi�irme istasyonunun menzilinde miyiz?
            if (_stationInRange != null)
            {
                _stationInRange.Interact(this); // �stasyonla etkile�ime gir (objeyi slota koy)
            }
            // �ncelik 2: Hi�bir etkile�im alan�nda de�ilsek, objeyi normal bir �ekilde yere b�rak
            else
            {
                ReleaseItem(); // Objeyi yere d���r
            }
        }
    }

    // Di�er script'lerin (Cabinet, GrabbableItem) eline obje vermesi i�in kulland��� metot
    public void HoldItem(GameObject item)
    {
        _heldItem = item;
        _heldItemRb = _heldItem.GetComponent<Rigidbody>();

        // Fizi�ini kapat
        if (_heldItemRb != null) _heldItemRb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;

        // An�nda elin pozisyonuna ���nla
        if (handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }

        Debug.Log(_heldItem.name + " tutuluyor.");
    }

    // Elimizdeki objeyi b�rakma metodu
    private void ReleaseItem()
    {
        if (_heldItem == null) return;
        Debug.Log(_heldItem.name + " b�rak�ld�.");

        // Fizi�ini tekrar a�
        if (_heldItemRb != null) _heldItemRb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        // Referanslar� temizle
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
        Debug.Log("El referanslar� temizlendi.");
    }
}