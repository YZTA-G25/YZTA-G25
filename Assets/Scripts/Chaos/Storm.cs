using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System.Collections.Generic;

public class Storm : ChaosBehaviour
{
    [Header("Storm Settings")]
    private Transform stormCenter;
    [SerializeField] private float stormRadius = 20f;
    [SerializeField] private float windForce = 50f;
    [SerializeField] private float upwardForce = 2f;

    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    public bool isEventActive { get; private set; }

    void Awake()
    {
        stormCenter = transform;
    }

    public override void StartChaosEvent()
    {
        Debug.Log("Fırtına Başladı");

        OnChaosEventStarted.Invoke();
        FindObjectsInStormArea();
        isEventActive = true;
    }

    public override void UpdateChaosEvent()
    {
        // This is empty because all our logic is Physics based
        // it is implemented in FixedUpdate
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody rb in affectedObjects)
        {
            if (rb != null)
            {
                Vector3 upwardDirection = Vector3.up * upwardForce;
                //Merkezden objeye doğru olan kuvvet yönü
                Vector3 radialDirection = (rb.transform.position - stormCenter.position).normalized;

                //Objenin dönmesini sağlayan merkeze doğru olana teğetsel kuvvet. (Sola ya da sağa doğru)
                Vector3 tangentialDirection = Vector3.Cross(radialDirection, Vector3.up);

                //Merkezkaç kuvveti
                Vector3 vortexForce = (tangentialDirection - radialDirection * 0.5f).normalized;

                rb.AddForce(vortexForce * windForce + upwardDirection, ForceMode.Force);

                if (rb.linearVelocity == Vector3.zero)
                {
                    rb.AddForce(vortexForce * 2, ForceMode.VelocityChange);
                }
            }
        }
    }

    public override void EndEvent()
    {
        Debug.Log("Fırtına Sona Erdi");
        OnChaosEventEnded.Invoke();
        isEventActive = false;
        affectedObjects.Clear();
    }

    private void FindObjectsInStormArea()
    {
        Collider[] colliders = Physics.OverlapSphere(stormCenter.position, stormRadius);
        Debug.Log($"[Storm Test] OverlapSphere found {colliders.Length} colliders.");

        foreach (Collider col in colliders)
        {
            var grabbable = col.GetComponent<GrabbableItem>();
            if (col.GetComponent<Rigidbody>() != null && grabbable != null && !grabbable.inFood)
            {
                affectedObjects.Add(col.GetComponent<Rigidbody>());
            }
        }
        Debug.Log($"[Storm Test] {affectedObjects.Count} rigidbodies were added to the affected list.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(stormCenter.position, stormRadius);
    }
}  