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


    [Header("Settings UI")] // Mevcut baþlýðýn altýna veya yeni bir baþlýða
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // Çözünürlükleri saklamak için bir liste
    private List<Resolution> resolutions;

    private void Awake()
    {
        // Tüm butonlarýn týklama olaylarýna ilgili fonksiyonlarý atýyoruz.
        // Her týklama önce sesi çalar, sonra ana fonksiyonu çalýþtýrýr.

        hostButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnHostClicked();
        });

        joinButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnJoinClicked();
        });

        settingsButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnSettingsClicked();
        });

        quitButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnQuitClicked();
        });

        backButton.onClick.AddListener(() => {
            SoundManager.PlaySound(SoundType.MENU_CLICK);
            OnBackClicked();
        });

        // Slider'ýn deðer deðiþtirme olayýna ses fonksiyonunu atýyoruz.
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void Start()
    {
        // Kayýtlý ses ayarýný yükle ve uygula.
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
        // YENÝ GÜNCELLENMÝÞ SATIR:
        // Eðer slider deðeri sýfýra çok yakýnsa sesi -80dB (sessiz) yap,
        // deðilse normal logaritmik hesabý yap.
        float dbVolume = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        // Mixer'a hesaplanan desibel deðerini gönder.
        mainMixer.SetFloat("MasterVolume", dbVolume);

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

}