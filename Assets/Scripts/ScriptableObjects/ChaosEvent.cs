using System.Collections.Generic;
using UnityEngine;

public enum ChaosEventType
{
    Storm,
    Levitation,
    EyeDazzle,
    LoseBalance,
    Hallucination
}

public class ChaosEvent : ScriptableObject
{
    [Header("Event Info")]
    public ChaosEventType eventType;
    [TextArea] public string description;
    public float duration;

    [Header("Event Logic")]
    [Tooltip("Bu olayın mantığını içeren prefab")]
    public GameObject behaviourPrefab;
}
