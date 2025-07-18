using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        scoreText.text = "Skor: 0";
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged += UpdateScoreText;
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
        scoreText.text = "Skor: " + newScore;
    }
}