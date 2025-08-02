using UnityEngine;
using UnityEngine.UI; // Image kontrolü için bu satýr gerekli
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    [SerializeField] private TextMeshProUGUI scoreText; // Ýsteðe baðlý, skor metni
    [SerializeField] private Image scoreBarImage;   // Inspector'dan atanacak 'Fill' görselin
    [SerializeField] private RectTransform scoreBarContainer; // <-- YENÝ: Barýn tamamýný içeren RectTransform
    [SerializeField] private RectTransform scoreBarHeartIcon;

    [Header("Bar Ayarlarý")]
    [Tooltip("Barýn %100 dolmasý için gereken toplam skor")]
    [SerializeField] private int maxScore = 100; // Arkadaþýnýn önerdiði gibi 100 ile baþlayabiliriz

    private void Start()
    {
        // Baþlangýçta UI'ý sýfýrla
        UpdateScoreUI(0);

        // ScoringManager'dan gelen skor deðiþikliklerini dinlemeye baþla
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged += UpdateScoreUI;
        }
    }

    private void OnDestroy()
    {
        // Sahne kapanýrken veya obje yok olurken event aboneliðini iptal et (önemli!)
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }
    }

    // Skor deðiþtiðinde çaðrýlacak ana fonksiyon
    private void UpdateScoreUI(int newScore)
    {
        // 1. Metni Güncelle
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + newScore;
        }

        // 2. Görsel Barý Güncelle (Arkadaþýnýn Söylediði Mantýk)
        if (scoreBarImage != null)
        {
            float fillAmount = (float)newScore / (float)maxScore;
            scoreBarImage.fillAmount = Mathf.Clamp01(fillAmount);

            // --- DÝKEY BARA UYGUN KALP POZÝSYON KODU ---
            if (scoreBarHeartIcon != null && scoreBarContainer != null)
            {
                // Barýn toplam YÜKSEKLÝÐÝNÝ al (geniþliðini deðil)
                float barHeight = scoreBarContainer.rect.height; // DEÐÝÞÝKLÝK: .width yerine .height

                // Kalbin yeni Y pozisyonunu hesapla (yükseklik * doluluk oraný)
                float heartYPosition = barHeight * scoreBarImage.fillAmount;

                // Kalbin pozisyonunu güncelle (sadece Y ekseninde)
                // anchoredPosition.x'i koruyarak sadece Y'yi deðiþtiriyoruz
                scoreBarHeartIcon.anchoredPosition = new Vector2(scoreBarHeartIcon.anchoredPosition.x, heartYPosition); // DEÐÝÞÝKLÝK: Vector2'nin yapýsý deðiþti
            }
        }
    }
}