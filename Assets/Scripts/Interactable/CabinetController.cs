// CabinetController.cs
using UnityEngine;

// Bu script art�k Interactable de�il. Sadece bir y�netici.
public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Settings")]
    [Tooltip("Bu dolab�n i�inde g�r�necek olan malzeme.")]
    [SerializeField] private Ingredient ingredientToShow;

    [Tooltip("Malzemenin duraca�� yer.")]
    [SerializeField] private Transform itemDisplayPoint;

    private void Start()
    {
        SpawnDisplayItem();
        Debug.Log(gameObject.name + " i�ine g�rsel obje spawn etti ve GrabbableCloner script'ini ekledi.");


    }

    private void SpawnDisplayItem()
    {
        if (ingredientToShow == null || ingredientToShow.prefab == null || itemDisplayPoint == null)
        {
            Debug.LogError("Cabinet display item i�in atamalar eksik!");
            return;
        }

        // G�rsel objeyi olu�tur.
        GameObject displayItem = Instantiate(ingredientToShow.prefab, itemDisplayPoint.position, itemDisplayPoint.rotation);

        // Onu dolab�n bir par�as� yap (parent'� yap).
        displayItem.transform.SetParent(itemDisplayPoint);

        // Bu objenin, kendisinin bir "kopyala ve al" objesi oldu�unu bilmesi laz�m.
        // Bunun i�in yeni bir script ekleyece�iz.
        var cloner = displayItem.AddComponent<GrabbableCloner>();
        cloner.Setup(ingredientToShow); // Hangi malzemeyi klonlayaca��n� ona s�yl�yoruz.
    }
}