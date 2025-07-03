using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
    public GameObject prefab;
    public bool isSafe;
    public float cookTime;


    public GameObject displayPrefab;
}
