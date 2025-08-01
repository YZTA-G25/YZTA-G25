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
    [SerializeField] private float queueCheckDistance = 2f; // How far to raycast for queue detection
    [SerializeField] private LayerMask customerLayerMask = -1; // Layer mask for customer detection
    [SerializeField] private float visionAngle = 60f; // Total angle of vision cone in degrees
    [SerializeField] private int rayCount = 5; // Number of rays in the vision cone
    public Transform orderPosition;
    public Transform exitPosition;
    
    // Private variables
    private Recipe currentOrder;
    private bool canMove;
    private bool hasReachedOrderPosition = false;
    private bool isMovingToExit = false;
    private UnityEngine.AI.NavMeshAgent navAgent;
    private float originalSpeed;
    private int frameCounter = 0;
    private const int QUEUE_CHECK_INTERVAL = 50; // Check every 50 frames
    private float rayHeight = 1f;
    
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
        // Initialize NavMeshAgent
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            originalSpeed = navAgent.speed;
        }
        else
        {
            originalSpeed = moveSpeed;
        }
        
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
        
        // Increment frame counter for queue checking
        frameCounter++;
        
        HandleMovement();
        HandlePatience();
        
        // Check queue every 50 frames (for performance)
        if (frameCounter >= QUEUE_CHECK_INTERVAL)
        {
            HandleQueueing();
            frameCounter = 0;
        }
    }
    
    private void HandleQueueing()
    {
        // Only check queue when moving towards order position and not reached it yet
        if (isMovingToExit || hasReachedOrderPosition) return;
        
        // Multi-ray vision system to detect customers in a cone
        Vector3 rayOrigin = transform.position + Vector3.up * rayHeight;
        bool customerInFront = false;
        
        // Calculate angle step between rays
        float angleStep = rayCount > 1 ? visionAngle / (rayCount - 1) : 0f;
        float startAngle = -visionAngle / 2f;
        
        // Cast multiple rays in a cone formation
        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            
            // Calculate ray direction based on forward direction + angle offset
            Vector3 rayDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;

            // Cast the ray
            RaycastHit hit;
            bool hitDetected = Physics.Raycast(rayOrigin, rayDirection, out hit, queueCheckDistance, customerLayerMask);
            
            if (hitDetected)
            {
                customerInFront = true;
                Debug.Log($"[Customer] {gameObject.name} detected customer in ray {i} at angle {currentAngle:F1}° detected: {hit.collider.name}");
            }
            
            // Debug visualization for each ray
            Color rayColor = hitDetected ? Color.red : Color.green;
            Debug.DrawRay(rayOrigin, rayDirection * queueCheckDistance, rayColor, 1f);
        }
        
        // Apply movement logic based on detection results
        if (navAgent != null)
        {
            if (customerInFront)
            {
                // Stop moving - someone is in front
                navAgent.speed = 0f;
                canMove = false;
            }
            else
            {
                // Resume normal speed - path is clear
                navAgent.speed = originalSpeed;
                canMove = true;
            }
        }
        else
        {
            // Fallback for non-NavMesh movement
            if (customerInFront)
            {
                moveSpeed = 0f;
                canMove = false;
            }
            else
            {
                moveSpeed = originalSpeed;
                canMove = true;
            }
        }
    }

    private void HandleMovement()
    {
        if (canMove)
        {
            if (isMovingToExit)
            {
                // Move to exit
                if (exitPosition != null)
                {
                    if (navAgent != null)
                    {
                        // Use NavMeshAgent for pathfinding
                        navAgent.speed = originalSpeed; // Full speed when leaving
                        navAgent.SetDestination(exitPosition.position);

                        // Check if reached destination
                        if (!navAgent.pathPending && navAgent.remainingDistance < 0.1f)
                        {
                            // Customer has left, notify manager then despawn after a small delay
                            CustomerManager.Instance.OnCustomerLeft(this);
                            Invoke(nameof(DespawnSelf), 0.1f);
                        }
                    }
                    else
                    {
                        // Fallback to manual movement
                        Vector3 targetPosition = exitPosition.position;
                        Vector3 currentPosition = transform.position;

                        Vector3 direction = (targetPosition - currentPosition).normalized;
                        if (direction != Vector3.zero)
                        {
                            transform.rotation = Quaternion.LookRotation(direction);
                        }

                        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, originalSpeed * Time.deltaTime);

                        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                        {
                            CustomerManager.Instance.OnCustomerLeft(this);
                            Invoke(nameof(DespawnSelf), 0.1f);
                        }
                    }
                }
                return;
            }

            if (!hasReachedOrderPosition && orderPosition != null)
            {
                 navAgent.speed = originalSpeed; // Full speed when leaving
                // Move to order position
                if (navAgent != null)
                {
                    // Use NavMeshAgent for pathfinding
                    navAgent.SetDestination(orderPosition.position);

                    // Check if reached destination
                    if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
                    {
                        hasReachedOrderPosition = true;
                        navAgent.speed = 0f; // Stop moving when reached order position
                        CustomerManager.Instance.OnCustomerReachedOrderPosition(this);
                    }
                }
                else
                {
                    // Fallback to manual movement
                    Vector3 targetPosition = orderPosition.position;
                    Vector3 currentPosition = transform.position;

                    Vector3 direction = (targetPosition - currentPosition).normalized;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(direction);
                    }

                    transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                    {
                        hasReachedOrderPosition = true;
                        CustomerManager.Instance.OnCustomerReachedOrderPosition(this);
                    }
                }
            }
        }
        else
        {
            navAgent.speed = 0;
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
        if (navAgent != null)
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
        
        // Restore full speed for leaving
        if (navAgent != null)
        {
            navAgent.speed = originalSpeed;
        }
        else
        {
            moveSpeed = originalSpeed;
        }
        
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
        
        // Restore full speed for leaving
        if (navAgent != null)
        {
            navAgent.speed = originalSpeed;
        }
        else
        {
            moveSpeed = originalSpeed;
        }
        
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
    
    // Helper method to visualize queue detection in Scene view
    private void OnDrawGizmosSelected()
    {
        if (!isMovingToExit && !hasReachedOrderPosition)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * rayHeight;
            
            // Calculate angle step between rays
            float angleStep = rayCount > 1 ? visionAngle / (rayCount - 1) : 0f;
            float startAngle = -visionAngle / 2f;
            
            // Draw each ray in the vision cone
            for (int i = 0; i < rayCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                
                // Calculate ray direction based on forward direction + angle offset
                Vector3 rayDirection = Quaternion.AngleAxis(currentAngle, Vector3.up) * transform.forward;
                Vector3 rayEnd = rayOrigin + rayDirection * queueCheckDistance;
                
                // Set color based on ray position (center ray different color)
                Gizmos.color = (i == rayCount / 2) ? Color.yellow : Color.cyan;
                
                // Draw the ray line
                Gizmos.DrawLine(rayOrigin, rayEnd);
                
                // Draw a small sphere at the end of each ray
                Gizmos.DrawWireSphere(rayEnd, 0.05f);
            }
            
            // Draw vision cone edges to show the detection area
            if (rayCount > 1)
            {
                Gizmos.color = Color.white;
                Vector3 leftEdge = Quaternion.AngleAxis(-visionAngle / 2f, Vector3.up) * transform.forward;
                Vector3 rightEdge = Quaternion.AngleAxis(visionAngle / 2f, Vector3.up) * transform.forward;
                
                Gizmos.DrawLine(rayOrigin, rayOrigin + leftEdge * queueCheckDistance);
                Gizmos.DrawLine(rayOrigin, rayOrigin + rightEdge * queueCheckDistance);
            }
        }
    }
}
