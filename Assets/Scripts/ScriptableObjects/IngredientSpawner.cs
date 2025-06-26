using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    
    public Recipe currentRecipe;
    public IngredientSpawnPoint[] spawnPoints;


    void Start()
    {
        SpawnIngredients();
    }

    public void SpawnIngredients()
    {
        foreach (Ingredient ingredient in currentRecipe.requiredIngredients)
        {
            // Metoda artýk ingredient'in ismini deðil, kendisini yolluyoruz.
            Transform targetSpawnPoint = GetSpawnPointForIngredient(ingredient);
            if (targetSpawnPoint != null)
            {
                Instantiate(ingredient.prefab, targetSpawnPoint.position, Quaternion.identity);
            }
        }
    }

    // Metodun parametresini 'Ingredient' tipine çeviriyoruz.
    private Transform GetSpawnPointForIngredient(Ingredient targetIngredient)
    {
        foreach (var sp in spawnPoints)
        {
            // Artýk 'targetIngredient' deðiþkeni tanýmlý ve karþýlaþtýrma yapabiliriz.
            if (sp.ingredient == targetIngredient)
                return sp.spawnPoint;
        }
        return null;
    }


}

[System.Serializable]
public class IngredientSpawnPoint
{
    public Ingredient ingredient;
    public Transform spawnPoint;
}