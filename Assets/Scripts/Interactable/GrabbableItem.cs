// GrabbableItem.cs
using UnityEngine;
using Unity.Netcode;

public class GrabbableItem : NetworkBehaviour, IInteractable
{
    [Header("Grab Settings")]
    public bool inFood = false;
    [Tooltip("Custom grab point offset for this item")]
    public Vector3 customGrabOffset = Vector3.zero;

    // Current interaction state
    private HandInteractor currentInteractor;
    private Rigidbody itemRigidbody;
    private Vector3 grabPointOffset;
    
    // Original object properties for restoration
    private bool originalKinematic;
    private bool originalGravity;
    private Transform originalParent;
    private Vector3 originalScale;
    
    // Network variable to track original parent across network
    private NetworkVariable<ulong> originalParentNetworkId = new NetworkVariable<ulong>(0);

    private void Awake()
    {
        itemRigidbody = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    public void Interact(HandInteractor interactor)
    {
        if (currentInteractor != null) return; // Already being held
        if (interactor.IsHoldingSomething()) return; // Hand is full

        Debug.Log($"Grabbing: {gameObject.name}");
        
        currentInteractor = interactor;
        
        // Store original properties for restoration
        if (itemRigidbody != null)
        {
            originalKinematic = itemRigidbody.isKinematic;
            originalGravity = itemRigidbody.useGravity;
        }
        originalParent = transform.parent;
        
        // Store original parent NetworkObject ID for network restoration
        if (originalParent != null && IsServer)
        {
            NetworkObject originalParentNetworkObject = originalParent.GetComponent<NetworkObject>();
            if (originalParentNetworkObject != null)
            {
                originalParentNetworkId.Value = originalParentNetworkObject.NetworkObjectId;
            }
            else
            {
                originalParentNetworkId.Value = 0; // No NetworkObject parent
            }
        }

        // Calculate grab point - use custom offset or contact point
        Vector3 contactPoint;
        if (customGrabOffset != Vector3.zero)
        {
            contactPoint = transform.position + customGrabOffset;
        }
        else
        {
            contactPoint = GetContactPoint(interactor);
        }

        // Calculate grab point offset in hand's local space
        grabPointOffset = interactor.GetHandHoldPoint().InverseTransformPoint(contactPoint);

        // Make object kinematic and parent it to hand
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
        }

        // Position object so contact point aligns with hand (for immediate visual feedback)
        Vector3 desiredObjectPosition = interactor.GetHandHoldPoint().position - (contactPoint - transform.position);
        transform.position = desiredObjectPosition;

        // Handle networking - request server to parent the object
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && NetworkManager.Singleton != null)
        {
            // Before calling RequestParentingRpc, add these checks:
            if (!IsSpawned)
            {
                Debug.LogError("GrabbableItem is not spawned, cannot send RPC!");
                return;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogError("NetworkObject is not spawned, cannot send RPC!");
                return;
            }

            Debug.Log($"IsSpawned: {IsSpawned}, NetworkObject.IsSpawned: {networkObject.IsSpawned}");
            Debug.Log($"NetworkManager.IsListening: {NetworkManager.Singleton.IsListening}");
            
            Debug.Log("About to get handNetworkObject...");
            
            // Get the NetworkObject of the hand (player)
            NetworkObject handNetworkObject = interactor.GetComponentInParent<NetworkObject>();
            
            // Additional debugging - let's see what we're actually finding
            HandController handController = interactor.GetComponentInParent<HandController>();
            if (handController != null)
            {
                Debug.Log($"Found HandController: {handController.name}");
                Debug.Log($"HandController IsSpawned: {handController.IsSpawned}");
                Debug.Log($"HandController NetworkObjectId: {handController.NetworkObjectId}");
                
                NetworkObject handControllerNetworkObject = handController.GetComponent<NetworkObject>();
                if (handControllerNetworkObject != null)
                {
                    Debug.Log($"HandController has NetworkObject: {handControllerNetworkObject.IsSpawned}");
                }
                else
                {
                    Debug.LogError("HandController does not have a NetworkObject component!");
                }
            }
            else
            {
                Debug.LogError("Could not find HandController in parent!");
            }
            
            if (handNetworkObject != null)
            {
                Debug.Log($"Found handNetworkObject: {handNetworkObject.name}");
                Debug.Log($"handNetworkObject.IsSpawned: {handNetworkObject.IsSpawned}");
                Debug.Log($"handNetworkObject.NetworkObjectId: {handNetworkObject.NetworkObjectId}");
                
                if (!handNetworkObject.IsSpawned)
                {
                    Debug.LogWarning("Hand NetworkObject is not spawned! Using local-only parenting.");
                    // Fallback to local parenting when networking is not available
                    transform.SetParent(interactor.GetHandHoldPoint());
                    transform.localPosition = Vector3.zero;
                    
                    // Notify the hand interactor
                    interactor.SetCurrentInteractable(this);
                    Debug.Log($"Grabbed {gameObject.name} (local-only mode)");
                    return;
                }
                
                // Add try-catch to see if there's an exception
                try
                {
                    Debug.Log("About to get NetworkObjectId...");
                    ulong handNetworkObjectId = handNetworkObject.NetworkObjectId;
                    Debug.Log($"Got NetworkObjectId: {handNetworkObjectId}");
                    
                    // Calculate the desired local position relative to hand hold point after positioning
                    Vector3 localOffset = interactor.GetHandHoldPoint().InverseTransformPoint(transform.position);
                    Debug.Log($"Calculated localOffset: {localOffset}");
                    
                    // Request server to parent this object to the hand
                    Debug.Log("About to call RequestParentingRpc...");
                    RequestParentingRpc(handNetworkObjectId, localOffset);
                    Debug.Log("RequestParentingRpc called successfully!");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception occurred: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning("handNetworkObject is null!");
                // Fallback to regular parenting if no NetworkObject found on hand
                Debug.LogWarning($"No NetworkObject found on HandInteractor, using regular transform parenting");
                transform.SetParent(interactor.GetHandHoldPoint());
                transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            Debug.LogWarning($"networkObject: {networkObject}, NetworkManager.Singleton: {NetworkManager.Singleton}");
            transform.SetParent(interactor.GetHandHoldPoint());
            transform.localPosition = Vector3.zero;
        }

        // Notify the hand interactor
        interactor.SetCurrentInteractable(this);
        
        Debug.Log($"Grabbed {gameObject.name} at contact point {contactPoint}");
    }

    [Rpc(SendTo.Server)]
    private void RequestParentingRpc(ulong handNetworkObjectId, Vector3 localOffset)
    {
        // Only server can execute this
        if (!IsServer) return;

        // Find the hand NetworkObject
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(handNetworkObjectId, out NetworkObject handNetworkObject))
        {
            // Find the HandInteractor and its hold point
            HandInteractor handInteractor = handNetworkObject.GetComponentInChildren<HandInteractor>();
            if (handInteractor != null)
            {
                Transform handHoldPoint = handInteractor.GetHandHoldPoint();
                
                // Store original parent before parenting
                if (transform.parent != null)
                {
                    NetworkObject parentNetworkObject = transform.parent.GetComponentInParent<NetworkObject>();
                    if (parentNetworkObject != null)
                    {
                        originalParentNetworkId.Value = parentNetworkObject.NetworkObjectId;
                    }
                }
                
                // Store current world position and rotation before parenting
                Vector3 worldPosition = transform.position;
                Quaternion worldRotation = transform.rotation;
                
                // Server performs the parenting
                NetworkObject thisNetworkObject = GetComponent<NetworkObject>();
                if (thisNetworkObject != null && thisNetworkObject.TrySetParent(handHoldPoint))
                {
                    // Restore world position and rotation, then apply local offset
                    transform.position = worldPosition;
                    transform.rotation = worldRotation;
                    transform.localPosition = localOffset;
                    
                    Debug.Log($"Server: Successfully parented {gameObject.name} to {handHoldPoint.name}");
                }
                else
                {
                    Debug.LogError($"Server: Failed to parent {gameObject.name}");
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestUnparentingRpc()
    {
        // Only server can execute this
        if (!IsServer) return;

        // Server performs the unparenting
        NetworkObject thisNetworkObject = GetComponent<NetworkObject>();
        if (thisNetworkObject != null)
        {
            Transform targetParent = null;
            
            // Try to find the original parent NetworkObject
            if (originalParentNetworkId.Value != 0 && 
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(originalParentNetworkId.Value, out NetworkObject originalParentNetworkObject))
            {
                targetParent = originalParentNetworkObject.transform;
            }
            // If no network parent or not found, use null (world space)
            
            if (thisNetworkObject.TrySetParent(targetParent))
            {
                Debug.Log($"Server: Successfully unparented {gameObject.name}");
            }
            else
            {
                Debug.LogError($"Server: Failed to unparent {gameObject.name}");
            }
        }
    }

    public void Release()
    {
        if (currentInteractor == null) return; // Not being held

        Debug.Log($"Releasing: {gameObject.name}");

        // Calculate release velocity for throwing
        Vector3 releaseVelocity = currentInteractor.CalculateHandVelocity();
        bool shouldThrow = releaseVelocity.magnitude > currentInteractor.GetVelocityThreshold();

        // Restore original properties
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = originalKinematic;
            itemRigidbody.useGravity = originalGravity;
        }

        transform.localScale = originalScale;

        // Handle networking - request server to unparent the object
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null && NetworkManager.Singleton != null)
        {
            // Request server to unparent this object
            RequestUnparentingRpc();
        }
        else
        {
            transform.SetParent(originalParent);
        }

        // Apply momentum if throwing
        if (shouldThrow && !originalKinematic && itemRigidbody != null)
        {
            Vector3 throwVelocity = releaseVelocity * currentInteractor.GetThrowForceMultiplier() * Time.deltaTime;
            itemRigidbody.linearVelocity = throwVelocity;
            Debug.Log($"Threw {gameObject.name} with velocity: {throwVelocity}");
        }
        else
        {
            // Just drop
            if (!originalKinematic && itemRigidbody != null)
            {
                itemRigidbody.linearVelocity = Vector3.zero;
                itemRigidbody.angularVelocity = Vector3.zero;
            }
            Debug.Log($"Dropped {gameObject.name}");
        }

        // Clear references
        currentInteractor.ClearCurrentInteractable();
        currentInteractor = null;
    }

    private Vector3 GetContactPoint(HandInteractor interactor)
    {
        Transform handHoldPoint = interactor.GetHandHoldPoint();
        
        // Raycast from hand to object for exact contact point
        Vector3 direction = (transform.position - handHoldPoint.position).normalized;
        
        if (Physics.Raycast(handHoldPoint.position, direction, out RaycastHit hit, interactor.GetGrabRange(), interactor.GetGrabbableLayer()))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log($"Contact point found at: {hit.point}");
                return hit.point;
            }
        }
        
        // Fallback to object center
        Debug.Log("Using object center as contact point");
        return transform.position;
    }

    // Utility methods
    public bool IsBeingHeld()
    {
        return currentInteractor != null;
    }

    public HandInteractor GetCurrentInteractor()
    {
        return currentInteractor;
    }
}