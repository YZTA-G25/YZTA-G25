// Scripts/Interaction/Cabinet.cs
using UnityEngine;

// Interactable sýnýfýndan miras alýyoruz.
public class Cabinet : Interactable
{
    [Header("Cabinet Settings")]
    [SerializeField] private Ingredient ingredientToSpawn;

    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log($"Dolapla etkileþime girildi: {ingredientToSpawn.ingredientName}");

        // Yeni bir malzeme oluþtur.
        GameObject spawnedItem = Instantiate(ingredientToSpawn.prefab);

        // Oyuncunun eline tutmasý için yolla.
        interactor.HoldItem(spawnedItem);
    }
}