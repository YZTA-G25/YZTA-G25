using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; // Sahne yönetimi için eklendi

// Hangi müziðin çalýnacaðýný belirtmek için bir enum
public enum MusicTrack
{
    MainMenu,
    GameScene
}

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private AudioClip mainMenuMusic; // Menü müziði için
    [SerializeField] private AudioClip gameSceneMusic; // Oyun sahnesi müziði için
    [SerializeField] private AudioMixer mainMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // AudioSource'un çýkýþýný "MusicVolume" grubuna ata
        audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("MusicVolume")[0];
        audioSource.loop = true;

        // Oyun ilk açýldýðýnda menü müziðini çal
        PlayMusic(MusicTrack.MainMenu);
    }

    // Dýþarýdan çaðrýlarak müziði deðiþtirecek olan metot
    public void PlayMusic(MusicTrack track)
    {
        AudioClip clipToPlay = null;

        switch (track)
        {
            case MusicTrack.MainMenu:
                clipToPlay = mainMenuMusic;
                break;
            case MusicTrack.GameScene:
                clipToPlay = gameSceneMusic;
                break;
        }

        // Eðer çalýnan müzik zaten istenen müzikse, tekrar baþlatma
        if (clipToPlay != null && audioSource.clip != clipToPlay)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
}