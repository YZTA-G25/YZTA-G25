using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Events;

public class CustomerManager : NetworkBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [Header("Customer Settings")]
    [SerializeField] private GameObject[] customerPrefabs; // Multiple customer prefab variants
    [SerializeField] private CustomerOrderData orderData;
    [SerializeField] private Transform customerSpawnPoint;
    [SerializeField] private Transform customerOrderPosition;
    [SerializeField] private Transform customerExitPosition;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Events")]
    public UnityEvent<Customer> OnCustomerSpawned;
    public UnityEvent<Customer> OnCustomerLeave;
    public UnityEvent<Customer> OnCustomerReachedOrder;
    
    // Customer management
    private List<Customer> activeCustomers = new List<Customer>();
    private List<Customer> waitingCustomers = new List<Customer>(); // Customers who have placed orders
    private const int MAX_CUSTOMERS = 5;
    
    // Timing
    private float nextSpawnTime;
    
    // Recipe database for ID lookups
    private Dictionary<int, Recipe> recipeDatabase = new Dictionary<int, Recipe>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            enabled = false;
            return;
        }
        
        // Build recipe database
        BuildRecipeDatabase();
        
        // Set initial spawn timer
        SetNextSpawnTime();
        
        if (enableDebugLogs)
            Debug.Log("[CustomerManager] Started on server");
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // Check if we should spawn a new customer
        if (Time.time >= nextSpawnTime && CanSpawnNewCustomer())
        {
            SpawnCustomer();
            SetNextSpawnTime();
        }
    }
    
    private void BuildRecipeDatabase()
    {
        if (orderData == null || orderData.availableRecipes == null) 
        {
            Debug.LogError("[CustomerManager] OrderData or availableRecipes is null!");
            return;
        }
        
        recipeDatabase.Clear();
        for (int i = 0; i < orderData.availableRecipes.Length; i++)
        {
            if (orderData.availableRecipes[i] != null)
            {
                recipeDatabase[i] = orderData.availableRecipes[i];
            }
        }
        
        if (enableDebugLogs)
            Debug.Log($"[CustomerManager] Built recipe database with {recipeDatabase.Count} recipes");
    }
    
    private bool CanSpawnNewCustomer()
    {
        return activeCustomers.Count < MAX_CUSTOMERS;
    }
    
    private GameObject GetRandomCustomerPrefab()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("[CustomerManager] No customer prefabs available!");
            return null;
        }
        
        // Filter out null prefabs
        List<GameObject> validPrefabs = new List<GameObject>();
        foreach (GameObject prefab in customerPrefabs)
        {
            if (prefab != null)
            {
                validPrefabs.Add(prefab);
            }
        }
        
        if (validPrefabs.Count == 0)
        {
            Debug.LogError("[CustomerManager] No valid customer prefabs found!");
            return null;
        }
        
        // Return a random valid prefab
        int randomIndex = Random.Range(0, validPrefabs.Count);
        GameObject selectedPrefab = validPrefabs[randomIndex];
        
        if (enableDebugLogs)
            Debug.Log($"[CustomerManager] Selected customer prefab: {selectedPrefab.name}");
            
        return selectedPrefab;
    }
    
    private void SpawnCustomer()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("[CustomerManager] Customer prefabs array is null or empty!");
            return;
        }
        
        if (customerSpawnPoint == null)
        {
            Debug.LogError("[CustomerManager] Customer spawn point is null!");
            return;
        }
        
        // Select a random customer prefab
        GameObject selectedPrefab = GetRandomCustomerPrefab();
        if (selectedPrefab == null)
        {
            Debug.LogError("[CustomerManager] Selected customer prefab is null!");
            return;
        }
        
        // Instantiate customer
        GameObject customerObj = Instantiate(selectedPrefab);
        Customer customer = customerObj.GetComponent<Customer>();
        
        if (customer == null)
        {
            Debug.LogError("[CustomerManager] Customer prefab missing Customer component!");
            Destroy(customerObj);
            return;
        }
        
        // Set up customer's target positions
        customer.GetComponent<Customer>().orderPosition = customerOrderPosition;
        customer.GetComponent<Customer>().exitPosition = customerExitPosition;
        
        // Spawn on network FIRST
        NetworkObject networkObject = customerObj.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            
            // THEN set position via ServerRpc after spawning
            customer.SetPositionServerRpc(customerSpawnPoint.position);
            
            // Set up the customer's order and patience
            Recipe randomRecipe = orderData.GetRandomRecipe();
            float patienceTime = orderData.GetRandomPatienceTime();
            
            if (randomRecipe != null)
            {
                int recipeId = GetRecipeId(randomRecipe);
                customer.SetupCustomerServerRpc(recipeId, patienceTime);
                
                if (enableDebugLogs)
                    Debug.Log($"[CustomerManager] Spawned customer with order: {randomRecipe.recipeName}, patience: {patienceTime:F1}s");
            }
            
            // Add to active customers list
            activeCustomers.Add(customer);
            
            // Invoke customer spawned event
            OnCustomerSpawned?.Invoke(customer);
        }
        else
        {
            Debug.LogError("[CustomerManager] Customer prefab missing NetworkObject component!");
            Destroy(customerObj);
        }
    }
    
    private void SetNextSpawnTime()
    {
        float spawnInterval = orderData.GetRandomSpawnInterval();
        nextSpawnTime = Time.time + spawnInterval;
        
        if (enableDebugLogs)
            Debug.Log($"[CustomerManager] Next customer will spawn in {spawnInterval:F1} seconds");
    }
    
    private int GetRecipeId(Recipe recipe)
    {
        foreach (var kvp in recipeDatabase)
        {
            if (kvp.Value == recipe)
                return kvp.Key;
        }
        return -1;
    }
    
    public Recipe GetRecipeById(int recipeId)
    {
        recipeDatabase.TryGetValue(recipeId, out Recipe recipe);
        return recipe;
    }
    
    public void OnCustomerReachedOrderPosition(Customer customer)
    {
        if (!IsServer) return;
        
        // Customer has reached ordering position and is now waiting
        if (!waitingCustomers.Contains(customer))
        {
            waitingCustomers.Add(customer);
            if (enableDebugLogs)
                Debug.Log($"[CustomerManager] Customer reached order position. Now waiting: {waitingCustomers.Count}");
        }
        
        // Invoke event for UI updates
        OnCustomerReachedOrder?.Invoke(customer);
    }

    public void OnCustomerStartedLeaving(Customer customer)
    {
        if (!IsServer) return;

        // Remove from waiting list when they start leaving
        if (waitingCustomers.Contains(customer))
        {
            waitingCustomers.Remove(customer);
            if (enableDebugLogs)
                Debug.Log($"[CustomerManager] Customer started leaving. Still waiting: {waitingCustomers.Count}");
        }
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
        }
        OnCustomerLeave?.Invoke(customer);
    }
    
    public void OnCustomerLeft(Customer customer)
    {
        if (!IsServer) return;
        
        // Remove from all lists when customer completely leaves
        activeCustomers.Remove(customer);
        waitingCustomers.Remove(customer);
        
        // Invoke customer leave event
        OnCustomerLeave?.Invoke(customer);
        
        if (enableDebugLogs)
            Debug.Log($"[CustomerManager] Customer left. Active: {activeCustomers.Count}, Waiting: {waitingCustomers.Count}");
    }
    
    /// <summary>
    /// Called by CookingStation when a recipe is completed correctly
    /// </summary>
    public void TryDeliverOrder(Recipe completedRecipe)
    {
        if (!IsServer) return;
        
        // Find a waiting customer with matching order
        Customer targetCustomer = null;
        foreach (Customer customer in waitingCustomers)
        {
            if (customer.CurrentOrder == completedRecipe)
            {
                targetCustomer = customer;
                break;
            }
        }
        
        if (targetCustomer != null)
        {
            // Deliver the order
            targetCustomer.ReceiveCorrectOrder();
            if (enableDebugLogs)
                Debug.Log($"[CustomerManager] Delivered {completedRecipe.recipeName} to customer");
            return;
        }
        
        // No matching customer found
        if (enableDebugLogs)
            Debug.Log($"[CustomerManager] No customer found waiting for {completedRecipe.recipeName}");
    }
    
    /// <summary>
    /// Get all waiting customers for UI display
    /// </summary>
    public List<Customer> GetWaitingCustomers()
    {
        return new List<Customer>(waitingCustomers);
    }
    
    /// <summary>
    /// Get total number of active customers
    /// </summary>
    public int GetActiveCustomerCount()
    {
        return activeCustomers.Count;
    }
}
