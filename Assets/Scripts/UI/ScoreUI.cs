using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image scoreBarFill;
    
    [Header("Score Settings")]
    [SerializeField] private int maxPoints = 100;

    private void Start()
    {
        UpdateScoreDisplay(0);
        SubscribeToScoringManager();
    }

    private void SubscribeToScoringManager()
    {
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged += UpdateScoreText;
        }
        else
        {
            // If ScoringManager isn't ready yet, try again next frame
            Invoke(nameof(SubscribeToScoringManager), 0.1f);
        }
    }

    private void OnDestroy()
    {
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged -= UpdateScoreText;
        }
    }

    private void UpdateScoreText(int newScore)
    {
        UpdateScoreDisplay(newScore);
    }

    private void UpdateScoreDisplay(int score)
    {
        // Update text
        if (scoreText != null)
        {
            scoreText.text = $"Skor: {score}/{maxPoints}";
        }

        // Update fill bar
        if (scoreBarFill != null)
        {
            float fillAmount = Mathf.Clamp01((float)score / maxPoints);
            scoreBarFill.fillAmount = fillAmount;
        }
    }
}