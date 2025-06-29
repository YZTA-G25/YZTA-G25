// Scripts/Interaction/Cabinet.cs
using UnityEngine;

// Interactable s�n�f�ndan miras al�yoruz.
public class Cabinet : Interactable
{
    [Header("Cabinet Settings")]
    [SerializeField] private Ingredient ingredientToSpawn;

    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log($"Dolapla etkile�ime girildi: {ingredientToSpawn.ingredientName}");

        // Yeni bir malzeme olu�tur.
        GameObject spawnedItem = Instantiate(ingredientToSpawn.prefab);

        // Oyuncunun eline tutmas� i�in yolla.
        interactor.HoldItem(spawnedItem);
    }
}