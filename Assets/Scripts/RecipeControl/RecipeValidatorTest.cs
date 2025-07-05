using UnityEngine;
using System.Collections.Generic;

public class RecipeValidatorTest : MonoBehaviour
{
    public Recipe testRecipe;
    public List<Ingredient> correctIngredients;
    public List<Ingredient> wrongIngredients;

    private void Start()
    {
        Debug.Log("-TAR�F DO�RULAMA BA�LIYOR-");

        Debug.Log("TEST 1: Do�ru Malzemeler ile kontrol ediliyor...");
        bool isCorrect = RecipeValidator.ValidateRecipe(testRecipe, correctIngredients);
        Debug.Log($"Sonu�: {(isCorrect ? "BA�ARILI (TRUE)" : "BA�ARISIZ (FALSE)")} -> Beklenen : TRUE");

        Debug.Log("TEST 2: Yanl�� Malzemeler ile kontrol ediliyor...");
        bool isWrong = RecipeValidator.ValidateRecipe(testRecipe, wrongIngredients);
        Debug.Log($"Sonu�: {(isWrong ? "BA�ARILI (TRUE)" : "BA�ARISIZ (FALSE)")} -> Beklenen : FALSE");

        Debug.Log("-TEST B�TT�-");
    }
}