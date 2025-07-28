using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListItemPrefab;

    private Lobby _connectedLobby;
    private float _lobbyRefreshTimer;

    private async void Start()
    {
        createLobbyButton.interactable = false;
        joinLobbyButton.interactable = false;

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Unity Servisleri baþlatýldý. Oyuncu ID: " + AuthenticationService.Instance.PlayerId);

        createLobbyButton.interactable = true;
        joinLobbyButton.interactable = true;

        createLobbyButton.onClick.AddListener(CreateLobby);
        joinLobbyButton.onClick.AddListener(JoinLobbyWithCode);
        startGameButton.onClick.AddListener(StartGame);
    }

    private void Update()
    {
        if (_connectedLobby != null)
        {
            _lobbyRefreshTimer -= Time.deltaTime;
            if (_lobbyRefreshTimer <= 0f)
            {
                _lobbyRefreshTimer = 1.5f; // Yenileme süresini biraz artýrdýk
                RefreshLobby();
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string playerName = string.IsNullOrEmpty(playerNameInput.text) ? "Oyuncu (Host)" : playerNameInput.text;
            Debug.Log("Lobi oluþturuluyor... Oyuncu Adý: " + playerName);

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = new Player { Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) } } },
                Data = new Dictionary<string, DataObject> { { "RELAY_JOIN_CODE", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) } }
            };

            _connectedLobby = await LobbyService.Instance.CreateLobbyAsync("Yeni Lobi", 2, options);
            lobbyCodeText.text = "Lobi Kodu: " + _connectedLobby.LobbyCode;
            Debug.Log("Lobi baþarýyla oluþturuldu! Lobi Kodu: " + _connectedLobby.LobbyCode + ", Relay Kodu: " + relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartHost();
            startGameButton.gameObject.SetActive(true);
        }
        catch (LobbyServiceException e) { Debug.LogError("Lobi oluþturulamadý: " + e); }
    }

    private async void JoinLobbyWithCode()
    {
        try
        {
            string playerName = string.IsNullOrEmpty(playerNameInput.text) ? "Oyuncu (Client)" : playerNameInput.text;
            string joinCode = lobbyCodeInput.text;
            Debug.Log("Lobiye katýlýnýyor... Lobi Kodu: " + joinCode + ", Oyuncu Adý: " + playerName);

            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = new Player { Data = new Dictionary<string, PlayerDataObject> { { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) } } }
            };

            _connectedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, options);
            Debug.Log("Lobiye baþarýyla katýldý: " + _connectedLobby.Name);

            string relayJoinCode = _connectedLobby.Data["RELAY_JOIN_CODE"].Value;
            Debug.Log("Relay kodu alýndý: " + relayJoinCode);

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            Debug.Log("Relay sunucusuna baþarýyla katýldý.");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e) { Debug.LogError("Lobiye katýlamadý: " + e); }
    }

    private void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            LobbyService.Instance.UpdateLobbyAsync(_connectedLobby.Id, new UpdateLobbyOptions { IsLocked = true });
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private async void RefreshLobby()
    {
        if (_connectedLobby != null)
        {
            _connectedLobby = await LobbyService.Instance.GetLobbyAsync(_connectedLobby.Id);
            RefreshPlayerListUI();
        }
    }

    private void RefreshPlayerListUI()
    {
        foreach (Transform child in playerListContent) { Destroy(child.gameObject); }

        foreach (Player player in _connectedLobby.Players)
        {
            GameObject playerItemGO = Instantiate(playerListItemPrefab);
            playerItemGO.transform.SetParent(playerListContent, false);
            playerItemGO.GetComponent<PlayerListItemUI>().SetPlayer(player);
        }
    }
}