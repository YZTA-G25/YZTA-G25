using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private float sliderSoundInterval = 0.1f; // Sesler arasý minimum saniye
    private float sliderSoundTimer = 0f;


    [Header("Settings UI")] // Mevcut baþlýðýn altýna veya yeni bir baþlýða
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // Çözünürlükleri saklamak için bir liste
    private List<Resolution> resolutions;
    private bool isSettingsInitialized = false;

    private void Awake()
    {
        // Tüm butonlarýn týklama olaylarýna ilgili fonksiyonlarý atýyoruz.
        // Her týklama önce sesi çalar, sonra ana fonksiyonu çalýþtýrýr.

        hostButton.onClick.AddListener(() => {
            
            OnHostClicked();
        });

        joinButton.onClick.AddListener(() => {
            
            OnJoinClicked();
        });

        settingsButton.onClick.AddListener(() => {
            
            OnSettingsClicked();
        });

        quitButton.onClick.AddListener(() => {
            
            OnQuitClicked();
        });

        backButton.onClick.AddListener(() => {
            
            OnBackClicked();
        });

        // Slider'ýn deðer deðiþtirme olayýna ses fonksiyonunu atýyoruz.
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void Start()
    {
        // --- SES EFEKTÝ SLIDER'I KURULUMU ---
        // 1. Önce deðeri yükle ve ayarla.
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);
        // 2. SONRA dinlemeye baþla.
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // --- MÜZÝK SLIDER'I KURULUMU ---
        // 1. Önce deðeri yükle ve ayarla.
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicVolumeSlider.value = savedMusicVolume;
        SetMusicVolume(savedMusicVolume);
        // 2. SONRA dinlemeye baþla.
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        // --- DÝÐER AYARLAR ---
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
    }

    private void Update()
    {
        // Zamanlayýcýyý her karede azalt
        if (sliderSoundTimer > 0)
        {
            sliderSoundTimer -= Time.deltaTime;
        }

        // InGameUIManager için:
        // if (Input.GetKeyDown(KeyCode.Escape)) { ... } kýsmý burada kalmaya devam edecek.
    }

    private void OnSettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void SetVolume(float volume)
    {

        if (isSettingsInitialized)
        {
            if (sliderSoundTimer <= 0f)
            {
                SoundManager.PlaySound(SoundType.SLIDER_TICK,0.1f);
                sliderSoundTimer = sliderSoundInterval;
            }
        }
        // YENÝ GÜNCELLENMÝÞ SATIR:
        // Eðer slider deðeri sýfýra çok yakýnsa sesi -80dB (sessiz) yap,
        // deðilse normal logaritmik hesabý yap.
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        // Mixer'a hesaplanan desibel deðerini gönder.
        mainMixer.SetFloat("SFXVolume", dbVolume);

        // Ayarý oyuncunun bilgisayarýna kaydet.
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    private void OnHostClicked()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    private void OnJoinClicked()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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

    // Script'in içine yeni metot olarak ekleyin

    public void SetMusicVolume(float volume)
    {
        // Ses çalma mantýðý
        if (isSettingsInitialized)
        {
            if (sliderSoundTimer <= 0f)
            {
                SoundManager.PlaySound(SoundType.SLIDER_TICK);
                sliderSoundTimer = sliderSoundInterval;
            }
        }

        // Sýfýr deðeri için matematiksel düzeltme
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        // Mixer'a doðru parametre adýyla doðru deðeri gönderme
        mainMixer.SetFloat("MusicVolume", dbVolume);

        // Ayarý kaydetme
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
}