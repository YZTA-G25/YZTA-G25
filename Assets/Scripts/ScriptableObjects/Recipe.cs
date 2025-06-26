using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> requiredIngredients;
    public Sprite recipeImage;
    [TextArea]
    public string cookingInstructions;
}
