// DisplayItemButton.cs
using UnityEngine;

// Bu da Interactable olmalý ki oyuncu onunla etkileþime girebilsin.
public class DisplayItemButton : Interactable
{
    private Ingredient _myIngredient;
    private CabinetController _myCabinet;

    // CabinetController bu metodu çaðýrarak bu görsel objeyi ayarlar.
    public void Setup(Ingredient ingredient, CabinetController cabinet)
    {
        _myIngredient = ingredient;
        _myCabinet = cabinet;
        // Bu objenin kendisi de tutulabilir olmalý ki oyuncu onu algýlasýn.
        if (!TryGetComponent(out Rigidbody rb))
        {
            var newRb = gameObject.AddComponent<Rigidbody>();
            newRb.isKinematic = true;
        }
    }

    // Oyuncu bu görsel objeyi "almaya" çalýþtýðýnda bu metot çalýþýr.
    public override void Interact(HandInteractor interactor)
    {
        // Kopyayý kendisi oluþturmak yerine, ait olduðu dolaptan istiyor.
        if (_myCabinet != null && _myIngredient != null)
        {
            _myCabinet.RequestItemClone(_myIngredient, interactor);
        }
    }
}