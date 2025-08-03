using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

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

    [Header("Audio")]
    [SerializeField] private float sliderSoundInterval = 0.1f; // Sesler aras� minimum saniye
    [SerializeField] private Slider musicVolumeSlider;
    private float sliderSoundTimer = 0f;

    [Header("Settings UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private List<Resolution> resolutions;
    private bool isSettingsInitialized = false;

    [Header("Disconnect UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private bool isPaused = false;

    // Oyuncu kontrol�n� durdurmak i�in kullan�lacak static event
    public static event Action<bool> OnGamePaused;
    

    private void Start()
    {

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(MusicTrack.GameScene);
        }

        // Butonlara t�kland���nda ne olaca��n� belirle
        // Her t�klamada �NCE sesi �al, SONRA ilgili fonksiyonu �al��t�r.

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        // Kay�tl� m�zik sesini y�kle
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicVolumeSlider.value = savedMusicVolume;
        SetMusicVolume(savedMusicVolume);

        resumeButton.onClick.AddListener(() => {
            
            ResumeGame();
        });

        mainMenuButton.onClick.AddListener(() => {
            
            OnMainMenuClicked();
        });

        settingsButton.onClick.AddListener(() => {
            
            ShowSettingsPanel();
        });

        backButton.onClick.AddListener(() => {
            
            HideSettingsPanel();
        });

        // Slider'�n de�eri de�i�ti�inde SetVolume metodunu �a��r
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Ba�lang��ta t�m panellerin kapal� oldu�undan emin ol
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        disconnectPanel.SetActive(false);

        // Kay�tl� ses ayar�n� y�kle ve uygula
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

        // --- ��z�n�rl�k Ayarlar� Ba�lang�c� ---
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

        isSettingsInitialized = true;
    }

    private void Update()
    {
        if (sliderSoundTimer > 0)
        {
            sliderSoundTimer -= Time.deltaTime;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

    }

    public void TogglePauseMenu()
    {
        SoundManager.PlaySound(SoundType.MENU_PAUSE,0.5f);


        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        // �mleci y�net
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;

        // E�er men� kapan�yorsa ve ayarlar paneli a��ksa, onu da kapat
        if (!isPaused)
        {
            settingsPanel.SetActive(false);
        }

        // T�m dinleyicilere oyunun duraklat�lma durumunu (true/false) g�nder
        OnGamePaused?.Invoke(isPaused);
    }

    public void ResumeGame()
    {
        // E�er zaten oyun devam ediyorsa bir �ey yapma
        if (!isPaused) return;

        // TogglePauseMenu'y� �a��rmak, t�m mant��� tek bir yerde tutar.
        TogglePauseMenu();
    }

    private void OnMainMenuClicked()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(MusicTrack.MainMenu);
        }
        // A� oturumunu g�venli bir �ekilde kapat
        NetworkManager.Singleton.Shutdown();

        // Kal�c� objeleri temizle
        if (SoundManager.Instance != null) Destroy(SoundManager.Instance.gameObject);
        if (ScoringManager.Instance != null) Destroy(ScoringManager.Instance.gameObject);

        // Ana men� sahnesine d�n
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
        // �nce zamanlay�c�y� kontrol edip sesi �al�yor
        if (isSettingsInitialized)
        {
            if (sliderSoundTimer <= 0f)
            {
                SoundManager.PlaySound(SoundType.SLIDER_TICK, 0.1f);
                sliderSoundTimer = sliderSoundInterval;
            }
        }

        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void ShowDisconnectPanel(float duration)
    {
        disconnectPanel.SetActive(true);
        // �ste�e ba�l�: Geri say�m metnini burada g�ncelleyebilirsiniz.
        if (countdownText != null)
            countdownText.text = $"Di�er oyuncunun ba�lant�s� koptu. Ana men�ye d�nmek i�in bekleniyor: {duration}";
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

    public void SetMusicVolume(float volume)
    {
        // Ses �alma mant���
        if (isSettingsInitialized)
        {
            if (sliderSoundTimer <= 0f)
            {
                SoundManager.PlaySound(SoundType.SLIDER_TICK, 0.1f);
                sliderSoundTimer = sliderSoundInterval;
            }
        }

        // S�f�r de�eri i�in matematiksel d�zeltme
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        // Mixer'a do�ru parametre ad�yla do�ru de�eri g�nderme
        mainMixer.SetFloat("MusicVolume", dbVolume);

        // Ayar� kaydetme
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }


}