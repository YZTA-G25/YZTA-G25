using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public enum PanelState
{
    PreOrder,
    ActiveOrder
}

public class CustomerOrderPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image recipeImage;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform ingredientListContainer; // You'll create this empty object with layout
    [SerializeField] private Image timerBar;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Timer Colors")]
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color redColor = Color.red;
    
    [Header("Animation Settings")]
    [SerializeField] private float expandDuration = 0.5f;
    [SerializeField] private float ingredientShowDelay = 0.1f;
    [SerializeField] private float panelExpandAmount = 60f; // How much to expand per ingredient
    
    private Customer customer;
    private ulong customerId; // Store customer's NetworkObjectId for reliable identification
    private Vector3 targetPosition;
    private Vector3 currentVelocity; // For smooth damping
    private float panelWidth = 200f;
    private PanelState currentState = PanelState.PreOrder;
    private bool isAnimatingExpansion = false; // Track if expansion animation is running
    private List<GameObject> ingredientItems = new List<GameObject>();
    private RectTransform panelRect;
    private Vector2 originalPanelSize;
    private Vector3 originalTimerPosition;
    
    public Customer Customer => customer;
    public ulong CustomerId => customerId;
    
    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
        {
            originalPanelSize = panelRect.sizeDelta;
        }
        
        if (timerBar != null && timerBar.transform.parent != null)
        {
            originalTimerPosition = timerBar.transform.parent.localPosition;
        }
    }
    
    public void SetupPanel(Customer customer)
    {
        this.customer = customer;
        this.customerId = customer.GetComponent<NetworkObject>().NetworkObjectId;
        
        if (customer.CurrentOrder != null)
        {
            // Set recipe image
            if (recipeImage != null && customer.CurrentOrder.recipeImage != null)
            {
                recipeImage.sprite = customer.CurrentOrder.recipeImage;
            }
            
            // Set recipe name
            if (recipeNameText != null)
            {
                recipeNameText.text = customer.CurrentOrder.recipeName;
                
                // Configure text to fit without wrapping
                recipeNameText.textWrappingMode = TextWrappingModes.NoWrap;
                recipeNameText.overflowMode = TextOverflowModes.Overflow;
                recipeNameText.alignment = TextAlignmentOptions.Left;
                
                // Enable auto-sizing to fit text within bounds if needed
                recipeNameText.enableAutoSizing = true;
                recipeNameText.fontSizeMin = 25f;
                recipeNameText.fontSizeMax = 35f; // Keep current size as max
            }
        }
        
        // Start in PreOrder state
        SetPanelState(PanelState.PreOrder);
    }
    
    public void TransitionToActiveOrder()
    {
        if (currentState == PanelState.PreOrder)
        {
            StartCoroutine(TransitionToActiveOrderCoroutine());
        }
    }
    
    private void SetPanelState(PanelState newState)
    {
        currentState = newState;
        
        if (newState == PanelState.PreOrder)
        {
            // Hide ingredient list
            if (ingredientListContainer != null)
            {
                ingredientListContainer.gameObject.SetActive(false);
            }
            
            // Reset panel size
            if (panelRect != null)
            {
                panelRect.sizeDelta = originalPanelSize;
            }
            
            // Reset timer position
            if (timerBar != null && timerBar.transform.parent != null)
            {
                timerBar.transform.parent.localPosition = originalTimerPosition;
            }
        }
    }
    
    private IEnumerator TransitionToActiveOrderCoroutine()
    {
        // Ingredients are guaranteed to exist when customer is created, no need to check for null
        var requiredIngredients = customer.CurrentOrder.requiredIngredients;
        
        currentState = PanelState.ActiveOrder;
        isAnimatingExpansion = true; // Prevent position updates during animation
        
        // Show ingredient container
        if (ingredientListContainer != null)
        {
            ingredientListContainer.gameObject.SetActive(true);
        }
        
        // Clear existing ingredient items
        ClearIngredientItems();
        
        // Calculate total expansion needed
        int ingredientCount = requiredIngredients.Count;
        float totalExpansion = ingredientCount * panelExpandAmount;
        
        // Create and show ingredients one by one
        for (int i = 0; i < requiredIngredients.Count; i++)
        {
            CreateIngredientItem(requiredIngredients[i]);
            
            // Animate panel expansion for this ingredient
            float ingredientElapsedTime = 0f;
            Vector2 ingredientStartSize = panelRect.sizeDelta;
            Vector2 ingredientTargetSize = ingredientStartSize + new Vector2(0, panelExpandAmount);
            Vector3 ingredientStartTimerPos = timerBar.transform.parent.localPosition;
            Vector3 ingredientTargetTimerPos = ingredientStartTimerPos + new Vector3(0, -panelExpandAmount, 0);
            
            while (ingredientElapsedTime < expandDuration)
            {
                ingredientElapsedTime += Time.deltaTime;
                float t = ingredientElapsedTime / expandDuration;
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth curve
                
                // Animate panel size
                if (panelRect != null)
                {
                    panelRect.sizeDelta = Vector2.Lerp(ingredientStartSize, ingredientTargetSize, t);
                }
                
                // Animate timer position
                if (timerBar != null && timerBar.transform.parent != null)
                {
                    timerBar.transform.parent.localPosition = Vector3.Lerp(ingredientStartTimerPos, ingredientTargetTimerPos, t);
                }
                
                yield return null;
            }
            
            // Ensure final positions are exact
            if (panelRect != null)
            {
                panelRect.sizeDelta = ingredientTargetSize;
            }
            if (timerBar != null && timerBar.transform.parent != null)
            {
                timerBar.transform.parent.localPosition = ingredientTargetTimerPos;
            }
            
            // Wait before showing next ingredient
            yield return new WaitForSeconds(ingredientShowDelay);
        }
        
        // Animation complete, allow position updates again
        isAnimatingExpansion = false;
    }
    
    private void CreateIngredientItem(Ingredient ingredient)
    {
        if (ingredientListContainer == null || ingredient == null) return;
        
        // Create ingredient item GameObject
        GameObject ingredientItem = new GameObject($"Ingredient_{ingredient.Name}");
        ingredientItem.transform.SetParent(ingredientListContainer, false);
        
        // Add RectTransform
        RectTransform itemRect = ingredientItem.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 30); // Height per ingredient item - increased from 25 to 30
        
        // Create horizontal layout for icon + text
        HorizontalLayoutGroup layout = ingredientItem.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.spacing = 5f;
        layout.padding = new RectOffset(5, 5, 2, 2);
        layout.childAlignment = TextAnchor.MiddleLeft;
        
        // Create icon if available
        if (ingredient.icon != null && ingredient.icon.sprite != null)
        {
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(ingredientItem.transform, false);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.sprite = ingredient.icon.sprite;
            iconImage.preserveAspect = true;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(24, 24); // Increased from 20x20 to 24x24
        }
        
        // Create text for ingredient name
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(ingredientItem.transform, false);
        
        TextMeshProUGUI ingredientText = textObj.AddComponent<TextMeshProUGUI>();
        ingredientText.text = ingredient.Name;
        ingredientText.fontSize = 26f;
        ingredientText.color = Color.white;
        ingredientText.alignment = TextAlignmentOptions.Left;
        ingredientText.textWrappingMode = TextWrappingModes.NoWrap;
        ingredientText.overflowMode = TextOverflowModes.Overflow;
        
        // Store reference for cleanup
        ingredientItems.Add(ingredientItem);
    }
    
    private void ClearIngredientItems()
    {
        foreach (GameObject item in ingredientItems)
        {
            if (item != null)
            {
                DestroyImmediate(item);
            }
        }
        ingredientItems.Clear();
    }
    
    public void SetTargetPosition(int index)
    {
        targetPosition = new Vector3(index * panelWidth, 0, 0);
    }
    
    public void UpdatePosition(float slideSpeed)
    {
        // Don't update position during expansion animation to avoid interference
        if (isAnimatingExpansion) return;
        
        // Use SmoothDamp for consistent, smooth movement without slowdown
        float smoothTime = 1f / slideSpeed; // Convert speed to smooth time
        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );
    }
    
    public void UpdateTimerDisplay()
    {
        if (customer == null) return;
        
        float timePercentage = customer.PatiencePercentage;
        
        // Update timer bar fill
        if (timerBar != null)
        {
            timerBar.fillAmount = timePercentage;
            
            // Update color based on time remaining
            if (timePercentage > 0.5f)
            {
                timerBar.color = greenColor;
            }
            else if (timePercentage > 0.2f)
            {
                timerBar.color = yellowColor;
            }
            else
            {
                timerBar.color = redColor;
            }
        }
        
        // Update timer text
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(customer.PatienceTime);
            timerText.text = $"{seconds}s";
        }
    }
}
