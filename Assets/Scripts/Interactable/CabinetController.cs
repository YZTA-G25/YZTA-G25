// CabinetController.cs (SAĞLAM VE NİHAİ VERSİYON)
using System.Collections.Generic;
using UnityEngine;

public class CabinetController : MonoBehaviour
{
    [Header("Cabinet Contents")]
    [Tooltip("Bu dolabın içinde görünecek malzemelerin listesi.")]
    [SerializeField] private List<Ingredient> availableIngredients;

    [Tooltip("Her malzemenin duracağı yerlerin listesi. Sayısı malzeme listesiyle aynı olmalı.")]
    [SerializeField] private List<Transform> itemDisplayPoints;

    // Bu dolabın ana etkileşim alanı
    private Collider interactionTrigger;

    private void Awake()
    {
        // Dolabın üzerindeki ana Collider'ı al ve Trigger moduna ayarla
        interactionTrigger = GetComponent<Collider>();
    }

    private void Start()
    {
        CreateDisplayItems();
    }

    // Vitrin malzemelerini oluşturan metot
    private void CreateDisplayItems()
    {
        if (availableIngredients.Count != itemDisplayPoints.Count)
        {
            Debug.LogError(gameObject.name + " dolabında malzeme sayısı ile spawn noktası sayısı uyuşmuyor!");
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

    // HandInteractor bu metodu çağırarak bir item talep edecek
    public void RequestItem(HandInteractor interactor)
    {
        if (availableIngredients.Count == 0) return;

        // Elin pozisyonuna en yakın olan malzemeyi bul
        Ingredient closestIngredient = GetClosestIngredientTo(interactor.transform.position);

        if (closestIngredient != null)
        {
            Debug.Log($"{closestIngredient.ingredientName} için klonlama talebi alındı.");
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

    // Elin pozisyonuna en yakın spawn noktasını ve dolayısıyla malzemeyi bulan metot
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