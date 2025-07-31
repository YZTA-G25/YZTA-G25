using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public enum CustomerState
{
    Waiting,
    Happy,
    Angry,
    Leaving
}

public class Customer : NetworkBehaviour
{
    [Header("Customer Data")]
    private NetworkVariable<float> patienceTime = new NetworkVariable<float>(60f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private NetworkVariable<float> maxPatienceTime = new NetworkVariable<float>(60f, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private NetworkVariable<int> orderedRecipeId = new NetworkVariable<int>(-1, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private NetworkVariable<CustomerState> currentState = new NetworkVariable<CustomerState>(CustomerState.Waiting, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    [Header("Customer Settings")]
    [SerializeField] private float moveSpeed = 2f;
    public Transform orderPosition;
    public Transform exitPosition;
    
    // Private variables
    private Recipe currentOrder;
    private bool hasReachedOrderPosition = false;
    private bool isMovingToExit = false;
    
    // Properties for external access
    public float PatienceTime => patienceTime.Value;
    public float MaxPatienceTime => maxPatienceTime.Value;
    public Recipe CurrentOrder => currentOrder;
    public CustomerState State => currentState.Value;
    public float PatiencePercentage => maxPatienceTime.Value > 0 ? patienceTime.Value / maxPatienceTime.Value : 0f;

    public static UnityEvent OnCustomerSpawn = new UnityEvent(); 
    public static UnityEvent OnCustomerOrder = new UnityEvent();
    public static UnityEvent OnCustomerLeave = new UnityEvent();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Server handles movement and timer logic
            if (orderPosition == null)
            {
                Debug.LogError("Order position not set for customer!");
                return;
            }
        }

        // Subscribe to state changes for all clients
        currentState.OnValueChanged += OnStateChanged;
        orderedRecipeId.OnValueChanged += OnOrderChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        currentState.OnValueChanged -= OnStateChanged;
        orderedRecipeId.OnValueChanged -= OnOrderChanged;
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        HandleMovement();
        HandlePatience();
    }
    
    private void HandleMovement()
    {
        if (isMovingToExit)
        {
            // Move to exit
            if (exitPosition != null)
            {
                Vector3 targetPosition = exitPosition.position;
                Vector3 currentPosition = transform.position;
                
                // Calculate movement direction and face that direction
                Vector3 direction = (targetPosition - currentPosition).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
                
                transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
                
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    // Customer has left, notify manager then despawn after a small delay
                    CustomerManager.Instance.OnCustomerLeft(this);
                    Invoke(nameof(DespawnSelf), 0.1f); // Small delay to ensure event processing
                }
            }
            return;
        }
        
        if (!hasReachedOrderPosition && orderPosition != null)
        {
            // Move to order position
            Vector3 targetPosition = orderPosition.position;
            Vector3 currentPosition = transform.position;
            
            // Calculate movement direction and face that direction
            Vector3 direction = (targetPosition - currentPosition).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                hasReachedOrderPosition = true;
                // Customer has reached ordering position, they can now place their order
                CustomerManager.Instance.OnCustomerReachedOrderPosition(this);
            }
        }
    }
    
    private void HandlePatience()
    {
        if (!hasReachedOrderPosition || isMovingToExit) return;
        
        patienceTime.Value -= Time.deltaTime;
        
        if (patienceTime.Value <= 0)
        {
            // Customer ran out of patience
            currentState.Value = CustomerState.Angry;
            LeaveAngry();
        }
    }
    
    /// <summary>
    /// Server method to set up the customer with their order and patience
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetupCustomerServerRpc(int recipeId, float patience)
    {
        orderedRecipeId.Value = recipeId;
        patienceTime.Value = patience;
        maxPatienceTime.Value = patience;
        currentState.Value = CustomerState.Waiting;
    }
    
    /// <summary>
    /// Server method to set customer position (needed for NavMeshAgent)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SetPositionServerRpc(Vector3 position)
    {
        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var navAgent))
        {
            navAgent.Warp(position);
        }
        else
        {
            transform.position = position;
        }
    }
    
    /// <summary>
    /// Server method called when customer receives correct order
    /// </summary>
    public void ReceiveCorrectOrder()
    {
        if (!IsServer) return;
        
        currentState.Value = CustomerState.Happy;
        
        // Calculate tip based on remaining time
        float timeRemainingPercent = PatiencePercentage;
        int baseTip = 0;
        
        if (timeRemainingPercent > 0.5f)
        {
            baseTip = Mathf.RoundToInt(currentOrder.scoreValue * 0.2f); // 20% tip
        }
        else if (timeRemainingPercent > 0.2f)
        {
            baseTip = Mathf.RoundToInt(currentOrder.scoreValue * 0.05f); // 5% tip
        }
        // Below 20% = no tip
        
        int totalReward = currentOrder.scoreValue + baseTip;
        ScoringManager.Instance.AddScoreServerRpc(totalReward);
        
        Debug.Log($"Customer received correct order! Base: {currentOrder.scoreValue}, Tip: {baseTip}, Total: {totalReward}");
        
        // Leave happy after a short delay
        Invoke(nameof(LeaveHappy), 2f);
    }
    
    private void LeaveHappy()
    {
        if (!IsServer) return;
        
        currentState.Value = CustomerState.Leaving;
        isMovingToExit = true;
        OnCustomerLeave.Invoke();
        CustomerManager.Instance.OnCustomerStartedLeaving(this);
        
        // Play happy sound
        PlayCustomerSoundClientRpc(true);
    }
    
    private void LeaveAngry()
    {
        if (!IsServer) return;
        
        currentState.Value = CustomerState.Leaving;
        isMovingToExit = true;
        OnCustomerLeave.Invoke();
        CustomerManager.Instance.OnCustomerStartedLeaving(this);
        
        // Play angry sound
        PlayCustomerSoundClientRpc(false);
    }
    
    [ClientRpc]
    private void PlayCustomerSoundClientRpc(bool isHappy)
    {
        // Play appropriate sound effect
        if (isHappy)
        {
            // Play happy customer sound
            SoundManager.PlaySound(SoundType.RECIPE_COMPLETE);
        }
        else
        {
            // Play angry customer sound - you might need to add this to SoundType enum
            Debug.Log("Customer left angry - play angry sound");
        }
    }
    
    private void OnStateChanged(CustomerState previousState, CustomerState newState)
    {
        // Handle visual state changes on all clients
        switch (newState)
        {
            case CustomerState.Waiting:
                // Show waiting animation
                break;
            case CustomerState.Happy:
                // Show happy animation
                break;
            case CustomerState.Angry:
                // Show angry animation
                break;
            case CustomerState.Leaving:
                // Show leaving animation
                break;
        }
    }
    
    private void DespawnSelf()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
    
    private void OnOrderChanged(int previousRecipeId, int newRecipeId)
    {
        // Update the current order when recipe ID changes
        if (newRecipeId >= 0)
        {
            // Find the recipe from the database or CustomerManager
            currentOrder = CustomerManager.Instance.GetRecipeById(newRecipeId);
        }
    }
}
