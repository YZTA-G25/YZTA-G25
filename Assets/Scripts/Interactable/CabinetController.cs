// CabinetController.cs (Çoklu Obje Versiyonu)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Settings")]
    [Tooltip("Bu dolabýn içinde görünecek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> displayIngredients;

    [Tooltip("Her malzemenin duracaðý yerlerin listesi. Sayýsý malzeme listesiyle ayný olmalý.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    private void Start()
    {
        // Oyun baþladýðýnda, dolabýn içine tüm "görsel" malzemeleri spawn et.
        CreateDisplayItems();
    }

    private void CreateDisplayItems()
    {
        // Malzeme sayýsý ile spawn noktasý sayýsý eþleþmiyorsa hata ver.
        if (displayIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolabýnda malzeme sayýsý ile spawn noktasý sayýsý uyuþmuyor!");
            return;
        }

        for (int i = 0; i < displayIngredients.Count; i++)
        {
            Ingredient ingredient = displayIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            GameObject displayItem = Instantiate(ingredient.prefab, spawnPoint.position, spawnPoint.rotation);
            displayItem.transform.SetParent(spawnPoint);

            // Bu objeye, etkileþime girildiðinde kendisinin bir kopyasýný istemesini söyleyen bir script ekle.
            var itemButton = displayItem.AddComponent<DisplayItemButton>();
            // Bu "düðme", hangi malzemeyi ve hangi dolabý temsil ettiðini bilmeli.
            itemButton.Setup(ingredient, this);
        }
    }

    // Bu metot, içindeki bir görsel objeye týklandýðýnda çaðrýlacak.
    public void RequestItemClone(Ingredient ingredient, HandInteractor interactor)
    {
        Debug.Log($"{ingredient.ingredientName} için klonlama talebi alýndý.");

        // Malzemenin prefab'ýndan YENÝ BÝR KOPYA (klon) oluþtur.
        GameObject clone = Instantiate(ingredient.prefab);

        // Bu yeni kopyaya, yere býrakýldýðýnda tekrar alýnabilmesi için GrabbableItem ekle.
        clone.AddComponent<GrabbableItem>();

        // Yeni kopyayý (klonu) oyuncunun eline ver.
        interactor.HoldItem(clone);
    }
}