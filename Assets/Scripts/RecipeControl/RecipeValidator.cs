using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Collections;

public static class RecipeValidator
{
    public static bool ValidateRecipe(Recipe targetRecipe, List<Ingredient> submittedIngredients)
    {
        if (targetRecipe == null || targetRecipe.requiredIngredients == null || submittedIngredients == null)
        {
            Debug.LogError("Tarif veya malzeme listesi NULL olamaz.");
            return false;
        }

        if (targetRecipe.requiredIngredients.Count != submittedIngredients.Count)
        {
            return false;
        }

        var checklist = new List<string>(targetRecipe.requiredIngredients.Select(x => x.Name));

        foreach (var submittedIngredient in submittedIngredients)
        {
            if (checklist.Contains(submittedIngredient.Name) && submittedIngredient.isSafe)
            {
                checklist.Remove(submittedIngredient.Name);
            }
            else
            {
                return false;
            }
        }

        return checklist.Count == 0;
    }
}