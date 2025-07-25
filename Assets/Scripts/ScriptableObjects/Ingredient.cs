using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject
{
    [Tooltip("Bu malzemenin að üzerindeki eþsiz kimliði. 0'dan baþlayarak sýralý gidin.")]
    public ushort ingredientId; // ushort (0-65535) yeterli ve daha az yer kaplar
    public string ingredientName;
    public Sprite icon;
    public GameObject prefab;
    public bool isSafe;
    public float cookTime;
    public LayerMask layerMask;
}
