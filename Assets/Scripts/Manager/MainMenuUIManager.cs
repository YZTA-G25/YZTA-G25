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
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

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

        isSettingsInitialized = true;
        #endregion
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
    }
    #endregion

    #region Lobby Logic
    private async void OnCreateLobbyConfirmed()
    {
        string lobbyName = lobbyNameInputField.text;
        bool isPrivate = isPrivateToggle.isOn;
        if (string.IsNullOrEmpty(lobbyName)) lobbyName = "My Friendslop Lobby";

        await LobbyManager.Instance.CreateLobby(lobbyName, isPrivate);

        if (LobbyManager.Instance.JoinedLobby != null)
        {
            SetActivePanel(hostLobbyPanel);
            joinCodeText.text = "Katýlým Kodu: " + LobbyManager.Instance.JoinedLobby.Data["JoinCodeKey"].Value;
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

        // LobbyManager üzerinden deðil, doðrudan RelayManager ile katýlmayý dene
        await RelayManager.Instance.JoinRelay(joinCode);
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        startGameButton.interactable = NetworkManager.Singleton.ConnectedClients.Count == 2;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        startGameButton.interactable = false;
    }
    #endregion

    #region Settings Logic
    public void SetVolume(float volume)
    {
        if (isSettingsInitialized) { if (sliderSoundTimer <= 0f) { SoundManager.PlaySound(SoundType.SLIDER_TICK); sliderSoundTimer = sliderSoundInterval; } }
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (isSettingsInitialized) { if (sliderSoundTimer <= 0f) { SoundManager.PlaySound(SoundType.SLIDER_TICK); sliderSoundTimer = sliderSoundInterval; } }
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("MusicVolume", dbVolume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
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
}