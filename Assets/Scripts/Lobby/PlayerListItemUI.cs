using TMPro;
using UnityEngine;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void SetPlayer(Unity.Services.Lobbies.Models.Player player)
    {
        // Oyuncunun verisinden "PlayerName" anahtarýný ara
        if (player.Data != null && player.Data.TryGetValue("PlayerName", out var playerDataObject))
        {
            // Eðer bulursa, metin olarak yaz
            playerNameText.text = playerDataObject.Value;
        }
        else
        {
            // Eðer isim verisi bulunamazsa, oyuncunun ID'sini yaz (hata ayýklama için)
            playerNameText.text = "Oyuncu " + player.Id;
        }
    }
}