using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Ingredient[] requiredIngredients;
    [TextArea]
    public string cookingInstructions;
}
