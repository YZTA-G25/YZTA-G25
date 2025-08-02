using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;

    private Lobby lobby;

    public void Setup(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private async void OnJoinClicked()
    {
        joinButton.interactable = false; // Tekrar basýlmasýný engelle
        await LobbyManager.Instance.JoinLobby(this.lobby);
    }
}