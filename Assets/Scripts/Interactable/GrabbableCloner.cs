// GrabbableCloner.cs (G�NCELLENM�� HAL�)
using System.Collections.Generic; // Listeler i�in bu sat�r gerekli
using UnityEngine;

public class GrabbableCloner : Interactable
{
    // Art�k tek bir malzeme de�il, bir malzeme listesi tutuyor
    private List<Ingredient> _ingredientsToCloneFrom;

    // CabinetController bu metodu �a��rarak klonlanabilecek malzemelerin listesini verir.
    public void Setup(List<Ingredient> ingredients)
    {
        _ingredientsToCloneFrom = ingredients;
    }

    public override void Interact(HandInteractor interactor)
    {
        if (_ingredientsToCloneFrom == null || _ingredientsToCloneFrom.Count == 0)
        {
            Debug.LogError("Klonlanacak malzeme listesi bo�!");
            return;
        }

        // Listeden rastgele bir malzeme se�
        Ingredient randomIngredient = _ingredientsToCloneFrom[Random.Range(0, _ingredientsToCloneFrom.Count)];

        Debug.Log($"Klonlama talebi al�nd�, rastgele se�ilen: {randomIngredient.ingredientName}");

        // Se�ilen rastgele malzemenin prefab'�ndan YEN� B�R KOPYA (klon) olu�tur.
        GameObject clone = Instantiate(randomIngredient.prefab);
        clone.AddComponent<GrabbableItem>();
        interactor.HoldItem(clone);
    }
}