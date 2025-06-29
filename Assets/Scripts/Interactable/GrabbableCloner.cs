// GrabbableCloner.cs
using UnityEngine;

// Bu script, dolapta duran ve oyuncunun ilk etkileþime girdiði "görsel" objeye eklenir.
// Kendisi de etkileþime girilebilir olmalýdýr.
public class GrabbableCloner : Interactable
{
    // Klonlanacak olan malzemenin bilgisi (private, dýþarýdan eriþilmez)
    private Ingredient _ingredientToClone;

    // CabinetController bu metodu çaðýrarak hangi malzemenin klonlanacaðýný ayarlar.
    // 'Setup' metodunu eklediðimiz için diðer script'teki hata düzelecek.
    public void Setup(Ingredient ingredient)
    {
        _ingredientToClone = ingredient;
    }

    // Oyuncu bu görsel objeyi "almaya" çalýþtýðýnda bu metot çalýþýr.
    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log("GrabbableCloner.Interact metodu tetiklendi!");
        // Eðer klonlanacak malzeme bilgisi yoksa, bir hata mesajý ver ve iþlemi durdur.
        if (_ingredientToClone == null || _ingredientToClone.prefab == null)
        {
            Debug.LogError("Klonlanacak malzeme bilgisi ayarlanmamýþ!");
            return;
        }

        Debug.Log($"Klonlama talebi alýndý: {_ingredientToClone.ingredientName}");

        // Malzemenin prefab'ýndan YENÝ BÝR KOPYA (klon) oluþtur.
        GameObject clone = Instantiate(_ingredientToClone.prefab);

        // Bu yeni kopyaya, yere býrakýldýðýnda tekrar alýnabilmesi için
        // normal bir "tutulabilir" script'i ekle.
        clone.AddComponent<GrabbableItem>();

        // Yeni kopyayý (klonu) oyuncunun eline ver.
        interactor.HoldItem(clone);
    }
}