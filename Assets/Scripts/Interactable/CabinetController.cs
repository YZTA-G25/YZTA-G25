// CabinetController.cs (SA�LAM VE N�HA� VERS�YON)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Contents")]
    [Tooltip("Bu dolab�n i�inde g�r�necek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> availableIngredients;

    [Tooltip("Her malzemenin duraca�� yerlerin listesi. Say�s� malzeme listesiyle ayn� olmal�.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    // Bu dolab�n ana etkile�im alan�
    private Collider interactionTrigger;

    private void Awake()
    {
        // Dolab�n �zerindeki ana Collider'� al ve Trigger moduna ayarla
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

    // Vitrin malzemelerini olu�turan metot
    private void CreateDisplayItems()
    {
        if (availableIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolab�nda malzeme say�s� ile spawn noktas� say�s� uyu�muyor!");
            return;
        }

        for (int i = 0; i < availableIngredients.Count; i++)
        {
            Ingredient ingredient = availableIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            GameObject displayItem = Instantiate(ingredient.prefab, spawnPoint.position, spawnPoint.rotation);
            displayItem.transform.SetParent(spawnPoint);

            // --- YEN� EKLENEN VE SORUNU ��ZEN KISIM ---

            // 1. G�rsel objenin Rigidbody'sini bul.
            Rigidbody itemRb = displayItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                // 2. Onu Kinematik yap ve yer�ekimini kapat.
                // Bu, onun bir heykel gibi sabit kalmas�n� ve a�a�� d��memesini sa�lar.
                itemRb.isKinematic = true;
                itemRb.useGravity = false;
            }

            // 3. Collider'�n� kapat.
            // Bu, elin yanl��l�kla bu g�rsel objeyi alg�lamas�n� engeller.
            // El, sadece dolab�n ana trigger'�n� alg�lamal�.
            Collider itemCollider = displayItem.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = false;
            }
            // --- YEN� KISIM B�TT� ---
        }
    }

    // HandInteractor bu metodu �a��rarak bir item talep edecek
    public void RequestItem(HandInteractor interactor)
    {
        if (availableIngredients.Count == 0) return;

        // Elin pozisyonuna en yak�n olan malzemeyi bul
        Ingredient closestIngredient = GetClosestIngredientTo(interactor.transform.position);

        if (closestIngredient != null)
        {
            Debug.Log($"{closestIngredient.ingredientName} i�in klonlama talebi al�nd�.");
            GameObject clone = Instantiate(closestIngredient.prefab);
            clone.AddComponent<GrabbableItem>();
            interactor.HoldItem(clone);
        }
    }

    // Elin pozisyonuna en yak�n spawn noktas�n� ve dolay�s�yla malzemeyi bulan metot
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