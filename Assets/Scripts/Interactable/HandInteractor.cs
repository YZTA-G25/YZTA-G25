using UnityEngine;
using UnityEngine.InputSystem;

public class HandInteractor : MonoBehaviour
{
    [Header("Holding Settings")]
    [Tooltip("Elin objeyi tutaca�� nokta.")]
    [SerializeField] private Transform handHoldPoint;

    // Elin etkile�im alan�ndaki objeleri tutar
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
        if (context.performed) // Tu�a ilk bas�ld���nda
        {
            if (_heldItem == null) // E�er elimiz bo�sa
            {
                // �ncelik: Yerdeki bir objeyi al
                if (_grabbableInRange != null)
                {
                    _grabbableInRange.Interact(this);
                }
                // E�er yerde bir �ey yoksa ama dolap alan�ndaysak, dolaptan iste
                else if (_cabinetInRange != null)
                {
                    _cabinetInRange.RequestItem(this);
                }
            }
        }
        else if (context.canceled) // Tu� b�rak�ld���nda
        {
            if (_heldItem != null) // E�er elimiz doluysa
            {
                ReleaseItem();
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
}