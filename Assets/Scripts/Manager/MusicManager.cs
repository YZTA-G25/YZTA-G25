using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private AudioClip backgroundMusic;
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
        if (mainMixer == null)
        {
            Debug.LogError("MusicManager'da 'Main Mixer' atanmamýþ! Lütfen Inspector'dan atayýn.");
            return;
        }
        // Mixer'daki grubun adýný "MusicVolume" olarak düzelttik.
        audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("MusicVolume")[0];
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}