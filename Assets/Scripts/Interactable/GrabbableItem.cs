// GrabbableItem.cs - Network Synchronized Version
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GrabbableItem : NetworkBehaviour, IInteractable
{
    [Header("Grab Settings")]
    public bool inFood = false;
    [Tooltip("Custom grab point offset for this item")]
    public Vector3 customGrabOffset = Vector3.zero;

    // Network Variables for synchronization
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> networkIsGrabbed = new NetworkVariable<bool>();
    private NetworkVariable<ulong> grabbingClientId = new NetworkVariable<ulong>();

    // Current interaction state
    private HandInteractor currentInteractor;
    private Rigidbody itemRigidbody;
    private Collider itemCollider;

    // Transform tracking for grabbed objects
    private Transform targetTransform; // HandHoldPoint to follow
    private bool isBeingGrabbed = false;

    // Original object properties for restoration
    private bool originalKinematic;
    private bool originalGravity;
    private Transform originalParent;

    private void Awake()
    {
        itemRigidbody = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to network variable changes
        networkPosition.OnValueChanged += OnNetworkPositionChanged;
        networkIsGrabbed.OnValueChanged += OnNetworkGrabbedChanged;

        // Initialize network position
        if (IsServer)
        {
            networkPosition.Value = transform.position;
        }
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from network variable changes
        networkPosition.OnValueChanged -= OnNetworkPositionChanged;
        networkIsGrabbed.OnValueChanged -= OnNetworkGrabbedChanged;
    }

    private void OnNetworkPositionChanged(Vector3 oldPos, Vector3 newPos)
    {
        // Only apply network position if we're not the owner and object is grabbed
        // OR if server is handling physics and we're not the server
        if ((networkIsGrabbed.Value && !IsOwner) || (!networkIsGrabbed.Value && !IsServer))
        {
            transform.position = newPos;
        }
    }

    private void OnNetworkGrabbedChanged(bool oldValue, bool newValue)
    {
        // Handle grab state changes from network
        if (newValue && !isBeingGrabbed)
        {
            // Someone else grabbed this item
            SetupGrabbedState();
        }
        else if (!newValue && isBeingGrabbed)
        {
            // Someone else released this item
            RestoreOriginalState();
        }
    }

    private void Update()
    {
        // Track the HandHoldPoint position while grabbed - Use Update for responsive movement
        if (isBeingGrabbed && targetTransform != null && IsOwner)
        {
            Vector3 handPosition = targetTransform.position;

            // Direct position assignment for responsive feel
            transform.position = handPosition;

            // Update network position for other clients
            UpdateNetworkPositionServerRpc(handPosition);

            // Debug every 60 frames to avoid spam
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Tracking: {gameObject.name} - Hand: {handPosition}, Object: {transform.position}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNetworkPositionServerRpc(Vector3 newPosition)
    {
        // Only update network position if object is being grabbed
        if (networkIsGrabbed.Value)
        {
            networkPosition.Value = newPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TransferOwnershipToServerServerRpc(Vector3 finalPosition, Vector3 throwVelocity)
    {
        Debug.Log($"Server: Taking ownership of {gameObject.name} for physics simulation");

        // Set the object's position on the server
        transform.position = finalPosition;

        // Change ownership to server (server client ID is usually 0)
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);

        // Apply physics on the server
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.useGravity = true;
            itemRigidbody.WakeUp();

            // Apply throw force if there was one
            if (throwVelocity.magnitude > 2f)
            {
                itemRigidbody.AddForce(throwVelocity * 5f, ForceMode.Impulse);
                Debug.Log($"Server applied throw force: {throwVelocity.magnitude}");
            }
        }

        if (itemCollider != null)
        {
            itemCollider.enabled = true;
            itemCollider.isTrigger = false;
        }

        // Update network position one final time
        networkPosition.Value = finalPosition;

        Debug.Log("TransferOwnershipToServerServerRpc Completed");
    }

    public void Interact(HandInteractor interactor)
    {
        // GrabbableItem only responds to Grab (hold), not Interact (tap)
        Debug.Log($"GrabbableItem {gameObject.name} ignoring tap - use hold to grab");
    }

    public void Grab(HandInteractor interactor)
    {
        Debug.Log($"Grab called on {gameObject.name}");

        if (currentInteractor != null)
        {
            Debug.Log($"Already being held by {currentInteractor.name}");
            return; // Already being held
        }
        if (interactor.IsHoldingSomething())
        {
            Debug.Log($"Hand is full - holding: {interactor.GetHeldObject()?.name}");
            return; // Hand is full
        }

        Debug.Log($"Grabbing: {gameObject.name}");

        // Request ownership of this object
        if (IsSpawned && !IsOwner)
        {
            RequestOwnershipServerRpc();
        }


        // Set up local grab state
        SoundManager.PlaySound(SoundType.OBJECT_GRAB);

        currentInteractor = interactor;
        SetupGrabbedState();

        // Set up transform tracking
        targetTransform = interactor.GetHandHoldPoint();
        isBeingGrabbed = true;

        Debug.Log($"HandHoldPoint position: {targetTransform.position}");
        Debug.Log($"Object position before grab: {transform.position}");

        // Position object immediately at grab point for visual feedback
        transform.position = interactor.GetHandHoldPoint().position;

        // Notify the hand interactor
        interactor.SetCurrentInteractable(this);

        // Update network state
        UpdateGrabStateServerRpc(true, NetworkManager.Singleton.LocalClientId);

        Debug.Log($"Grabbed {gameObject.name}, will track HandHoldPoint");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ServerRpcParams rpcParams = default)
    {
        // Change ownership to the requesting client
        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGrabStateServerRpc(bool grabbed, ulong clientId)
    {
        networkIsGrabbed.Value = grabbed;
        grabbingClientId.Value = grabbed ? clientId : 0;
    }

    private void SetupGrabbedState()
    {
        // Store original properties for restoration (do this right before changing them)
        if (itemRigidbody != null)
        {
            originalKinematic = itemRigidbody.isKinematic;
            originalGravity = itemRigidbody.useGravity;

            // Make object kinematic for smooth tracking
            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
            // Reset velocity to prevent any residual physics forces
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }
        originalParent = transform.parent;

        // Disable collider temporarily to prevent physics conflicts during grab
        Collider itemCollider = GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }
    }

    public void Release()
    {
        Debug.Log($"Releasing: {gameObject.name}");

        // Stop transform tracking
        isBeingGrabbed = false;
        targetTransform = null;

        // Calculate throw velocity before clearing interactor
        Vector3 throwVelocity = Vector3.zero;
        if (currentInteractor != null)
        {
            throwVelocity = currentInteractor.CalculateHandVelocity();
        }

        // IMPORTANT: Transfer ownership back to server for physics simulation
        if (IsSpawned && IsOwner)
        {
            TransferOwnershipToServerServerRpc(transform.position, throwVelocity);
        }

        RestoreOriginalState(throwVelocity);

        // Clear interaction state
        if (currentInteractor != null)
        {
            currentInteractor.ClearCurrentInteractable();
        }
        currentInteractor = null;

        // Update network state
        UpdateGrabStateServerRpc(false, 0);

        Debug.Log($"Released {gameObject.name}");
    }

    private void RestoreOriginalState(Vector3 throwVelocity = default)
    {
        Debug.Log($"RestoreOriginalState called for {gameObject.name}");

        // Restore original physics properties
        if (itemRigidbody != null)
        {
            Debug.Log($"Before restore - isKinematic: {itemRigidbody.isKinematic}, useGravity: {itemRigidbody.useGravity}");

            itemRigidbody.isKinematic = originalKinematic;
            itemRigidbody.useGravity = originalGravity;

            // Wake up the rigidbody to ensure physics kicks in
            itemRigidbody.WakeUp();

            Debug.Log($"After restore - isKinematic: {itemRigidbody.isKinematic}, useGravity: {itemRigidbody.useGravity}");
            Debug.Log($"Original values were - isKinematic: {originalKinematic}, useGravity: {originalGravity}");

            // Apply throw velocity if hand is moving fast enough
            if (throwVelocity.magnitude > (currentInteractor?.GetVelocityThreshold() ?? 2f))
            {
                itemRigidbody.AddForce(throwVelocity * (currentInteractor?.GetThrowForceMultiplier() ?? 5f), ForceMode.Impulse);
                Debug.Log($"Threw {gameObject.name} with velocity: {throwVelocity.magnitude}");
            }
            else
            {
                Debug.Log($"No throw - velocity too low: {throwVelocity.magnitude}");
            }
        }

        // Re-enable collider
        Collider itemCollider = GetComponent<Collider>();
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
            itemCollider.isTrigger = false;
            Debug.Log($"Re-enabled collider for {gameObject.name}");
        }
        else
        {
            Debug.Log($"No collider found on {gameObject.name}");
        }

        // Restore original parent
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            Debug.Log($"Restored parent to {originalParent.name}");
        }
        else
        {
            transform.SetParent(null);
            Debug.Log($"Set parent to null (world space)");
        }

        Debug.Log("RestoreOriginalState completed");
    }

    // Utility methods
    public bool IsBeingHeld()
    {
        return currentInteractor != null || networkIsGrabbed.Value;
    }

    public HandInteractor GetCurrentInteractor()
    {
        return currentInteractor;
    }
}