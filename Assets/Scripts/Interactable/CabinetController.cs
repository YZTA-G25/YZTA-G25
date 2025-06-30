// CabinetController.cs (SAÐLAM VE NÝHAÝ VERSÝYON)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Contents")]
    [Tooltip("Bu dolabýn içinde görünecek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> availableIngredients;

    [Tooltip("Her malzemenin duracaðý yerlerin listesi. Sayýsý malzeme listesiyle ayný olmalý.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    // Bu dolabýn ana etkileþim alaný
    private Collider interactionTrigger;

    private void Awake()
    {
        // Dolabýn üzerindeki ana Collider'ý al ve Trigger moduna ayarla
        interactionTrigger = GetComponent<Collider>();
        if (interactionTrigger != null)
        {
            interactionTrigger.isTrigger = true;
        }
    }

    private void Start()
    {
        CreateDisplayItems();
    }

    // Vitrin malzemelerini oluþturan metot
    private void CreateDisplayItems()
    {
        if (availableIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolabýnda malzeme sayýsý ile spawn noktasý sayýsý uyuþmuyor!");
            return;
        }

        for (int i = 0; i < availableIngredients.Count; i++)
        {
            Ingredient ingredient = availableIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            GameObject displayItem = Instantiate(ingredient.prefab, spawnPoint.position, spawnPoint.rotation);
            displayItem.transform.SetParent(spawnPoint);

            // --- YENÝ EKLENEN VE SORUNU ÇÖZEN KISIM ---

            // 1. Görsel objenin Rigidbody'sini bul.
            Rigidbody itemRb = displayItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                // 2. Onu Kinematik yap ve yerçekimini kapat.
                // Bu, onun bir heykel gibi sabit kalmasýný ve aþaðý düþmemesini saðlar.
                itemRb.isKinematic = true;
                itemRb.useGravity = false;
            }

            // 3. Collider'ýný kapat.
            // Bu, elin yanlýþlýkla bu görsel objeyi algýlamasýný engeller.
            // El, sadece dolabýn ana trigger'ýný algýlamalý.
            Collider itemCollider = displayItem.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }
            // --- YENÝ KISIM BÝTTÝ ---
        }
    }

    // HandInteractor bu metodu çaðýrarak bir item talep edecek
    public void RequestItem(HandInteractor interactor)
    {
        if (availableIngredients.Count == 0) return;

        // Elin pozisyonuna en yakýn olan malzemeyi bul
        Ingredient closestIngredient = GetClosestIngredientTo(interactor.transform.position);

        if (closestIngredient != null)
        {
            Debug.Log($"{closestIngredient.ingredientName} için klonlama talebi alýndý.");
            GameObject clone = Instantiate(closestIngredient.prefab);
            clone.AddComponent<GrabbableItem>();
            interactor.HoldItem(clone);
        }
    }

    // Elin pozisyonuna en yakýn spawn noktasýný ve dolayýsýyla malzemeyi bulan metot
    private Ingredient GetClosestIngredientTo(Vector3 handPosition)
    {
        float closestDistance = float.MaxValue;
        Ingredient closestIngredient = null;

        for (int i = 0; i < itemDisplayPoints.Count; i++)
        {
            float distance = Vector3.Distance(handPosition, itemDisplayPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIngredient = availableIngredients[i];
            }
        }
        return closestIngredient;
    }
}