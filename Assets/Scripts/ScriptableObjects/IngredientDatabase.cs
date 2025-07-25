using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Bu satýrý ekleyin

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "ScriptableObjects/IngredientDatabase")]
public class IngredientDatabase : ScriptableObject
{
    [Tooltip("Oyundaki tüm Ingredient ScriptableObject'larýný buraya sürükleyin.")]
    public List<Ingredient> allIngredients;

    // ID'ye göre hýzlý arama yapmak için bir sözlük (dictionary)
    private Dictionary<ushort, Ingredient> ingredientDictionary;

    // ScriptableObject aktif olduðunda çalýþýr
    private void OnEnable()
    {
        // Sözlüðü oluþtur ve doldur
        ingredientDictionary = new Dictionary<ushort, Ingredient>();
        foreach (var ingredient in allIngredients)
        {
            if (ingredient != null && !ingredientDictionary.ContainsKey(ingredient.ingredientId))
            {
                ingredientDictionary.Add(ingredient.ingredientId, ingredient);
            }
        }
    }

    // Dýþarýdan ID ile Ingredient verisini almak için kullanýlacak metot
    public Ingredient GetIngredientById(ushort id)
    {
        if (ingredientDictionary.TryGetValue(id, out Ingredient ingredient))
        {
            return ingredient;
        }
        return null; // ID bulunamazsa null döner
    }
}