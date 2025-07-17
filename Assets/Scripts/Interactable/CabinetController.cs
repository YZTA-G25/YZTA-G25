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
            
            // Convert LayerMask to layer index (get the first set bit)
            int layerIndex = 0;
            int layerMaskValue = ingredient.layerMask.value;
            while (layerMaskValue > 1)
            {
                layerMaskValue >>= 1;
                layerIndex++;
            }
            displayItem.gameObject.layer = layerIndex;
            
            // --- UPDATED PHYSICS SETUP FOR GRABBING ---

            // 1. Setup Rigidbody for physics-based grabbing
            Rigidbody itemRb = displayItem.GetComponent<Rigidbody>();
            if (itemRb != null)
            {
                // Make it kinematic and disable gravity so it stays in place as display
                itemRb.isKinematic = true;
                itemRb.useGravity = false;
            }

            // 2. Keep collider enabled but make it a trigger for display items
            // This allows raycast detection while preventing physics interference
            Collider itemCollider = displayItem.GetComponent<Collider>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;  // Keep enabled for raycast detection
                itemCollider.isTrigger = false;  // Not trigger so raycast can hit it
            }
            
            // 3. Add a tag to identify this as a display item (optional)
            displayItem.tag = "Interactable";
            // --- UPDATED SECTION END ---
        }
    }

    // HandInteractor bu metodu cagirarak bir item talep edecek
    public void RequestItem(HandInteractor interactor)
    {
        if (availableIngredients.Count == 0) return;

        // Elin pozisyonuna en yakin olan malzemeyi bul
        Ingredient closestIngredient = GetClosestIngredientTo(interactor.transform.position);

        if (closestIngredient != null)
        {
            Debug.Log($"{closestIngredient.ingredientName} icin klonlama talebi alindi.");
            GameObject clone = Instantiate(closestIngredient.prefab);
            
            // Set up the clone for physics-based grabbing
            SetupGrabbableClone(clone, closestIngredient);
            
            interactor.HoldItem(clone);
        }
    }
    
    // Set up cloned objects for proper physics-based grabbing
    private void SetupGrabbableClone(GameObject clone, Ingredient ingredient)
    {
        // Convert LayerMask to layer index for the grabbable clone
        int layerIndex = 0;
        int layerMaskValue = ingredient.layerMask.value;
        while (layerMaskValue > 1)
        {
            layerMaskValue >>= 1;
            layerIndex++;
        }
        clone.gameObject.layer = layerIndex;
        
        // Ensure the clone has proper physics setup
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        if (cloneRb == null)
        {
            cloneRb = clone.AddComponent<Rigidbody>();
        }
        
        // Make it dynamic (not kinematic) so it can be grabbed
        cloneRb.isKinematic = false;
        cloneRb.useGravity = true;
        cloneRb.mass = 1f; // Set reasonable mass
        
        // Ensure collider is enabled and not a trigger
        Collider cloneCollider = clone.GetComponent<Collider>();
        if (cloneCollider != null)
        {
            cloneCollider.enabled = true;
            cloneCollider.isTrigger = false;
        }
        
        // Add GrabbableItem component for legacy compatibility
        if (clone.GetComponent<GrabbableItem>() == null)
        {
            clone.AddComponent<GrabbableItem>();
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