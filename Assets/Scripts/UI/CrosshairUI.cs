using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Elements")]
    [SerializeField] private Image crosshairDot;
    [SerializeField] private Image crosshairRing;
    // Göz bebeği büyümüş versiyon grabbedCrosshairRing olsun? 
    [SerializeField] private TextMeshProUGUI interactionText;
    
    [Header("Crosshair Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactableColor = Color.green;
    [SerializeField] private Color grabbedColor = Color.red;
    
    private CrosshairState currentState = CrosshairState.Normal;

    // Singleton pattern for easy access
    public static CrosshairUI Instance { get; private set; }
    
    public enum CrosshairState
    {
        Normal,        // Default state
        Interactable,  // When looking at a lever
        Grabbed        // When controlling a lever
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SetCrosshairState(CrosshairState.Normal);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCrosshairState(CrosshairState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case CrosshairState.Normal:
                SetNormalState();
                break;
            case CrosshairState.Interactable:
                SetInteractableState();
                break;
            case CrosshairState.Grabbed:
                SetGrabbedState();
                break;
        }
    }
    
    private void SetNormalState()
    {
        SetCrosshairColor(normalColor);
        SetInteractionText("");
        
        crosshairDot.gameObject.SetActive(true);
        crosshairRing.gameObject.SetActive(false);
    }

    private void SetInteractableState()
    {
        SetCrosshairColor(interactableColor);
        SetInteractionText("Left Click to Grab Lever");

        crosshairDot.gameObject.SetActive(false);
        crosshairRing.gameObject.SetActive(true);
    }
    
    private void SetGrabbedState()
    {
        SetCrosshairColor(grabbedColor);
        SetInteractionText("Move Mouse to Control Lever • Left Click to Release");
    }
    
    private void SetCrosshairColor(Color color)
    {
        if (crosshairDot != null)
        {
            crosshairDot.color = color;
        }
        
        if (crosshairRing != null)
        {
            crosshairRing.color = color;
        }
    }
    
    private void SetInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text;
            interactionText.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }
    }
    
    // Public methods for EyeInteractor to call
    public void ShowInteractable()
    {
        SetCrosshairState(CrosshairState.Interactable);
    }
    
    public void ShowGrabbed()
    {
        SetCrosshairState(CrosshairState.Grabbed);
    }
    
    public void ShowNormal()
    {
        SetCrosshairState(CrosshairState.Normal);
    }
}
