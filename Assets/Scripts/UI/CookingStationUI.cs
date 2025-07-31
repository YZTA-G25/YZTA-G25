using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingStationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button cookButton;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject uiPanel;
    
    [Header("Cooking Station")]
    [SerializeField] private CookingStation cookingStation;
    
    private bool isNearStation = false;
    
    private void Start()
    {
        if (cookButton != null)
        {
            cookButton.onClick.AddListener(OnCookButtonPressed);
        }
        
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Check for manual input when near station
        if (isNearStation && Input.GetKeyDown(KeyCode.E))
        {
            OnCookButtonPressed();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a player entering the cooking area
        if (other.CompareTag("Player")) // You might need to adjust this tag check
        {
            isNearStation = true;
            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }
            
            if (instructionText != null)
            {
                instructionText.text = "Press E or Click Cook to prepare meal";
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearStation = false;
            if (uiPanel != null)
            {
                uiPanel.SetActive(false);
            }
        }
    }
    
    private void OnCookButtonPressed()
    {
        if (cookingStation != null)
        {
            cookingStation.ValidateAndCook();
        }
    }
}
