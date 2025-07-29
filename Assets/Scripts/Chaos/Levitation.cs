using System.Collections.Generic;
using System.Runtime.InteropServices;


#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

using UnityEngine;

public class Levitation : ChaosBehaviour
{
    [Header("Levitation Settings")]
    [Tooltip("Eşyaların ne kadar yükseğe ve ne hızla havalanacağını belirler.")]
    [SerializeField] private float levitationForce = 2f;

    [Tooltip("Eşyaların havada ne kadar rastgele süzüleceğini belirler.")]
    [SerializeField] private float driftForce = 0.5f;

    [Tooltip("Hangi alandaki eşyaların havalanacağını belirler")]
    [SerializeField] private float searchRadius = 10f;

    [Tooltip("Arama merkezi. Atama yapılmazsa objenin pozisyonunu kullanır.")]
    [SerializeField] private Transform searchCenter;

    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    private bool isEventActive = false;

    void Awake()
    {
        if (searchCenter == null)
        {
            searchCenter = transform;
        }
    }

    public override void StartChaosEvent()
    {
        Debug.Log("Levitation Başladı");
        OnChaosEventStarted.Invoke();
        FindNearbyObjects();
        isEventActive = true;
    }

    public override void UpdateChaosEvent()
    {
        //All our calculations are physics based, so we are using a FixedUpdate instead of this.
    }

    private void FixedUpdate()
    {
        if (!isEventActive) return;

        foreach (Rigidbody rb in affectedObjects)
        {
            if (rb != null)
            {
                Vector3 upwardForce = Vector3.up * levitationForce;
                Vector3 randomDrift = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * driftForce;

                rb.AddForce(upwardForce + randomDrift, ForceMode.Acceleration);
            }
        }
    }

    public override void EndEvent()
    {
        Debug.Log("Levitation Kaos Olayı Sona Erdi.");
        OnChaosEventEnded.Invoke();
        isEventActive = false;
        affectedObjects.Clear();
    }

    private void FindNearbyObjects()
    {
        Collider[] colliders = Physics.OverlapSphere(searchCenter.position, searchRadius);
        Debug.Log($"[Levitation] OverlapSphere {colliders.Length} collider buldu.");

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<Rigidbody>(out Rigidbody rb) && col.TryGetComponent<GrabbableItem>(out GrabbableItem grabbable) && !grabbable.inFood)
            {
                affectedObjects.Add(rb);
            }
        }
        Debug.Log($"[Levitation] Etkilenecek obje listesine {affectedObjects.Count} rigidbody eklendi.");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(searchCenter != null ? searchCenter.position : transform.position, searchRadius);
    }
}  
