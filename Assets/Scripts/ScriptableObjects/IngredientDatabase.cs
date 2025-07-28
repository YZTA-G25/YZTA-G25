using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Bu satırı ekleyin

[CreateAssetMenu(fileName = "IngredientDatabase", menuName = "ScriptableObjects/IngredientDatabase")]
public class IngredientDatabase : ScriptableObject
{
    [Tooltip("Oyundaki tüm Ingredient ScriptableObject'larını buraya sürükleyin.")]
    public List<Ingredient> allIngredients;

    // ID'ye göre hızlı arama yapmak için bir sözlük (dictionary)
    private Dictionary<ulong, Ingredient> ingredientDictionary;

    // ScriptableObject aktif olduğunda çalışır
    private void OnEnable()
    {
        // Sözlüğü oluştur ve doldur
        ingredientDictionary = new Dictionary<ulong, Ingredient>();
        foreach (var ingredient in allIngredients)
        {
            if (ingredient != null && !ingredientDictionary.ContainsKey(ingredient.ID))
            {
                ingredientDictionary.Add(ingredient.ID, ingredient);
            }
        }
    }

    // Dışarıdan ID ile Ingredient verisini almak için kullanılacak metot
    public Ingredient GetIngredientById(ulong id)
    {
        if (ingredientDictionary.TryGetValue(id, out Ingredient ingredient))
        {
            return ingredient;
        }
        return null; // ID bulunamazsa null döner
    }
}