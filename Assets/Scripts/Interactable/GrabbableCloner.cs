// GrabbableCloner.cs
using UnityEngine;

// Bu script, dolapta duran ve oyuncunun ilk etkile�ime girdi�i "g�rsel" objeye eklenir.
// Kendisi de etkile�ime girilebilir olmal�d�r.
public class GrabbableCloner : Interactable
{
    // Klonlanacak olan malzemenin bilgisi (private, d��ar�dan eri�ilmez)
    private Ingredient _ingredientToClone;

    // CabinetController bu metodu �a��rarak hangi malzemenin klonlanaca��n� ayarlar.
    // 'Setup' metodunu ekledi�imiz i�in di�er script'teki hata d�zelecek.
    public void Setup(Ingredient ingredient)
    {
        _ingredientToClone = ingredient;
    }

    // Oyuncu bu g�rsel objeyi "almaya" �al��t���nda bu metot �al���r.
    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log("GrabbableCloner.Interact metodu tetiklendi!");
        // E�er klonlanacak malzeme bilgisi yoksa, bir hata mesaj� ver ve i�lemi durdur.
        if (_ingredientToClone == null || _ingredientToClone.prefab == null)
        {
            Debug.LogError("Klonlanacak malzeme bilgisi ayarlanmam��!");
            return;
        }

        Debug.Log($"Klonlama talebi al�nd�: {_ingredientToClone.ingredientName}");

        // Malzemenin prefab'�ndan YEN� B�R KOPYA (klon) olu�tur.
        GameObject clone = Instantiate(_ingredientToClone.prefab);

        // Bu yeni kopyaya, yere b�rak�ld���nda tekrar al�nabilmesi i�in
        // normal bir "tutulabilir" script'i ekle.
        clone.AddComponent<GrabbableItem>();

        // Yeni kopyay� (klonu) oyuncunun eline ver.
        interactor.HoldItem(clone);
    }
}