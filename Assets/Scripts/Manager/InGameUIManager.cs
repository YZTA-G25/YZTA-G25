using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject disconnectPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton; // Settings panelindeki geri butonu
    [SerializeField] private Button mainMenuButton;

    [Header("Settings UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private List<Resolution> resolutions;

    [Header("Disconnect UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private bool isPaused = false;

    // Oyuncu kontrolünü durdurmak için kullanýlacak static event
    public static event Action<bool> OnGamePaused;

    private void Start()
    {
        // Butonlara týklandýðýnda ne olacaðýný belirle
        // Her týklamada ÖNCE sesi çal, SONRA ilgili fonksiyonu çalýþtýr.

        resumeButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            ResumeGame();
        });

        mainMenuButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnMainMenuClicked();
        });

        settingsButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            ShowSettingsPanel();
        });

        backButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            HideSettingsPanel();
        });

        // Slider'ýn deðeri deðiþtiðinde SetVolume metodunu çaðýr
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Baþlangýçta tüm panellerin kapalý olduðundan emin ol
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        disconnectPanel.SetActive(false);

        // Kayýtlý ses ayarýný yükle ve uygula
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(SetQuality);

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        // --- Çözünürlük Ayarlarý Baþlangýcý ---
        resolutions = new List<Resolution>(Screen.resolutions);
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        // Ýmleci yönet
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        // Eðer menü kapanýyorsa ve ayarlar paneli açýksa, onu da kapat
        if (!isPaused)
        {
            settingsPanel.SetActive(false);
        }

        // Tüm dinleyicilere oyunun duraklatýlma durumunu (true/false) gönder
        OnGamePaused?.Invoke(isPaused);
    }

    public void ResumeGame()
    {
        // Eðer zaten oyun devam ediyorsa bir þey yapma
        if (!isPaused) return;

        // TogglePauseMenu'yü çaðýrmak, tüm mantýðý tek bir yerde tutar.
        TogglePauseMenu();
    }

    private void OnMainMenuClicked()
    {
        // Að oturumunu güvenli bir þekilde kapat
        NetworkManager.Singleton.Shutdown();

        // Kalýcý objeleri temizle
        if (SoundManager.Instance != null) Destroy(SoundManager.Instance.gameObject);
        if (ScoringManager.Instance != null) Destroy(ScoringManager.Instance.gameObject);

        // Ana menü sahnesine dön
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowSettingsPanel()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("MasterVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void ShowDisconnectPanel(float duration)
    {
        disconnectPanel.SetActive(true);
        // Ýsteðe baðlý: Geri sayým metnini burada güncelleyebilirsiniz.
        if (countdownText != null)
            countdownText.text = $"Diðer oyuncunun baðlantýsý koptu. Ana menüye dönmek için bekleniyor: {duration}";
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}