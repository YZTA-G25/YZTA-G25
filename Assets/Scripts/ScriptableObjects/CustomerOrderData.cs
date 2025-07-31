using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerOrderData", menuName = "ScriptableObjects/CustomerOrderData")]
public class CustomerOrderData : ScriptableObject
{
    [Header("Customer Settings")]
    [Tooltip("List of recipes that customers can order")]
    public Recipe[] availableRecipes;
    
    [Header("Timing Settings")]
    [Tooltip("Minimum patience time in seconds")]
    public float minPatienceTime = 30f;
    
    [Tooltip("Maximum patience time in seconds")]
    public float maxPatienceTime = 90f;
    
    [Header("Spawning Settings")]
    [Tooltip("Minimum time between customer spawns in seconds")]
    public float minSpawnInterval = 5f;
    
    [Tooltip("Maximum time between customer spawns in seconds")]
    public float maxSpawnInterval = 15f;
    
    [Header("Scoring Settings")]
    [Tooltip("Tip percentage when above 50% time remaining")]
    public float highSpeedTipPercent = 20f;
    
    [Tooltip("Tip percentage when above 20% time remaining")]
    public float mediumSpeedTipPercent = 5f;
    
    /// <summary>
    /// Gets a random recipe from the available recipes list
    /// </summary>
    public Recipe GetRandomRecipe()
    {
        if (availableRecipes == null || availableRecipes.Length == 0)
        {
            Debug.LogError("No available recipes in CustomerOrderData!");
            return null;
        }
        
        return availableRecipes[Random.Range(0, availableRecipes.Length)];
    }
    
    /// <summary>
    /// Gets a random patience time within the specified range
    /// </summary>
    public float GetRandomPatienceTime()
    {
        return Random.Range(minPatienceTime, maxPatienceTime);
    }
    
    /// <summary>
    /// Gets a random spawn interval within the specified range
    /// </summary>
    public float GetRandomSpawnInterval()
    {
        return Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
