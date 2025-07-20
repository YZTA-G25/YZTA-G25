using UnityEngine;
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

    // Current grabbed object
    private GameObject grabbedObject;
    private Rigidbody grabbedRigidbody;
    private Vector3 grabPointOffset;
    
    // Original object properties
    private bool originalKinematic;
    private bool originalGravity;
    private Transform originalParent;
    
    // Hand velocity tracking for throwing
    private List<Vector3> handPositions = new List<Vector3>();
    private List<float> handTimes = new List<float>();
    
    // Legacy interaction variables
    private GrabbableItem grabbableInRange;
    private PageTurnButton buttonInRange;
    private CabinetController cabinetInRange;

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
            if (grabbedObject == null)
            {
                // Try new grabbing system first
                TryGrabObject();
                
                // Fallback to legacy interactions
                if (grabbedObject == null)
                {
                    HandleLegacyInteractions();
                }
            }
        }
        else if (context.canceled)
        {
            if (grabbedObject != null)
            {
                ReleaseObject();
            }
        }
    }

    #region New Kinematic Grabbing System

    private void TryGrabObject()
    {
        // Find closest grabbable object
        GameObject targetObject = FindClosestGrabbableObject();
        
        if (targetObject != null)
        {
            // Get exact contact point via raycast
            Vector3 contactPoint = GetContactPoint(targetObject);
            
            if (contactPoint != Vector3.zero)
            {
                GrabObjectAtPoint(targetObject, contactPoint);
            }
        }
    }

    private GameObject FindClosestGrabbableObject()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(handHoldPoint.position, grabRange, grabbableLayer);
        
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyObjects)
        {
            // Must have rigidbody to be grabbable
            if (col.GetComponent<Rigidbody>() == null) continue;
            
            float distance = Vector3.Distance(handHoldPoint.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = col.gameObject;
            }
        }
        
        Debug.Log(closest != null ? $"Found closest object: {closest.name} at distance {closestDistance}" 
                                : "No grabbable objects found in range");
        
        return closest;
    }

    private Vector3 GetContactPoint(GameObject targetObject)
    {
        // Raycast from hand to object for exact contact point
        Vector3 direction = (targetObject.transform.position - handHoldPoint.position).normalized;
        
        if (Physics.Raycast(handHoldPoint.position, direction, out RaycastHit hit, grabRange, grabbableLayer))
        {
            if (hit.collider.gameObject == targetObject)
            {
                Debug.Log($"Contact point found at: {hit.point}");
                
                // Draw debug visualization
                Debug.DrawRay(handHoldPoint.position, direction * hit.distance, Color.green, 1f);
                
                return hit.point;
            }
        }
        
        Debug.Log("No valid contact point found");
        return Vector3.zero;
    }

    private void GrabObjectAtPoint(GameObject targetObject, Vector3 contactPoint)
    {
        grabbedObject = targetObject;
        grabbedRigidbody = targetObject.GetComponent<Rigidbody>();
        
        // Store original properties
        originalKinematic = grabbedRigidbody.isKinematic;
        originalGravity = grabbedRigidbody.useGravity;
        originalParent = grabbedObject.transform.parent;
        
        // Calculate grab point offset in hand's local space
        grabPointOffset = handHoldPoint.InverseTransformPoint(contactPoint);
        
        // Make object kinematic and parent it to hand
        grabbedRigidbody.isKinematic = true;
        grabbedRigidbody.useGravity = false;
        grabbedObject.transform.SetParent(handHoldPoint);
        
        // Position object so contact point aligns with hand
        Vector3 desiredObjectPosition = handHoldPoint.position - (contactPoint - grabbedObject.transform.position);
        grabbedObject.transform.position = desiredObjectPosition;
        
        Debug.Log($"Grabbed {targetObject.name} at contact point {contactPoint}");
    }

    private void ReleaseObject()
    {
        if (grabbedObject == null) return;
        
        // Calculate release velocity
        Vector3 releaseVelocity = CalculateHandVelocity();
        bool shouldThrow = releaseVelocity.magnitude > velocityThreshold;
        
        // Restore original properties
        grabbedRigidbody.isKinematic = originalKinematic;

        grabbedObject.transform.localScale = originalScale;

        grabbedRigidbody.useGravity = originalGravity;
        grabbedObject.transform.SetParent(originalParent);
        
        // Apply momentum if throwing
        if (shouldThrow && !originalKinematic)
        {
            grabbedRigidbody.linearVelocity = releaseVelocity * throwForceMultiplier * Time.deltaTime;
            Debug.Log($"Threw {grabbedObject.name} with velocity: {releaseVelocity * throwForceMultiplier}");
        }
        else
        {
            // Just drop
            if (!originalKinematic)
            {
                grabbedRigidbody.linearVelocity = Vector3.zero;
                grabbedRigidbody.angularVelocity = Vector3.zero;
            }
            Debug.Log($"Dropped {grabbedObject.name}");
        }
        
        // Clear references
        grabbedObject = null;
        grabbedRigidbody = null;
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

    private Vector3 CalculateHandVelocity()
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

    #region Legacy Interaction System

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet))
        {
            cabinetInRange = cabinet;
            Debug.Log("Dolap alanına girildi: " + cabinet.gameObject.name);
        }
        else if (other.TryGetComponent(out GrabbableItem item))
        {
            grabbableInRange = item;
            Debug.Log("Yerden alınabilir obje algılandı: " + item.gameObject.name);
        }
        else if (other.TryGetComponent(out PageTurnButton button))
        {
            buttonInRange = button;
            Debug.Log("Defter butonu algılandı: " + button.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CabinetController cabinet) && cabinetInRange == cabinet)
        {
            cabinetInRange = null;
            Debug.Log("Dolap alanından çıkıldı.");
        }
        else if (other.TryGetComponent(out GrabbableItem item) && grabbableInRange == item)
        {
            grabbableInRange = null;
            Debug.Log("Yerden alınabilir obje menzilden çıktı.");
        }
        else if (other.TryGetComponent(out PageTurnButton button) && buttonInRange == button)
        {
            buttonInRange = null;
            Debug.Log("Defter butonu menzilden çıktı.");
        }
    }

    private void HandleLegacyInteractions()
    {
        if (buttonInRange != null)
        {
            buttonInRange.Interact(this);
        }
        else if (grabbableInRange != null)
        {
            grabbableInRange.Interact(this);
        }
    }

    // Legacy method for other scripts
    public void HoldItem(GameObject item)
    {
        if (grabbedObject != null) return;
        
        grabbedObject = item;
        grabbedRigidbody = item.GetComponent<Rigidbody>();
        
        if (grabbedRigidbody != null)
        {
            originalKinematic = grabbedRigidbody.isKinematic;
            originalGravity = grabbedRigidbody.useGravity;
            grabbedRigidbody.isKinematic = true;
            grabbedRigidbody.useGravity = false;
        }
        
        originalParent = item.transform.parent;
        item.transform.SetParent(handHoldPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Holding {item.name} (legacy mode)");
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
        if (grabbedObject != null)
        {
            ReleaseObject();
        }
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