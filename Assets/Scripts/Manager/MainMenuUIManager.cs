using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Core Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Lobby Panels")]
    [SerializeField] private GameObject createLobbyPanel;
    [SerializeField] private GameObject hostLobbyPanel;
    [SerializeField] private GameObject lobbyListPanel;
    [SerializeField] private GameObject clientLobbyPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Create Lobby UI")]
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private Button createLobbyConfirmButton;
    [SerializeField] private Button createLobbyBackButton;

    [Header("Host Lobby UI")]
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI hostPlayerListText;
    [SerializeField] private TextMeshProUGUI clientPlayerListText;

    [Header("Lobby List UI")]
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinWithCodeButton;
    [SerializeField] private Button refreshLobbyListButton;
    [SerializeField] private Button lobbyListBackButton;
    [SerializeField] private Transform lobbyListContent;
    [SerializeField] private GameObject lobbyItemPrefab;

    [Header("Settings UI")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private float sliderSoundInterval = 0.1f;
    private float sliderSoundTimer = 0f;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Image sfxVolumeIcon;
    [SerializeField] private Image musicVolumeIcon;
    [SerializeField] private Sprite soundOnIcon;    // Ses açýk ikonu (audioIcon.PNG)
    [SerializeField] private Sprite soundOffIcon;

    private List<Resolution> resolutions;
    private bool isSettingsInitialized = false;
    private bool isRefreshingLobbyList = false;
    private void Awake()
    {
        // Butonlarýn OnClick olaylarýný atýyoruz. Sesler UIButtonSound.cs tarafýndan yönetilecek.
        hostButton.onClick.AddListener(ShowCreateLobbyPanel);
        joinButton.onClick.AddListener(ShowLobbyListPanel);
        settingsButton.onClick.AddListener(ShowSettingsPanel);
        quitButton.onClick.AddListener(OnQuitClicked);

        // Ayarlar Menüsü
        settingsBackButton.onClick.AddListener(ShowMainMenuPanel);

        // Lobi Butonlarý
        createLobbyConfirmButton.onClick.AddListener(OnCreateLobbyConfirmed);
        createLobbyBackButton.onClick.AddListener(ShowMainMenuPanel);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        refreshLobbyListButton.onClick.AddListener(async () => await RefreshLobbyList());
        lobbyListBackButton.onClick.AddListener(ShowMainMenuPanel);
        joinWithCodeButton.onClick.AddListener(OnJoinWithCodeClicked);

    }

    private void Start()
    {
        ShowMainMenuPanel(); // Oyun baþladýðýnda sadece ana menü görünsün.

        // Að olaylarýný dinle
        //NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        //NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        LobbyManager.Instance.OnLobbyUpdated += UpdatePlayerListUIFromLobby;
        #region Settings Initialization
        // --- SES EFEKTÝ SLIDER'I KURULUMU ---
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // --- MÜZÝK SLIDER'I KURULUMU ---
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicVolumeSlider.value = savedMusicVolume;
        SetMusicVolume(savedMusicVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        // --- GRAFÝK VE ÇÖZÜNÜRLÜK AYARLARI ---
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(SetQuality);

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        resolutions = new List<Resolution>(Screen.resolutions);
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        LobbyManager.Instance.OnJoinedLobby += HandleJoinedLobby;
        isSettingsInitialized = true;

        

        
        #endregion
    }

    private void OnDestroy()
    {

        if (LobbyManager.Instance != null)
        {
            // Hem OnJoinedLobby hem de OnLobbyUpdated aboneliklerini iptal et.
            LobbyManager.Instance.OnJoinedLobby -= HandleJoinedLobby;
            LobbyManager.Instance.OnLobbyUpdated -= UpdatePlayerListUIFromLobby;
        }
    }

    private void Update()
    {
        if (sliderSoundTimer > 0)
        {
            sliderSoundTimer -= Time.deltaTime;
        }
    }

    #region Panel Management
    public void ShowMainMenuPanel() => SetActivePanel(mainMenuPanel);
    public void ShowSettingsPanel() => SetActivePanel(settingsPanel);
    public void ShowCreateLobbyPanel() => SetActivePanel(createLobbyPanel);
    public async void ShowLobbyListPanel()
    {
        SetActivePanel(lobbyListPanel);
        await RefreshLobbyList();
    }
    private void SetActivePanel(GameObject targetPanel)
    {
        mainMenuPanel.SetActive(targetPanel == mainMenuPanel);
        settingsPanel.SetActive(targetPanel == settingsPanel);
        createLobbyPanel.SetActive(targetPanel == createLobbyPanel);
        hostLobbyPanel.SetActive(targetPanel == hostLobbyPanel);
        lobbyListPanel.SetActive(targetPanel == lobbyListPanel);
        clientLobbyPanel.SetActive(targetPanel == clientLobbyPanel);
    }
    #endregion

    #region Lobby Logic
    private async void OnCreateLobbyConfirmed()
    {
        string lobbyName = lobbyNameInputField.text;
        bool isPrivate = isPrivateToggle.isOn;
        if (string.IsNullOrEmpty(lobbyName)) lobbyName = "My Friendslop Lobby";

        // LobbyManager'a lobi oluþturmasýný söylüyoruz.
        await LobbyManager.Instance.CreateLobby(lobbyName, isPrivate);

        // Eðer lobi baþarýyla oluþturulduysa...
        if (LobbyManager.Instance.JoinedLobby != null)
        {
            SetActivePanel(hostLobbyPanel);

            // --- DEÐÝÞÝKLÝK BURADA ---
            // Ekrana Relay Kodu yerine, Lobi Servisi'nin verdiði LOBÝ KODU'nu yazdýr.
            joinCodeText.text = "Katýlým Kodu: " + LobbyManager.Instance.JoinedLobby.LobbyCode;

            startGameButton.interactable = false;
        }
    }

    public async Task RefreshLobbyList()
    {
        // Eðer zaten bir yenileme iþlemi devam ediyorsa, yenisini baþlatma.
        if (isRefreshingLobbyList) return;

        isRefreshingLobbyList = true; // Yenileme baþladý olarak iþaretle

        foreach (Transform child in lobbyListContent) Destroy(child.gameObject);

        List<Lobby> lobbies = await LobbyManager.Instance.ListLobbies();
        if (lobbies != null)
        {
            foreach (Lobby lobby in lobbies)
            {
                GameObject lobbyItemGO = Instantiate(lobbyItemPrefab, lobbyListContent);
                lobbyItemGO.GetComponent<LobbyItemUI>().Setup(lobby);
            }
        }

        // Birkaç saniye sonra tekrar yenileme yapýlmasýna izin ver.
        // Bu satýrý eklemek zorunda deðilsiniz ama iyi bir pratiktir.
        await Task.Delay(2000); // 2 saniye bekle
        isRefreshingLobbyList = false; // Yenileme bitti olarak iþaretle
    }

    private void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public async void OnJoinWithCodeClicked()
    {
        string joinCode = joinCodeInputField.text;
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("Lütfen bir katýlým kodu girin.");
            return;
        }

        // Doðrudan LobbyManager'daki yeni metodu çaðýrýyoruz.
        // Panel deðiþtirme iþini burasý yapmýyor, event dinleyicisi yapacak.
        await LobbyManager.Instance.JoinLobbyByCode(joinCode);
    }

    private void HandleClientConnected(ulong clientId)
    {
        
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.interactable = NetworkManager.Singleton.ConnectedClients.Count == 2;
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.interactable = false;
        }
    }
    #endregion

    #region Settings Logic
    public void SetVolume(float volume)
    {
        if (isSettingsInitialized) { if (sliderSoundTimer <= 0f) { SoundManager.PlaySound(SoundType.SLIDER_TICK); sliderSoundTimer = sliderSoundInterval; } }
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);

        if (volume <= 0.0001f)
        {
            sfxVolumeIcon.sprite = soundOffIcon;
        }
        else
        {
            sfxVolumeIcon.sprite = soundOnIcon;
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (isSettingsInitialized) { if (sliderSoundTimer <= 0f) { SoundManager.PlaySound(SoundType.SLIDER_TICK); sliderSoundTimer = sliderSoundInterval; } }
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("MusicVolume", dbVolume);
        PlayerPrefs.SetFloat("MusicVolume", volume);

        if (volume <= 0.0001f)
        {
            musicVolumeIcon.sprite = soundOffIcon;
        }
        else
        {
            musicVolumeIcon.sprite = soundOnIcon;
        }
    }

    public void SetQuality(int qualityIndex) => QualitySettings.SetQualityLevel(qualityIndex);
    public void SetFullscreen(bool isFullscreen) => Screen.fullScreen = isFullscreen;
    public void SetResolution(int resolutionIndex) { Resolution res = resolutions[resolutionIndex]; Screen.SetResolution(res.width, res.height, Screen.fullScreen); }
    #endregion

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public async void JoinLobbyFromList(Lobby lobby)
    {
        // LobbyItemUI bu metodu çaðýrabilir.
        // Panel deðiþtirme iþini burasý yapmýyor, event dinleyicisi yapacak.
        await LobbyManager.Instance.JoinLobby(lobby);
    }


    private void UpdatePlayerListUIFromLobby(Lobby lobby)
    {
        if (lobby == null) return;

        // --- DEÐÝÞÝKLÝK BURADA ---
        // Artýk oyuncu isimlerini listelemek yerine,
        // mevcut oyuncu sayýsýný ve maksimum oyuncu sayýsýný alýyoruz.
        int playerCount = lobby.Players.Count;
        int maxPlayers = lobby.MaxPlayers;

        // Ekranda "Oyuncular: 1/2" gibi görünecek metni oluþturuyoruz.
        string playerCountText = $"Oyuncular: {playerCount} / {maxPlayers}";

        // Hem Host'un hem de Client'ýn listesini bu yeni metinle güncelle
        // (Ayný Text objesini kullandýðýmýzý varsayýyoruz)
        if (hostPlayerListText != null)
            hostPlayerListText.text = playerCountText;

        if (clientPlayerListText != null)
            clientPlayerListText.text = playerCountText;

        // "Start Game" butonunu, Host ise ve lobide 2 kiþi varsa aktif et.
        if (NetworkManager.Singleton.IsHost && startGameButton != null)
        {
            startGameButton.interactable = playerCount == maxPlayers;
        }
    }



    private void HandleJoinedLobby(Lobby joinedLobby)
    {
        Debug.Log("--- MainMenuUIManager: OnJoinedLobby event'i baþarýyla alýndý!");

        if (clientLobbyPanel == null)
        {
            Debug.LogError("--- MainMenuUIManager HATA: ClientLobbyPanel Inspector'da atanmamýþ! Panel gösterilemiyor.");
            return;
        }

        Debug.Log("--- MainMenuUIManager: ClientLobbyPanel aktif ediliyor.");
        SetActivePanel(clientLobbyPanel);
    }


}