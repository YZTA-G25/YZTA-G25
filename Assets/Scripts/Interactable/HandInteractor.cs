using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutacaðý nokta.")]
    [SerializeField] private Transform handHoldPoint;

    // Elin etkileþim alanýndaki objeleri tutar
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
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed) // Tuþa ilk basýldýðýnda
        {
            if (_heldItem == null) // Eðer elimiz boþsa
            {
                // Öncelik: Yerdeki bir objeyi al
                if (_grabbableInRange != null)
                {
                    _grabbableInRange.Interact(this);
                }
                // Eðer yerde bir þey yoksa ama dolap alanýndaysak, dolaptan iste
                else if (_cabinetInRange != null)
                {
                    _cabinetInRange.RequestItem(this);
                }
            }
        }
        else if (context.canceled) // Tuþ býrakýldýðýnda
        {
            if (_heldItem != null) // Eðer elimiz doluysa
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
}