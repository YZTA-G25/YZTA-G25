using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HandInteractor : MonoBehaviour
{
    [Header("Kinematic Grabbing Settings")]
    [Tooltip("Elin objeyi tutacağı nokta.")]
    [SerializeField] private Transform handHoldPoint;
    
    [Tooltip("Ne kadar uzağa erişebiliriz")]
    [SerializeField] private float grabRange = 1.5f;
    
    [Tooltip("Hangi layer'daki objeler tutulabilir")]
    [SerializeField] private LayerMask grabbableLayer = -1;
    
    [Tooltip("Fırlatma kuvveti çarpanı")]
    [SerializeField] private float throwForceMultiplier = 5f;
    
    [Tooltip("Bırakma/fırlatma hassasiyeti")]
    [SerializeField] private float velocityThreshold = 2f;
    
    [Tooltip("Hız takip örnek sayısı")]
    [SerializeField] private int velocitySamples = 5;

    // Current grabbed object (for tracking and IsHoldingSomething() method)
    private GameObject grabbedObject;
    private Rigidbody grabbedRigidbody;
    
    // Hand velocity tracking for throwing
    private List<Vector3> handPositions = new List<Vector3>();
    private List<float> handTimes = new List<float>();

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Track hand position for velocity calculation
        TrackHandVelocity();
    }

    public void OnGrab(InputAction.CallbackContext context)
    {   
        if (context.performed)
        {
            // Check which interaction triggered this
            if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
            {
                // Hold interaction - start grabbing
                TryGrabClosest();
            }
            else if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
            {
                // Tap interaction - single click
                TryInteractWithClosest();
            }
            else
            {
                // Fallback for default interaction
                TryInteractWithClosest();
            }
        }
        else if (context.canceled)
        {
            // Release any held object
            if (currentInteractable != null)
            {
                currentInteractable.Release();
            }
        }
    }

    private void TryGrabClosest()
    {
        // Find closest interactable object
        IInteractable closestInteractable = FindClosestInteractable();
        
        if (closestInteractable != null)
        {
            closestInteractable.Grab(this); // Use Grab instead of Interact
        }
        else
        {
            Debug.Log("No interactable objects found in range");
        }
    }

    #region IInteractable System

    private void TryInteractWithClosest()
    {
        // Find closest interactable object
        IInteractable closestInteractable = FindClosestInteractable();
        
        if (closestInteractable != null)
        {
            closestInteractable.Interact(this);
        }
        else
        {
            Debug.Log("No interactable objects found in range");
        }
    }

    private IInteractable FindClosestInteractable()
    {
        // Find objects detected by physics
        return FindClosestPhysicsInteractable();
    }

    private IInteractable FindClosestPhysicsInteractable()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(handHoldPoint.position, grabRange, grabbableLayer);
        
        IInteractable closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyObjects)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable == null) continue;
            
            float distance = Vector3.Distance(handHoldPoint.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }
        
        if (closest != null)
        {
            Debug.Log($"Found closest physics interactable: {((MonoBehaviour)closest).name} at distance {closestDistance}");
        }
        
        return closest;
    }

    #endregion

    #region Hand Velocity Tracking

    private void TrackHandVelocity()
    {
        // Add current position and time
        handPositions.Add(handHoldPoint.position);
        handTimes.Add(Time.time);
        
        // Remove old samples
        while (handPositions.Count > velocitySamples)
        {
            handPositions.RemoveAt(0);
            handTimes.RemoveAt(0);
        }
    }

    private Vector3 CalculateHandVelocityInternal()
    {
        if (handPositions.Count < 2) return Vector3.zero;
        
        // Calculate average velocity over recent samples
        Vector3 totalVelocity = Vector3.zero;
        int velocityCount = 0;
        
        for (int i = 1; i < handPositions.Count; i++)
        {
            float deltaTime = handTimes[i] - handTimes[i - 1];
            if (deltaTime > 0)
            {
                Vector3 velocity = (handPositions[i] - handPositions[i - 1]) / deltaTime;
                totalVelocity += velocity;
                velocityCount++;
            }
        }
        
        return velocityCount > 0 ? totalVelocity / velocityCount : Vector3.zero;
    }

    #endregion

    #region Utility Methods

    public bool IsHoldingSomething()
    {
        return grabbedObject != null;
    }

    public GameObject GetHeldObject()
    {
        return grabbedObject;
    }

    public void ForceRelease()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Release();
        }
    }

    // Public methods for IInteractable objects to access
    public Transform GetHandHoldPoint()
    {
        return handHoldPoint;
    }

    public float GetGrabRange()
    {
        return grabRange;
    }

    public LayerMask GetGrabbableLayer()
    {
        return grabbableLayer;
    }

    public float GetVelocityThreshold()
    {
        return velocityThreshold;
    }

    public float GetThrowForceMultiplier()
    {
        return throwForceMultiplier;
    }

    public Vector3 CalculateHandVelocity()
    {
        return CalculateHandVelocityInternal();
    }

    // Current interactable tracking
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        
        if (interactable != null && interactable is GrabbableItem grabbable)
        {
            grabbedObject = grabbable.gameObject;
            grabbedRigidbody = grabbable.GetComponent<Rigidbody>();
        }
    }

    public void ClearCurrentInteractable()
    {
        currentInteractable = null;
        grabbedObject = null;
        grabbedRigidbody = null;
    }

    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw grab range
        if (handHoldPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handHoldPoint.position, grabRange);
        }
    }
}