using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject, IScriptableObject
{
    [Tooltip("Bu malzemenin ağ üzerindeki eşsiz kimliği.")]
    [SerializeField] private ulong _id;
    public ulong ID => _id;
    public string Name => name;
    public Image icon;
    public GameObject prefab;
    public bool isSafe;
    public float cookTime;
    public LayerMask layerMask;
}
