using System.Collections.Generic;
using UnityEngine;

public class CookingStation : Interactable
{
    [Header("Recipe Settings")]
    [Tooltip("Bu istasyonda yapýlabilecek tariflerin listesi")]
    [SerializeField] private List<Recipe> availableRecipes;

    [Header("Station State")]
    [SerializeField] private List<Transform> ingredientSlots;
    public List<Ingredient> ingredientsInStation = new List<Ingredient>();
    private List<GameObject> displayItemsInStation = new List<GameObject>();

    public override void Interact(HandInteractor interactor)
    {
        GameObject heldItem = interactor.GetHeldItem();
        if (heldItem == null) return;

        if (heldItem.TryGetComponent<IngredientHolder>(out IngredientHolder ingredientHolder))
        {
            if (ingredientsInStation.Count >= ingredientSlots.Count) return;

            AddIngredientToStation(ingredientHolder.ingredientData, interactor);

            // Her yeni malzeme eklendiðinde, bir tarifin tamamlanýp tamamlanmadýðýný kontrol et
            CheckForCompletedRecipe();
        }
    }

    private void AddIngredientToStation(Ingredient ingredient, HandInteractor interactor)
    {
        ingredientsInStation.Add(ingredient);
        Transform spawnSlot = ingredientSlots[ingredientsInStation.Count - 1];

        // 1. Objeyi SADECE yarat, henüz bir yere koyma veya parent'lama.
        GameObject visualClone = Instantiate(ingredient.displayPrefab);

        // 2. Objeyi spawn noktasýnýn (slot'un) altýna "çocuk" yap.
        visualClone.transform.SetParent(spawnSlot);

        // 3. Parent'tan gelebilecek tüm pozisyon ve rotasyon bozulmalarýný SIFIRLA.
        visualClone.transform.localPosition = Vector3.zero;
        visualClone.transform.localRotation = Quaternion.identity;

        // 4. ÖLÇEÐÝ ÝSTEDÝÐÝMÝZ DEÐERE AYARLA.
        visualClone.transform.localScale = new Vector3(3f, 3f, 3f);

        // 5. Diðer adýmlar...
        displayItemsInStation.Add(visualClone);

        // Eldeki objeyi að üzerinden yok et
        interactor.DestroyHeldItemOnNetwork();
        interactor.ClearHeldItem();
    }

    private void CheckForCompletedRecipe()
    {
        // Mevcut tariflerden herhangi biri, istasyondaki malzemelerle eþleþiyor mu?
        foreach (Recipe recipe in availableRecipes)
        {
            // Senin saðladýðýn RecipeValidator'ý kullanarak kontrol ediyoruz
            if (RecipeValidator.ValidateRecipe(recipe, ingredientsInStation))
            {
                CompleteRecipe(recipe);
                return;
            }
        }
    }

    private void CompleteRecipe(Recipe completedRecipe)
    {
        // 1. Puan eklemesi için sunucuya istek gönder
        ScoringManager.Instance.AddScoreServerRpc(completedRecipe.scoreValue);

        // 2. Ýstasyonu bir sonraki tarif için temizle
        ClearStation();
    }

    private void ClearStation()
    {
        ingredientsInStation.Clear();
        foreach (GameObject displayItem in displayItemsInStation)
        {
            Destroy(displayItem);
        }
        displayItemsInStation.Clear();
    }
}