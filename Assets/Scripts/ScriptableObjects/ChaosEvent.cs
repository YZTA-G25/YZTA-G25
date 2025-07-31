using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ChaosEventType
{
    Storm,
    Levitation,
    EyeDazzle,
    LoseBalance,
    Hallucination
}

public class ChaosEvent : ScriptableObject, IScriptableObject
{
    [Header("Event Info")]
    [SerializeField] private ulong _id;
    public ulong ID => _id;
    public string Name => name;
    public ChaosEventType eventType;
    [TextArea] public string description;
    public float duration;

    [Header("Event Logic")]
    [Tooltip("Bu olayın mantığını içeren prefab")]
    public GameObject behaviourPrefab;
}
