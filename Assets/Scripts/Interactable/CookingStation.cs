using System.Collections.Generic;
using UnityEngine;

// G�rev 5: Interactable s�n�f�ndan miras al
public class CookingStation : Interactable
{
    // G�rev 8 i�in: Malzemelerin istasyon �zerinde g�r�nece�i slotlar�n listesi
    [SerializeField] private List<Transform> ingredientSlots;

    // G�rev 7c i�in: �stasyonun i�ine eklenen malzemelerin verilerini tutacak liste
    public List<Ingredient> ingredientsInStation = new List<Ingredient>();

    // G�rev 6: Etkile�im metodunu yaz
    public override void Interact(HandInteractor interactor)
    {
        // G�rev 7a: Oyuncunun elinde bir obje var m�?
        GameObject heldItem = interactor.GetHeldItem();

        if (heldItem != null)
        {
            // G�rev 7b: Elimizdeki obje, bir malzeme mi? (IngredientHolder etiketi var m�?)
            if (heldItem.TryGetComponent<IngredientHolder>(out IngredientHolder ingredientHolder))
            {
                // �stasyonun kapasitesi dolu mu diye kontrol edelim.
                if (ingredientsInStation.Count >= ingredientSlots.Count)
                {
                    Debug.Log("�stasyon dolu, daha fazla malzeme eklenemez.");
                    return; // Metoddan ��k, i�lem yapma.
                }

                // G�rev 7c: Malzemeyi istasyonun listesine ekle
                ingredientsInStation.Add(ingredientHolder.ingredientData);
                Debug.Log(ingredientHolder.ingredientData.name + " istasyona eklendi.");

                // G�rev 8: Malzemenin g�rsel kopyas�n� istasyonda olu�tur
                // 1. Do�ru slotu bul (s�radaki bo� slot)
                Transform spawnSlot = ingredientSlots[ingredientsInStation.Count - 1];

                // 2. G�rsel kopyay� yarat
                GameObject visualClone = Instantiate(ingredientHolder.ingredientData.prefab, spawnSlot.position, spawnSlot.rotation);

                // 3. Kopyan�n fizi�ini kapatarak sabit kalmas�n� sa�la
                if (visualClone.TryGetComponent<Rigidbody>(out Rigidbody rb)) { rb.isKinematic = true; }
                if (visualClone.TryGetComponent<Collider>(out Collider col)) { col.enabled = false; }

                // G�rev 7c (devam�): Orijinal objeyi oyuncunun elinden yok et ve elini bo�alt
                Destroy(heldItem);
                interactor.ClearHeldItem();
            }
        }
    }
}