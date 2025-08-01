using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class CustomerUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject customerOrderPanelPrefab;
    public Transform orderPanelContainer;
    [SerializeField] private float panelSlideSpeed = 5f;
    
    [Header("Canvas Setup")]
    public Canvas customerOrderCanvas;
    
    private List<CustomerOrderPanel> orderPanels = new List<CustomerOrderPanel>();
    private List<Customer> trackedCustomers = new List<Customer>();
    private Dictionary<Customer, bool> customerOrderStates = new Dictionary<Customer, bool>(); // Track if customer has transitioned to active order
    private Dictionary<CustomerOrderPanel, Coroutine> activeAnimations = new Dictionary<CustomerOrderPanel, Coroutine>(); // Track active animations
    private bool isEyePlayer = false;
    private float lastCleanupTime = 0f;
    private const float CLEANUP_INTERVAL = 2f;

    private void Start()
    {
        // Wait for players to spawn, then check player type
        StartCoroutine(WaitForPlayersAndSetup());
    }
    
    private System.Collections.IEnumerator WaitForPlayersAndSetup()
    {
        Debug.Log("[CustomerUI] Waiting for players to spawn...");
        
        // Wait until we can detect the player type
        while (!TryCheckIfEyePlayer())
        {
            yield return new WaitForSeconds(0.1f); // Check every 100ms
        }
        
        Debug.Log($"[CustomerUI] Player type detected: isEyePlayer = {isEyePlayer}");
        SetupUI();
    }
    
    private bool TryCheckIfEyePlayer()
    {
        // Since there's no default player prefab, we need to find the RoleManager instead
        RoleManager[] roleManagers = FindObjectsByType<RoleManager>(FindObjectsSortMode.None);
        
        foreach (RoleManager roleManager in roleManagers)
        {
            // Check if this RoleManager belongs to the local client
            if (roleManager.IsOwner)
            {
                // If it's NOT a hand player, then it's an eye player
                isEyePlayer = !roleManager.isHandPlayer;
                Debug.Log($"[CustomerUI] Found local player: isEyePlayer = {isEyePlayer}");
                return true;
            }
        }
        
        // Fallback: Check if we're the host (usually EyePlayer)
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            isEyePlayer = true;
            Debug.Log($"[CustomerUI] Fallback to host detection: isEyePlayer = {isEyePlayer}");
            return true;
        }
        
        return false; // Haven't found player type yet
    }
    
    private void SetupUI()
    {
        Debug.Log($"[CustomerUI] Setting up UI - isEyePlayer: {isEyePlayer}");
        
        // Only show UI on EyePlayer's screen
        if (!isEyePlayer)
        {
            if (customerOrderCanvas != null)
                customerOrderCanvas.enabled = false;
            else
                gameObject.SetActive(false);
            Debug.Log("[CustomerUI] Not EyePlayer - UI disabled");
            return;
        }
        
        // Enable canvas for EyePlayer
        if (customerOrderCanvas != null)
            customerOrderCanvas.enabled = true;
            
        Debug.Log("[CustomerUI] EyePlayer detected - enabling UI and subscribing to events");
        
        // Subscribe to customer events
        SubscribeToCustomerEvents();
    }

    private void OnDestroy()
    {
        // Stop all active animations
        foreach (var animation in activeAnimations.Values)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }
        activeAnimations.Clear();
        
        // Unsubscribe from events to prevent memory leaks
        UnsubscribeFromCustomerEvents();
    }
    
    private void SubscribeToCustomerEvents()
    {
        Debug.Log("[CustomerUI] Attempting to subscribe to customer events");
        
        // If CustomerManager is not ready yet, try again next frame
        if (CustomerManager.Instance == null)
        {
            Debug.Log("[CustomerUI] CustomerManager not ready, waiting...");
            StartCoroutine(WaitAndSubscribe());
            return;
        }
        
        Debug.Log("[CustomerUI] CustomerManager found, subscribing to events");
        CustomerManager.Instance.OnCustomerSpawned.AddListener(OnCustomerSpawned);
        CustomerManager.Instance.OnCustomerLeave.AddListener(OnCustomerLeft);
        CustomerManager.Instance.OnCustomerReachedOrder.AddListener(OnCustomerReachedOrder);
        Debug.Log("[CustomerUI] Successfully subscribed to customer events");
    }
    
    private System.Collections.IEnumerator WaitAndSubscribe()
    {
        // Wait until CustomerManager is available
        while (CustomerManager.Instance == null)
        {
            yield return null;
        }
        
        CustomerManager.Instance.OnCustomerSpawned.AddListener(OnCustomerSpawned);
        CustomerManager.Instance.OnCustomerLeave.AddListener(OnCustomerLeft);
        CustomerManager.Instance.OnCustomerReachedOrder.AddListener(OnCustomerReachedOrder);
    }
    
    private void UnsubscribeFromCustomerEvents()
    {
        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.OnCustomerSpawned.RemoveListener(OnCustomerSpawned);
            CustomerManager.Instance.OnCustomerLeave.RemoveListener(OnCustomerLeft);
            CustomerManager.Instance.OnCustomerReachedOrder.RemoveListener(OnCustomerReachedOrder);
        }
    }
    
    private void Update()
    {
        if (!isEyePlayer) return;
        
        // Only need to update timer colors
        UpdateTimerColors();
        
        // Periodically clean up any orphaned panels
        if (Time.time - lastCleanupTime >= CLEANUP_INTERVAL)
        {
            CleanupOrphanedPanels();
            lastCleanupTime = Time.time;
        }
    }
    
    // Event handlers
    private void OnCustomerSpawned(Customer customer)
    {
        Debug.Log($"[CustomerUI] OnCustomerSpawned called - isEyePlayer: {isEyePlayer}, customer: {customer?.name}");
        
        if (!isEyePlayer) 
        {
            Debug.Log("[CustomerUI] Not EyePlayer, ignoring customer spawn");
            return;
        }
        
        // Add panel for any customer that gets spawned
        // We'll check their state during CheckForOrderTransitions
        if (!trackedCustomers.Contains(customer))
        {
            Debug.Log($"[CustomerUI] Adding panel for customer: {customer.name}");
            AddOrderPanel(customer);
        }
        else
        {
            Debug.Log($"[CustomerUI] Customer {customer.name} already tracked, skipping");
        }
    }
    
    private void OnCustomerLeft(Customer customer)
    {
        if (!isEyePlayer) return;
        
        Debug.Log($"[CustomerUI] OnCustomerLeft called for customer: {customer?.name}");
        
        // Get customer ID for reliable identification
        ulong customerId = 0;
        if (customer != null && customer.GetComponent<NetworkObject>() != null)
        {
            customerId = customer.GetComponent<NetworkObject>().NetworkObjectId;
        }
        
        // Remove panel for this customer
        for (int i = orderPanels.Count - 1; i >= 0; i--)
        {
            // Check for null panels and customers
            if (orderPanels[i] == null)
            {
                Debug.LogWarning($"[CustomerUI] Found null panel at index {i}, removing from list");
                orderPanels.RemoveAt(i);
                continue;
            }
            
            Customer panelCustomer = orderPanels[i].Customer;
            ulong panelCustomerId = orderPanels[i].CustomerId;
            
            // Try to match by customer reference first, then by ID
            bool isMatch = false;
            if (panelCustomer != null && customer != null && panelCustomer == customer)
            {
                isMatch = true;
                Debug.Log($"[CustomerUI] Found matching panel by reference for customer {customer?.name} at index {i}");
            }
            else if (customerId != 0 && panelCustomerId == customerId)
            {
                isMatch = true;
                Debug.Log($"[CustomerUI] Found matching panel by ID {customerId} for customer {customer?.name} at index {i}");
            }
            else if (panelCustomer == null)
            {
                Debug.LogWarning($"[CustomerUI] Panel at index {i} has null customer, removing panel");
                RemoveOrderPanel(i);
                continue;
            }
            
            if (isMatch)
            {
                Debug.Log($"[CustomerUI] Removing matching panel for customer {customer?.name} at index {i}");
                RemoveOrderPanel(i);
                break;
            }
        }
        
        Debug.Log($"[CustomerUI] After customer left cleanup, panels remaining: {orderPanels.Count}");
    }
    
    private void OnCustomerReachedOrder(Customer customer)
    {
        if (!isEyePlayer) return;
        
        Debug.Log($"[CustomerUI] OnCustomerReachedOrder called for customer: {customer?.name}");
        
        // Find the panel for this customer and transition it to active order
        foreach (CustomerOrderPanel panel in orderPanels)
        {
            if (panel != null && panel.Customer == customer)
            {
                // Check if customer hasn't transitioned yet
                bool hasTransitioned = customerOrderStates.ContainsKey(customer) && customerOrderStates[customer];
                
                if (!hasTransitioned)
                {
                    Debug.Log($"[CustomerUI] Transitioning panel for customer {customer.name} to active order");
                    panel.TransitionToActiveOrder();
                    customerOrderStates[customer] = true;
                }
                break;
            }
        }
    }
    
    private void AddOrderPanel(Customer customer)
    {
        Debug.Log($"[CustomerUI] AddOrderPanel called for {customer?.name}");
        Debug.Log($"[CustomerUI] Prefab: {customerOrderPanelPrefab?.name}, Container: {orderPanelContainer?.name}");
        
        if (customerOrderPanelPrefab == null || orderPanelContainer == null) 
        {
            Debug.LogError("[CustomerUI] Missing prefab or container!");
            return;
        }

        GameObject panelObj = Instantiate(customerOrderPanelPrefab, orderPanelContainer);
        Debug.Log($"[CustomerUI] Instantiated panel: {panelObj.name}");
        
        CustomerOrderPanel panel = panelObj.GetComponent<CustomerOrderPanel>();
        
        if (panel != null)
        {
            Debug.Log($"[CustomerUI] Setting up panel for customer: {customer.name}");
            panel.SetupPanel(customer);
            orderPanels.Add(panel);
            trackedCustomers.Add(customer);
            
            // Initialize customer order state as not transitioned
            customerOrderStates[customer] = false;
            
            Debug.Log($"[CustomerUI] Panel added successfully. Total panels: {orderPanels.Count}");
            
            // Set initial position immediately to prevent visual glitches
            int panelIndex = orderPanels.Count - 1;
            Vector3 startPosition = new Vector3(panelIndex * 200f, 0, 0);
            panel.transform.localPosition = startPosition;
            
            // Position new panel and animate it
            panel.SetTargetPosition(panelIndex);
            Coroutine animation = StartCoroutine(AnimatePanelToPosition(panel));
            activeAnimations[panel] = animation;
        }
    }
    
    private void RemoveOrderPanel(int index)
    {
        if (index < 0 || index >= orderPanels.Count) 
        {
            Debug.LogWarning($"[CustomerUI] RemoveOrderPanel called with invalid index {index}, list count: {orderPanels.Count}");
            return;
        }
        
        CustomerOrderPanel panel = orderPanels[index];
        Customer customer = panel?.Customer;
        
        Debug.Log($"[CustomerUI] Removing panel at index {index} for customer: {customer?.name}");
        
        // Stop any active animation for this panel
        if (panel != null && activeAnimations.ContainsKey(panel))
        {
            StopCoroutine(activeAnimations[panel]);
            activeAnimations.Remove(panel);
        }
        
        // Remove from lists and dictionary
        orderPanels.RemoveAt(index);
        if (customer != null && trackedCustomers.Contains(customer))
        {
            trackedCustomers.Remove(customer);
        }
        if (customer != null && customerOrderStates.ContainsKey(customer))
        {
            customerOrderStates.Remove(customer);
        }
        
        // Destroy the panel
        if (panel != null && panel.gameObject != null)
        {
            Destroy(panel.gameObject);
        }
        
        Debug.Log($"[CustomerUI] Panel destroyed. Remaining panels: {orderPanels.Count}");
        
        // Only move panels that come after the removed index (shift left)
        AnimateMovingPanels(index);
    }
    
    private void AnimateMovingPanels(int startIndex)
    {
        Debug.Log($"[CustomerUI] AnimateMovingPanels starting from index {startIndex}, total panels: {orderPanels.Count}");
        
        // Animate panels that need to move after a removal
        for (int i = startIndex; i < orderPanels.Count; i++)
        {
            if (orderPanels[i] != null)
            {
                Debug.Log($"[CustomerUI] Animating panel with customer: {orderPanels[i].Customer?.name} to position {i}");
                // Don't call SetTargetPosition - let AnimatePanelToPosition calculate it based on array index
                Coroutine animation = StartCoroutine(AnimatePanelToPosition(orderPanels[i]));
                if (orderPanels[i] != null)
                    activeAnimations[orderPanels[i]] = animation;
            }
            else
            {
                Debug.LogWarning($"[CustomerUI] Found null panel at index {i} during animation setup");
            }
        }
    }
    
    private System.Collections.IEnumerator AnimatePanelToPosition(CustomerOrderPanel panel)
    {
        if (panel == null) yield break;
        
        // Stop any existing animation for this panel
        if (activeAnimations.ContainsKey(panel))
        {
            StopCoroutine(activeAnimations[panel]);
            activeAnimations.Remove(panel);
        }
        
        // Calculate target position based on panel's index in the list
        int panelIndex = orderPanels.IndexOf(panel);
        if (panelIndex == -1) yield break; // Panel not found
        
        Vector3 targetPosition = new Vector3(panelIndex * 200f, 0, 0); // 200f matches panelWidth
        Transform panelTransform = panel.transform;
        
        Debug.Log($"[CustomerUI] Animating panel for {panel.Customer?.name} from {panelTransform.localPosition} to {targetPosition}");
        
        // Continue animating until panel reaches target
        float animationTime = 0f;
        Vector3 startPosition = panelTransform.localPosition;
        
        while (animationTime < 1f) // Use time-based animation instead of distance-based
        {
            animationTime += Time.deltaTime * panelSlideSpeed;
            panelTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, animationTime);
            yield return null;
        }
        
        // Ensure exact final position
        panelTransform.localPosition = targetPosition;
        Debug.Log($"[CustomerUI] Animation complete for panel {panel.Customer?.name} at position {targetPosition}");
        
        // Remove from active animations
        if (activeAnimations.ContainsKey(panel))
        {
            activeAnimations.Remove(panel);
        }
    }
    
    // Keep old methods for any edge cases
    private void ShiftPanelsAfterRemoval(int removedIndex)
    {
        // Only move panels that were after the removed panel (shift them left by 1)
        for (int i = removedIndex; i < orderPanels.Count; i++)
        {
            if (orderPanels[i] != null)
            {
                orderPanels[i].SetTargetPosition(i); // Their new position is their current index
            }
        }
    }
    
    // Keep the old method for any edge cases that might need full rearrangement
    private void ArrangePanels()
    {
        for (int i = 0; i < orderPanels.Count; i++)
        {
            if (orderPanels[i] != null)
            {
                orderPanels[i].SetTargetPosition(i);
            }
        }
    }
    
    private void UpdateTimerColors()
    {
        foreach (CustomerOrderPanel panel in orderPanels)
        {
            if (panel != null)
            {
                panel.UpdateTimerDisplay();
            }
        }
    }
    
    private void CleanupOrphanedPanels()
    {
        // Remove any panels that have null or destroyed customers
        for (int i = orderPanels.Count - 1; i >= 0; i--)
        {
            if (orderPanels[i] == null || orderPanels[i].Customer == null)
            {
                Debug.Log($"[CustomerUI] Cleaning up orphaned panel at index {i}");
                
                // Stop any active animation for this panel
                if (orderPanels[i] != null && activeAnimations.ContainsKey(orderPanels[i]))
                {
                    StopCoroutine(activeAnimations[orderPanels[i]]);
                    activeAnimations.Remove(orderPanels[i]);
                }
                
                // Remove from tracking lists
                if (orderPanels[i] != null)
                {
                    Customer customer = orderPanels[i].Customer;
                    if (trackedCustomers.Contains(customer))
                    {
                        trackedCustomers.Remove(customer);
                    }
                    if (customer != null && customerOrderStates.ContainsKey(customer))
                    {
                        customerOrderStates.Remove(customer);
                    }
                    
                    // Destroy the panel
                    if (orderPanels[i].gameObject != null)
                    {
                        Destroy(orderPanels[i].gameObject);
                    }
                }
                
                // Remove from list
                orderPanels.RemoveAt(i);
                
                // Rearrange remaining panels with proper animation
                AnimateMovingPanels(i);
            }
        }
    }
}
