// CabinetController.cs (SIMPLIFIED SINGLE INGREDIENT VERSION)
using Unity.Netcode;
using UnityEngine;

public class CabinetController : NetworkBehaviour, IInteractable
{
    [Header("Cabinet Contents")]
    [Tooltip("Bu dolabın içindeki tek malzeme türü.")]
    [SerializeField] private Ingredient ingredient;

    private void Awake()
    {
        // Ensure cabinet has a collider for physics-based interaction
        Collider interactionCollider = GetComponent<Collider>();
        if (interactionCollider != null)
        {
            // Cabinet should have a normal collider for physics-based detection
            interactionCollider.isTrigger = false;
        }
    }

    public void Interact(HandInteractor interactor)
    {
        // Only spawn if we have an ingredient and player isn't holding anything
        if (ingredient == null || interactor.IsHoldingSomething())
        {
            Debug.Log("Cannot give ingredient: " + 
                    (ingredient == null ? "No ingredient set" : "Player already holding something"));
            return;
        }

        // Request server to spawn the ingredient
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // We're the server, spawn directly
            SpawnIngredientForPlayer(interactor);
        }
        else if (NetworkManager.Singleton != null)
        {
            // We're a client, request from server
            RequestIngredientRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            // Fallback for non-networked play
            SpawnIngredientForPlayer(interactor);
        }
    }

    public void Grab(HandInteractor interactor)
    {
    }

    public void Release()
    {
        // Cabinet doesn't need any release functionality
    }

    [Rpc(SendTo.Server)]
    private void RequestIngredientRpc(ulong requestingClientId)
    {
        // Find the requesting player's hand interactor
        // Look for the hand interactor belonging to this client
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (spawnedObject.OwnerClientId == requestingClientId)
            {
                HandInteractor handInteractor = spawnedObject.GetComponentInChildren<HandInteractor>();
                if (handInteractor != null && !handInteractor.IsHoldingSomething())
                {
                    SpawnIngredientForPlayer(handInteractor);
                    break;
                }
            }
        }
    }

    private void SpawnIngredientForPlayer(HandInteractor interactor)
    {
        if (ingredient?.prefab == null) return;

        Debug.Log($"Spawning {ingredient.ingredientName} for player");

        // Instantiate the ingredient
        GameObject ingredientInstance = Instantiate(ingredient.prefab);
        
        // Set up the ingredient for grabbing
        SetupIngredientObject(ingredientInstance);
        
        // If networked, spawn the object with the requesting client as owner
        NetworkObject networkObject = ingredientInstance.GetComponent<NetworkObject>();
        if (NetworkManager.Singleton != null && networkObject != null && 
            (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            // Get the client ID of the requesting player
            ulong requestingClientId = GetClientIdFromHandInteractor(interactor);
            
            // Spawn with the requesting client as owner so they can parent it
            networkObject.SpawnWithOwnership(requestingClientId, true);
        }

        // Give it to the player using the new interaction system
        GrabbableItem grabbableComponent = ingredientInstance.GetComponent<GrabbableItem>();
        if (grabbableComponent != null)
        {
            grabbableComponent.Interact(interactor);
        }
        else
        {
            // If no GrabbableItem component, add one
            grabbableComponent = ingredientInstance.AddComponent<GrabbableItem>();
            grabbableComponent.Interact(interactor);
        }
    }

    private ulong GetClientIdFromHandInteractor(HandInteractor interactor)
    {
        // Find the NetworkObject that contains this HandInteractor
        NetworkObject playerNetworkObject = interactor.GetComponentInParent<NetworkObject>();
        if (playerNetworkObject != null)
        {
            return playerNetworkObject.OwnerClientId;
        }
        
        // Fallback to server if we can't find the client ID
        Debug.LogWarning("Could not find client ID for HandInteractor, defaulting to server");
        return NetworkManager.ServerClientId;
    }

    private void SetupIngredientObject(GameObject ingredientObj)
    {
        // Set the correct layer
        if (ingredient.layerMask.value > 0)
        {
            int layerIndex = 0;
            int layerMaskValue = ingredient.layerMask.value;
            while (layerMaskValue > 1)
            {
                layerMaskValue >>= 1;
                layerIndex++;
            }
            ingredientObj.layer = layerIndex;
        }

        // Ensure proper physics setup
        Rigidbody rb = ingredientObj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = ingredientObj.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 1f;

        // Ensure collider is set up properly
        Collider col = ingredientObj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = false;
        }

        // Add GrabbableItem component if it doesn't exist
        if (ingredientObj.GetComponent<GrabbableItem>() == null)
        {
            ingredientObj.AddComponent<GrabbableItem>();
        }
    }

    // Validation in editor
    private void OnValidate()
    {
        if (ingredient == null)
        {
            Debug.LogWarning($"Cabinet '{name}' has no ingredient assigned!", this);
        }
    }
}