// GrabbableCloner.cs (GÜNCELLENMÝÞ HALÝ)
using System.Collections.Generic; // Listeler için bu satýr gerekli
using UnityEngine;

public class GrabbableCloner : Interactable
{
    // Artýk tek bir malzeme deðil, bir malzeme listesi tutuyor
    private List<Ingredient> _ingredientsToCloneFrom;

    // CabinetController bu metodu çaðýrarak klonlanabilecek malzemelerin listesini verir.
    public void Setup(List<Ingredient> ingredients)
    {
        _ingredientsToCloneFrom = ingredients;
    }

    public override void Interact(HandInteractor interactor)
    {
        if (_ingredientsToCloneFrom == null || _ingredientsToCloneFrom.Count == 0)
        {
            Debug.LogError("Klonlanacak malzeme listesi boþ!");
            return;
        }

        // Listeden rastgele bir malzeme seç
        Ingredient randomIngredient = _ingredientsToCloneFrom[Random.Range(0, _ingredientsToCloneFrom.Count)];

        Debug.Log($"Klonlama talebi alýndý, rastgele seçilen: {randomIngredient.ingredientName}");

        // Seçilen rastgele malzemenin prefab'ýndan YENÝ BÝR KOPYA (klon) oluþtur.
        GameObject clone = Instantiate(randomIngredient.prefab);
        clone.AddComponent<GrabbableItem>();
        interactor.HoldItem(clone);
    }
}