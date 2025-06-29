// CabinetController.cs
using UnityEngine;

// Bu script artýk Interactable deðil. Sadece bir yönetici.
public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Settings")]
    [Tooltip("Bu dolabýn içinde görünecek olan malzeme.")]
    [SerializeField] private Ingredient ingredientToShow;

    [Tooltip("Malzemenin duracaðý yer.")]
    [SerializeField] private Transform itemDisplayPoint;

    private void Start()
    {
        SpawnDisplayItem();
        Debug.Log(gameObject.name + " içine görsel obje spawn etti ve GrabbableCloner script'ini ekledi.");


    }

    private void SpawnDisplayItem()
    {
        if (ingredientToShow == null || ingredientToShow.prefab == null || itemDisplayPoint == null)
        {
            Debug.LogError("Cabinet display item için atamalar eksik!");
            return;
        }

        // Görsel objeyi oluþtur.
        GameObject displayItem = Instantiate(ingredientToShow.prefab, itemDisplayPoint.position, itemDisplayPoint.rotation);

        // Onu dolabýn bir parçasý yap (parent'ý yap).
        displayItem.transform.SetParent(itemDisplayPoint);

        // Bu objenin, kendisinin bir "kopyala ve al" objesi olduðunu bilmesi lazým.
        // Bunun için yeni bir script ekleyeceðiz.
        var cloner = displayItem.AddComponent<GrabbableCloner>();
        cloner.Setup(ingredientToShow); // Hangi malzemeyi klonlayacaðýný ona söylüyoruz.
    }
}