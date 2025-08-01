using UnityEngine;
using System;

public enum SoundType
{
    KNIFE,
    FOOTSTEP,
    MENU_CLICK,
    MENU_HOVER,
    SLIDER_TICK,
    MENU_PAUSE,
    OBJECT_GRAB,
    RECIPE_COMPLETE
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;

    // Bu private deðiþken, singleton'ý saklar.
    private static SoundManager instance;

    // Bu public property (özellik), diðer script'lerin 'private' olan 'instance'a
    // güvenli bir þekilde eriþmesini saðlar. Diðer script'ler 'SoundManager.Instance' yazdýðýnda bu kýsým çalýþýr.
    public static SoundManager Instance
    {
        get { return instance; }
    }

    private AudioSource audioSource;

    private void Awake()
    {
        // Eðer sahnede zaten bir SoundManager varsa ve bu o deðilse, kendini yok et.
        // Bu, sahneler arasýnda geçiþ yaparken objenin kopyasýnýn oluþmasýný engeller.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return; // Kalan kodun çalýþmasýný engelle
        }

        // Ýlk defa oluþturuluyorsa, instance olarak kendini ata.
        instance = this;

        // Bu objenin sahne deðiþtirildiðinde yok olmamasýný saðla.
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // AudioSource bileþenini al ve deðiþkene ata.
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1f)
    {
        // Instance'ýn veya AudioSource'un null olup olmadýðýný kontrol etmek her zaman daha güvenlidir.
        if (instance == null || instance.audioSource == null)
        {
            Debug.LogError("SoundManager is not ready or AudioSource is missing!");
            return;
        }

        // Ses listesinde o ses türü için atanmýþ klip olup olmadýðýný kontrol et.
        if ((int)sound >= instance.soundList.Length || instance.soundList[(int)sound].Sounds.Length == 0)
        {
            Debug.LogWarning($"SoundType {sound} has no audio clips assigned in SoundManager!");
            return;
        }

        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        instance.audioSource.PlayOneShot(randomClip, volume);
    }

#if UNITY_EDITOR
    // Bu metot, Unity Editöründe çalýþarak Sound List'in enum ile senkronize olmasýný saðlar.
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
            soundList[i].name = names[i];
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}