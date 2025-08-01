using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    // Bir lobiye ilk kez katýldýðýnda tetiklenir.
    public event Action<Lobby> OnJoinedLobby;
    // Lobi her güncellendiðinde (yeni oyuncu katýlmasý vb.) tetiklenir.
    public event Action<Lobby> OnLobbyUpdated;

    public Lobby JoinedLobby { get; private set; }
    private float pollTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        // Lobi durumunu düzenli olarak kontrol et.
        HandleLobbyPolling();
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            string relayJoinCode = await RelayManager.Instance.CreateRelay();
            if (string.IsNullOrEmpty(relayJoinCode)) return;

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCodeKey", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, options);
            JoinedLobby = lobby;

            // Host olarak lobi oluþturduktan sonra NetworkManager'ý baþlat.
            NetworkManager.Singleton.StartHost();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    public async Task<List<Lobby>> ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return response.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to list lobbies: " + e.Message);
            return null;
        }
    }

    public async Task JoinLobby(Lobby lobby)
    {
        try
        {
            JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string relayJoinCode = JoinedLobby.Data["JoinCodeKey"].Value;
            await RelayManager.Instance.JoinRelay(relayJoinCode);

            OnJoinedLobby?.Invoke(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to join lobby by ID: " + e.Message);
        }
    }

    public async Task JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            string relayJoinCode = JoinedLobby.Data["JoinCodeKey"].Value;
            await RelayManager.Instance.JoinRelay(relayJoinCode);

            OnJoinedLobby?.Invoke(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to join lobby by code: " + e.Message);
        }
    }

    private async void HandleLobbyPolling()
    {
        if (JoinedLobby == null) return;

        pollTimer -= Time.deltaTime;
        if (pollTimer < 0f)
        {
            float pollInterval = 1.1f;
            pollTimer = pollInterval;
            JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);

            // Lobi güncellendiðinde haber ver.
            OnLobbyUpdated?.Invoke(JoinedLobby);
        }
    }
}