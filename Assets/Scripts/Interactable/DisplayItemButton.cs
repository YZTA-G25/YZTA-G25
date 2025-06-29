// DisplayItemButton.cs
using UnityEngine;

// Bu da Interactable olmal� ki oyuncu onunla etkile�ime girebilsin.
public class DisplayItemButton : Interactable
{
    private Ingredient _myIngredient;
    private CabinetController _myCabinet;

    // CabinetController bu metodu �a��rarak bu g�rsel objeyi ayarlar.
    public void Setup(Ingredient ingredient, CabinetController cabinet)
    {
        _myIngredient = ingredient;
        _myCabinet = cabinet;
        // Bu objenin kendisi de tutulabilir olmal� ki oyuncu onu alg�las�n.
        if (!TryGetComponent(out Rigidbody rb))
        {
            var newRb = gameObject.AddComponent<Rigidbody>();
            newRb.isKinematic = true;
        }
    }

    // Oyuncu bu g�rsel objeyi "almaya" �al��t���nda bu metot �al���r.
    public override void Interact(HandInteractor interactor)
    {
        // Kopyay� kendisi olu�turmak yerine, ait oldu�u dolaptan istiyor.
        if (_myCabinet != null && _myIngredient != null)
        {
            _myCabinet.RequestItemClone(_myIngredient, interactor);
        }
    }
}