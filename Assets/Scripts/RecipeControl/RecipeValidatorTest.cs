using UnityEngine;
using System.Collections.Generic;

public class RecipeValidatorTest : MonoBehaviour
{
    public Recipe testRecipe;
    public List<Ingredient> correctIngredients;
    public List<Ingredient> wrongIngredients;

    private void Start()
    {
        Debug.Log("-TARÝF DOÐRULAMA BAÞLIYOR-");

        Debug.Log("TEST 1: Doðru Malzemeler ile kontrol ediliyor...");
        bool isCorrect = RecipeValidator.ValidateRecipe(testRecipe, correctIngredients);
        Debug.Log($"Sonuç: {(isCorrect ? "BAÞARILI (TRUE)" : "BAÞARISIZ (FALSE)")} -> Beklenen : TRUE");

        Debug.Log("TEST 2: Yanlýþ Malzemeler ile kontrol ediliyor...");
        bool isWrong = RecipeValidator.ValidateRecipe(testRecipe, wrongIngredients);
        Debug.Log($"Sonuç: {(isWrong ? "BAÞARILI (TRUE)" : "BAÞARISIZ (FALSE)")} -> Beklenen : FALSE");

        Debug.Log("-TEST BÝTTÝ-");
    }
}