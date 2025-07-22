using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutacağı nokta.")]
    [SerializeField] private Transform handHoldPoint;

    // Elin etkileşim alanındaki objeleri tutar
    private CabinetController _cabinetInRange;
    private GrabbableItem _grabbableInRange;

    //Tarif defteri butonu için
    private PageTurnButton _buttonInRange;

    // Elin şu anda tuttuğu obje
    private GameObject _heldItem;
    private Rigidbody _heldItemRb;

    // El bir objenin etkileşim alanına girdiğinde...
    private void OnTriggerEnter(Collider other)
    {
        // Girdiği obje bir dolap mı?
        if (other.TryGetComponent(out CabinetController cabinet))
        {
            _cabinetInRange = cabinet;
            Debug.Log("Dolap alanına girildi: " + cabinet.gameObject.name);
        }
        // Girdiği obje yerden alınabilir bir malzeme mi?
        else if (other.TryGetComponent(out GrabbableItem item))
        {
            _grabbableInRange = item;
            Debug.Log("Yerden alınabilir obje algılandı: " + item.gameObject.name);
        }
        //Defterin butonu algılandı mı?
        else if (other.TryGetComponent(out PageTurnButton button))
        {
            _buttonInRange = button;
            Debug.Log("Defter butonu algılandı: " + button.gameObject.name);
        }
    }

    // El etkileşim alanından çıktığında...
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet) && _cabinetInRange == cabinet)
        {
            _cabinetInRange = null;
            Debug.Log("Dolap alanından çıkıldı.");
        }
        else if (other.TryGetComponent(out GrabbableItem item) && _grabbableInRange == item)
        {
            _grabbableInRange = null;
            Debug.Log("Yerden alınabilir obje menzilden çıktı.");
        }
        else if (other.TryGetComponent(out PageTurnButton button) && _buttonInRange == button)
        {
            _buttonInRange = null;
            Debug.Log("Defter butonu menzilden çıktı.");
        }
    }

    // Elimizdeki objenin pozisyonunu her frame sonunda güncelleyerek takılmayı önler.
    private void LateUpdate()
    {
        if (_heldItem != null && handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }
    }

    // Input'tan gelen "Grab" eylemi bu metodu çağırır.
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed) // Tuşa ilk basıldığında
        {
            if (_heldItem == null) // Eğer elimiz boşsa
            {
                // Elimiz bir defter butonunun menzilinde mi?
                if (_buttonInRange != null)
                {
                    Debug.Log("Grab tuşuna basıldı ve buton menzilde! Interact metodu çağrılıyor...");
                    // Evet, o zaman butonla etkileşime gir (sayfayı çevir).
                    _buttonInRange.Interact(this);
                }
                // Eğer buton menzilinde değilsek, diğer kontrollere geç.

                // Öncelik: Yerdeki bir objeyi al
                else if (_grabbableInRange != null)
                {
                    _grabbableInRange.Interact(this);
                }
                // Eğer yerde bir şey yoksa ama dolap alanındaysak, dolaptan iste
                else if (_cabinetInRange != null)
                {
                    _cabinetInRange.RequestItem(this);
                }
            }
        }
        else if (context.canceled) // Tuş bırakıldığında
        {
            if (_heldItem != null) // Eğer elimiz doluysa
            {
                ReleaseItem();
            }
        }
    }

    // Diğer script'lerin (Cabinet, GrabbableItem) eline obje vermesi için kullandığı metot
    public void HoldItem(GameObject item)
    {
        _heldItem = item;
        _heldItemRb = _heldItem.GetComponent<Rigidbody>();

        // Fiziğini kapat
        if (_heldItemRb != null) _heldItemRb.isKinematic = true;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = false;

        // Anında elin pozisyonuna ışınla
        if (handHoldPoint != null)
        {
            _heldItem.transform.position = handHoldPoint.position;
            _heldItem.transform.rotation = handHoldPoint.rotation;
        }

        Debug.Log(_heldItem.name + " tutuluyor.");
    }

    // Elimizdeki objeyi bırakma metodu
    private void ReleaseItem()
    {
        if (_heldItem == null) return;
        Debug.Log(_heldItem.name + " bırakıldı.");

        // Fiziğini tekrar aç
        if (_heldItemRb != null) _heldItemRb.isKinematic = false;
        if (_heldItem.TryGetComponent(out Collider col)) col.enabled = true;

        // Referansları temizle
        _heldItem = null;
        _heldItemRb = null;
    }
}