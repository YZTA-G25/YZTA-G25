using System.Collections.Generic;
using UnityEngine;

// Görev 5: Interactable sýnýfýndan miras al
public class CookingStation : Interactable
{
    // Görev 8 için: Malzemelerin istasyon üzerinde görüneceði slotlarýn listesi
    [SerializeField] private List<Transform> ingredientSlots;

    // Görev 7c için: Ýstasyonun içine eklenen malzemelerin verilerini tutacak liste
    public List<Ingredient> ingredientsInStation = new List<Ingredient>();

    // Görev 6: Etkileþim metodunu yaz
    public override void Interact(HandInteractor interactor)
    {
        // Görev 7a: Oyuncunun elinde bir obje var mý?
        GameObject heldItem = interactor.GetHeldItem();

        if (heldItem != null)
        {
            // Görev 7b: Elimizdeki obje, bir malzeme mi? (IngredientHolder etiketi var mý?)
            if (heldItem.TryGetComponent<IngredientHolder>(out IngredientHolder ingredientHolder))
            {
                // Ýstasyonun kapasitesi dolu mu diye kontrol edelim.
                if (ingredientsInStation.Count >= ingredientSlots.Count)
                {
                    Debug.Log("Ýstasyon dolu, daha fazla malzeme eklenemez.");
                    return; // Metoddan çýk, iþlem yapma.
                }

                // Görev 7c: Malzemeyi istasyonun listesine ekle
                ingredientsInStation.Add(ingredientHolder.ingredientData);
                Debug.Log(ingredientHolder.ingredientData.name + " istasyona eklendi.");

                // Görev 8: Malzemenin görsel kopyasýný istasyonda oluþtur
                // 1. Doðru slotu bul (sýradaki boþ slot)
                Transform spawnSlot = ingredientSlots[ingredientsInStation.Count - 1];

                // 2. Görsel kopyayý yarat
                GameObject visualClone = Instantiate(ingredientHolder.ingredientData.prefab, spawnSlot.position, spawnSlot.rotation);

                // 3. Kopyanýn fiziðini kapatarak sabit kalmasýný saðla
                if (visualClone.TryGetComponent<Rigidbody>(out Rigidbody rb)) { rb.isKinematic = true; }
                if (visualClone.TryGetComponent<Collider>(out Collider col)) { col.enabled = false; }

                // Görev 7c (devamý): Orijinal objeyi oyuncunun elinden yok et ve elini boþalt
                Destroy(heldItem);
                interactor.ClearHeldItem();
            }
        }
    }
}