using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public abstract class ChaosBehaviour : NetworkBehaviour
{
    public ChaosEvent eventData;

    public virtual void Initialize(ChaosEvent data)
    {
        this.eventData = data;
    }

    public abstract void StartChaosEvent();
    public UnityEvent OnChaosEventStarted = new UnityEvent();
    public abstract void UpdateChaosEvent(); //Manager will call this every frame
    public abstract void EndEvent();
    public UnityEvent OnChaosEventEnded = new UnityEvent();
}