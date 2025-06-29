// CabinetController.cs (�oklu Obje Versiyonu)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Settings")]
    [Tooltip("Bu dolab�n i�inde g�r�necek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> displayIngredients;

    [Tooltip("Her malzemenin duraca�� yerlerin listesi. Say�s� malzeme listesiyle ayn� olmal�.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    private void Start()
    {
        // Oyun ba�lad���nda, dolab�n i�ine t�m "g�rsel" malzemeleri spawn et.
        CreateDisplayItems();
    }

    private void CreateDisplayItems()
    {
        // Malzeme say�s� ile spawn noktas� say�s� e�le�miyorsa hata ver.
        if (displayIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolab�nda malzeme say�s� ile spawn noktas� say�s� uyu�muyor!");
            return;
        }

        for (int i = 0; i < displayIngredients.Count; i++)
        {
            Ingredient ingredient = displayIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            GameObject displayItem = Instantiate(ingredient.prefab, spawnPoint.position, spawnPoint.rotation);
            displayItem.transform.SetParent(spawnPoint);

            // Bu objeye, etkile�ime girildi�inde kendisinin bir kopyas�n� istemesini s�yleyen bir script ekle.
            var itemButton = displayItem.AddComponent<DisplayItemButton>();
            // Bu "d��me", hangi malzemeyi ve hangi dolab� temsil etti�ini bilmeli.
            itemButton.Setup(ingredient, this);
        }
    }

    // Bu metot, i�indeki bir g�rsel objeye t�kland���nda �a�r�lacak.
    public void RequestItemClone(Ingredient ingredient, HandInteractor interactor)
    {
        Debug.Log($"{ingredient.ingredientName} i�in klonlama talebi al�nd�.");

        // Malzemenin prefab'�ndan YEN� B�R KOPYA (klon) olu�tur.
        GameObject clone = Instantiate(ingredient.prefab);

        // Bu yeni kopyaya, yere b�rak�ld���nda tekrar al�nabilmesi i�in GrabbableItem ekle.
        clone.AddComponent<GrabbableItem>();

        // Yeni kopyay� (klonu) oyuncunun eline ver.
        interactor.HoldItem(clone);
    }
}