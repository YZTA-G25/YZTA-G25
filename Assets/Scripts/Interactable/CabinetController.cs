using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CabinetController : NetworkBehaviour
{
    [Header("Cabinet Contents")]
    [SerializeField] private List<Ingredient> availableIngredients;
    [SerializeField] private List<Transform> itemDisplayPoints;

    private Collider interactionTrigger;

    private void Awake()
    {
        interactionTrigger = GetComponent<Collider>();
        if (interactionTrigger != null)
        {
            interactionTrigger.isTrigger = true;
        }
    }

    // Art�k Coroutine'e gerek yok, Start() metoduna geri d�nebiliriz.
    private void Start()
    {
        CreateDisplayItems();
    }

    private void CreateDisplayItems()
    {
        if (availableIngredients.Count != itemDisplayPoints.Count) return;

        for (int i = 0; i < availableIngredients.Count; i++)
        {
            Ingredient ingredient = availableIngredients[i];
            Transform spawnPoint = itemDisplayPoints[i];

            // --- EN �NEML� DE����KL�K BURADA ---
            // A� �zellikli as�l prefab yerine, sadece g�rsel olan displayPrefab'� yarat�yoruz.
            GameObject displayItem = Instantiate(ingredient.displayPrefab, spawnPoint.position, spawnPoint.rotation);

            displayItem.transform.SetParent(spawnPoint);

            // Bu kopyalar�n �zerinde zaten NetworkObject olmad��� i�in
            // fizi�ini kapatmak sorun yaratmaz.
            if (displayItem.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            if (displayItem.TryGetComponent<Collider>(out Collider col))
            {
                col.enabled = false;
            }
        }
    }

    // --- Di�er metotlar (RequestItem, ServerRpc vs.) ayn� kalacak ---
    public void RequestItem(HandInteractor interactor)
    {
        RequestItemSpawnServerRpc(interactor.transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestItemSpawnServerRpc(Vector3 requesterPosition)
    {
        Ingredient ingredientToSpawn = GetClosestIngredientTo(requesterPosition);
        if (ingredientToSpawn == null) return;
        // Burada hala a� �zellikli as�l prefab'� spawn ediyoruz, bu do�ru.
        GameObject itemInstance = Instantiate(ingredientToSpawn.prefab, transform.position + transform.forward, Quaternion.identity);
        itemInstance.GetComponent<NetworkObject>().Spawn(true);
    }

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