# Friendslop - Mevcut Durum Belgesi

**Son Güncelleme:** 28 Temmuz 2025

## 1. Ana Konsept (High-Level Concept)

Asimetrik co-op bir oyundur. Sahnede tek bir karakter bedeni bulunur. Bir oyuncu (Host/Göz) bu bedenin kafasını, diğeri (Client/El) ise bedenin geri kalanını ve ellerini kontrol eder.

**Yemek Yapma Mekanikleri:** Oyuncular birlikte çalışarak malzemeleri dolaplardan alır, yemek istasyonuna koyar ve tarifler yapar. Doğru tarifler puan kazandırır.

## 2. Oyuncu Rolleri ve Kontrolleri

### El Oyuncusu (Client)

**Sorumlulukları:**
- Ana karakter bedeninin yürümesinden (WASD) sorumludur
- Ana karakter bedeninin sağa/sola dönüşünden (Mouse X) sorumludur  
- Fiziksel elin hassas hareketlerinden, obje alıp bırakmaktan (Grab tuşu) sorumludur
- Malzemeleri dolaplardan alır ve yemek istasyonuna taşır
- Yemek düğmesine basarak tarifleri pişirir

**Görüşü:**
- Dünyayı doğrudan görmez. Sadece elini ve özel efektleri (halüsinasyon vb.) gösteren kendine ait bir kamerası vardır

**Yapısı:**
```
Hand Player
|
|\_Hand
|    |\_HandHoldPoint
|
|\_Camera Rig
     |\_Hand Player CM (Outputs to Channel01)
|\_Eye Level
|\_Target Group
```

**Component'lar:**
- Hand Player -> Capsule Collider, Network Object, Network Transform, Character Controller, Player Input, Hand Controller, Role Manager
- Hand -> Capsule Collider, Rigidbody, Hand Interactor
- Hand Player CM -> Cinemachine Camera
- Target Group -> Cinemachine Target Group

### Göz Oyuncusu (Host)

**Sorumlulukları:**
- Oyun dünyasında değil, ayrı bir "kontrol odasında" bulunan kendi avatarını (EyeAvatar) yönetir
- Bu odadaki kollarla (levers) etkileşime girerek, ana karakter bedeninin kafasının rotasyonunu (yukarı/aşağı/yanlara bakma) kontrol eder
- El oyuncusuna rehberlik eder ve yemek tariflerini okur

**Görüşü:**
- Kendi avatarının birinci şahıs kamerasıyla control odasını görür
- Kontrol odasındaki bir ekran (RenderTexture) üzerinden, ana karakter bedeninin gözünden dış dünyayı izler

**Yapısı:**
```
Eye Player 
|
|\_Hand
|    |\_HandHoldPoint
|
|\_Eye Level
|\_Target Group
|\_Camera Rig
     |\_Eye Player CM (Outputs to Channel02)
```

**Component'lar:**
- Eye Player -> Capsule Collider, Network Object, Network Transform, Character Controller, Player Input, Eye Player Controller, Role Manager
- Hand -> Capsule Collider, Rigidbody
- Target Group -> Cinemachine Target Group
- Eye Player CM -> Cinemachine Camera

### Cameras

**Görevi:** Ana hiyerarşide bulunur, prefablarda değil. Oyunda bir tane camera yerine farklı channel'lar üzerinden farklı görüntü alan iki adet kamerayı bulundurmak ve EyePlayer Feed'i HandPlayer'a bir Eye Player Feed CM koymadan kontrol etmek.

**Child Objeler:**
```
Cameras
|\_Hand Player Camera (CM Brain)
|\_Eye Player Camera (CM Brain)  
|\_Eye Player Feed Rig
   |\_Eye Player Feed Cam (CM Brain)
   |\_Eye Player Feed CM (Outputs to Channel03)
```

## 3. Temel Mimari Kararları

**Ağ Yapısı:** Host-Client modeli kullanılır (Unity Netcode for GameObjects)

**Oyuncu Oluşturma (Spawning):** GameManager script'i, NetworkManager'ın bağlantı onayı mekanizmasını kullanarak Host için eyePlayerPF'i ve Client için handPlayerPF'i spawn eder

**Etkileşim Sistemi:** Fiziksel etkileşimler, elin üzerindeki HandInteractor script'i ve trigger sistemi ile yapılır. IInteractable interface'i kullanılır

**Yemek Sistemi:** Trigger-based malzeme ekleme/çıkarma sistemi. NetworkObjectId'ler ile tam obje takibi

## 4. Ana Yemek Yapma Sistemleri

### CookingStation (Trigger-Based Sistem)

Malzemeleri otomatik olarak algılar ve NetworkObjectId ile izler:

```csharp
public class CookingStation : NetworkBehaviour, IInteractable
{
    private NetworkList<ulong> objectsOnStation; // NetworkObjectId'leri saklar
    
    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (interactable is GrabbableItem grabbable)
        {
            NetworkObject networkObject = grabbable.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                AddIngredientServerRpc(networkObject.NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(ulong networkObjectId)
    {
        if (!objectsOnStation.Contains(networkObjectId))
        {
            objectsOnStation.Add(networkObjectId);
            // GrabbableItem'ı bul ve inFood = true yap
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ValidateAndCookServerRpc()
    {
        // NetworkObjectId'lerden gerçek ingredient'leri al
        List<Ingredient> submittedIngredients = new List<Ingredient>();
        List<NetworkObject> objectsToDestroy = new List<NetworkObject>();
        
        foreach (ulong networkObjectId in objectsOnStation)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                GrabbableItem grabbable = networkObject.GetComponent<GrabbableItem>();
                if (grabbable != null)
                {
                    Ingredient ingredient = ingredientDatabase.GetIngredientById(grabbable.data.ID);
                    if (ingredient != null)
                    {
                        submittedIngredients.Add(ingredient);
                        objectsToDestroy.Add(networkObject);
                    }
                }
            }
        }
        
        // Tarifi doğrula ve objeleri yok et
        bool isCorrect = RecipeValidator.ValidateRecipe(currentTargetRecipe, submittedIngredients);
        
        if (isCorrect)
        {
            ScoringManager.Instance.AddScoreServerRpc(currentTargetRecipe.scoreValue);
        }
        
        // Objeleri yok et
        foreach (NetworkObject obj in objectsToDestroy)
        {
            if (obj != null && obj.IsSpawned)
            {
                obj.Despawn(true);
            }
        }
        
        objectsOnStation.Clear();
    }
}
```

### Malzeme Sistemi

**Ingredient ScriptableObject:**
```csharp
[CreateAssetMenu(fileName = "NewIngredient", menuName = "ScriptableObjects/Ingredient")]
public class Ingredient : ScriptableObject, IScriptableObject
{
    [SerializeField] private ulong _id;
    public ulong ID => _id;
    public string Name => name;
    public Sprite icon;
    public GameObject prefab;
    public bool isSafe;
    public float cookTime;
    public LayerMask layerMask;
}
```

**GrabbableItem (Network Synchronized):**
```csharp
public class GrabbableItem : NetworkBehaviour, IInteractable
{
    [Header("Data")]
    public Ingredient data;
    
    [Header("Grab Settings")]  
    public bool inFood = false; // Yemekte olup olmadığını belirtir
    
    // Network senkronizasyonu için
    private NetworkVariable<bool> networkIsGrabbed = new NetworkVariable<bool>();
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<ulong> grabbingClientId = new NetworkVariable<ulong>();
    
    // Tutma mantığı
    private HandInteractor currentInteractor;
    private bool isBeingGrabbed = false;
    
    public void Grab(HandInteractor interactor)
    {
        if (currentInteractor != null || interactor.IsHoldingSomething()) return;
        
        currentInteractor = interactor;
        SetupGrabbedState();
        isBeingGrabbed = true;
        
        // Network sahipliğini al
        if (IsSpawned && !IsOwner)
        {
            RequestOwnershipServerRpc();
        }
        
        UpdateGrabStateServerRpc(true, NetworkManager.Singleton.LocalClientId);
    }
    
    public void Release()
    {
        isBeingGrabbed = false;
        
        // Fırlatma hızını hesapla
        Vector3 throwVelocity = Vector3.zero;
        if (currentInteractor != null)
        {
            throwVelocity = currentInteractor.CalculateHandVelocity();
        }
        
        // Sahipliği sunucuya geri ver
        if (IsSpawned && IsOwner)
        {
            TransferOwnershipToServerServerRpc(transform.position, throwVelocity);
        }
        
        RestoreOriginalState(throwVelocity);
        currentInteractor = null;
        UpdateGrabStateServerRpc(false, 0);
    }
}
```

### Dolap Sistemi

**CabinetController:**
```csharp
public class CabinetController : NetworkBehaviour, IInteractable
{
    [SerializeField] private Ingredient ingredient; // Tek malzeme türü
    
    public void Interact(HandInteractor interactor)
    {
        if (ingredient == null || interactor.IsHoldingSomething()) return;
        
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnIngredientForPlayer(interactor);
        }
        else
        {
            RequestIngredientRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void RequestIngredientRpc(ulong requestingClientId)
    {
        // ClientId'den HandInteractor'ı bul ve malzeme spawn et
        SpawnIngredientForPlayer(/* bulunacak interactor */);
    }
    
    private void SpawnIngredientForPlayer(HandInteractor interactor)
    {
        if (ingredient?.prefab == null) return;
        
        GameObject spawnedObject = Instantiate(ingredient.prefab, transform.position, transform.rotation);
        NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
        
        if (networkObject != null)
        {
            networkObject.Spawn();
            
            // Otomatik olarak eline ver
            GrabbableComponent grabbableComponent = spawnedObject.GetComponent<GrabbableItem>();
            if (grabbableComponent != null)
            {
                grabbableComponent.Grab(interactor);
            }
        }
    }
}
```

## 5. Kilit Script'lerin Detayları

### GameManager.cs
```csharp
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject handPlayerPF;
    [SerializeField] private GameObject eyePlayerPF;

    [Header("Spawn Points")]
    [SerializeField] private Transform handPlayerSpawnPoint;
    [SerializeField] private Transform eyePlayerSpawnPoint;

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[GameManager] OnNetworkSpawn called! IsServer: {IsServer}");
        
        if (!IsServer) return;
        
        // Host için EyePlayer, Client için HandPlayer spawn eder
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        bool isHost = (clientId == NetworkManager.Singleton.LocalClientId);
        SpawnPlayerForClient(clientId, isHost);
    }

    private void SpawnPlayerForClient(ulong clientId, bool isHost)
    {
        GameObject playerPrefab = isHost ? eyePlayerPF : handPlayerPF;
        Transform spawnPoint = isHost ? eyePlayerSpawnPoint : handPlayerSpawnPoint;
        
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
    }
}
```

### ScoringManager.cs
```csharp
using UnityEngine;
using Unity.Netcode;
using System;

public class ScoringManager : NetworkBehaviour
{
    public static ScoringManager Instance { get; private set; }

    private NetworkVariable<int> networkScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; }
    }

    public override void OnNetworkSpawn()
    {
        networkScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(newValue);
        };

        OnScoreChanged?.Invoke(networkScore.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int amount)
    {
        if (amount <= 0) return;
        networkScore.Value += amount;
        Debug.Log($"Skor eklendi: +{amount}. Toplam: {networkScore.Value}");
    }
}
```

### HandInteractor.cs
```csharp
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HandInteractor : MonoBehaviour
{
    [Header("Kinematic Grabbing Settings")]
    [SerializeField] private Transform handHoldPoint;
    [SerializeField] private float grabRange = 1.5f;
    [SerializeField] private LayerMask grabbableLayer = -1;
    [SerializeField] private float throwForceMultiplier = 5f;
    [SerializeField] private float velocityThreshold = 2f;
    [SerializeField] private int velocitySamples = 5;

    private GameObject grabbedObject;
    private IInteractable currentInteractable;
    private List<Vector3> handPositions = new List<Vector3>();
    private List<float> handTimes = new List<float>();

    private void Update()
    {
        TrackHandVelocity();
        
        if (grabbedObject == null)
        {
            FindInteractable();
        }
    }

    private void FindInteractable()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, grabRange, grabbableLayer);
        
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        SetCurrentInteractable(closestInteractable);
    }

    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Grab(this);
            }
        }
        else if (context.canceled)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Release();
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    public Vector3 CalculateHandVelocity()
    {
        if (handPositions.Count < 2) return Vector3.zero;
        
        Vector3 totalVelocity = Vector3.zero;
        int validSamples = 0;
        
        for (int i = 1; i < handPositions.Count; i++)
        {
            float deltaTime = handTimes[i] - handTimes[i - 1];
            if (deltaTime > 0)
            {
                Vector3 velocity = (handPositions[i] - handPositions[i - 1]) / deltaTime;
                totalVelocity += velocity;
                validSamples++;
            }
        }
        
        return validSamples > 0 ? totalVelocity / validSamples : Vector3.zero;
    }
    
    public bool IsHoldingSomething() => grabbedObject != null;
    public GameObject GetHeldObject() => grabbedObject;
    public Transform GetHandHoldPoint() => handHoldPoint;
    public float GetVelocityThreshold() => velocityThreshold;
    public float GetThrowForceMultiplier() => throwForceMultiplier;
}
```

### IInteractable.cs
```csharp
using UnityEngine;

public interface IInteractable
{
    public void Interact(HandInteractor interact);
    public void Grab(HandInteractor interactor);
    public void Release();
}
```

### CookButton.cs
```csharp
using UnityEngine;

public class CookButton : MonoBehaviour, IInteractable
{
    [SerializeField] private CookingStation cookingStation;

    public void Grab(HandInteractor interactor)
    {
        Interact(interactor);
    }

    public void Interact(HandInteractor interactor)
    {
        if (cookingStation == null)
        {
            Debug.LogError("CookButton'a CookingStation atanmamış!");
            return;
        }

        cookingStation.ValidateAndCook();
    }

    public void Release() { }
}
```

## 6. Chaos Event Sistemi

### ChaosEventManager.cs
```csharp
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ChaosEventManager : NetworkBehaviour
{
    public static ChaosEventManager Instance { get; private set; }

    [SerializeField] private List<ChaosEvent> allPossibleEvents;
    [SerializeField] private float minTimeBetweenEvents = 60f;
    [SerializeField] private float maxTimeBetweenEvents = 120f;

    private float _timer;
    private ChaosBehaviour _activeEventBehaviour;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); } 
        else { Instance = this; }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { enabled = false; return; }
        SetNextEventTimer();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (_activeEventBehaviour == null)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                TriggerRandomEvent();
            }
        }
        else
        {
            _activeEventBehaviour.UpdateChaosEvent();
        }
    }

    private void TriggerRandomEvent()
    {
        if (allPossibleEvents.Count <= 0) return;

        ChaosEvent selectedEvent = allPossibleEvents[Random.Range(0, allPossibleEvents.Count)];
        Debug.Log($"KAOS OLAYI BAŞLATILIYOR: {selectedEvent.eventType}");

        if (selectedEvent.behaviourPrefab != null)
        {
            GameObject behaviourInstance = Instantiate(selectedEvent.behaviourPrefab);
            _activeEventBehaviour = behaviourInstance.GetComponent<ChaosBehaviour>();
            
            if (_activeEventBehaviour != null)
            {
                _activeEventBehaviour.StartChaosEvent(selectedEvent, EndCurrentEvent);
            }
        }
    }

    private void EndCurrentEvent()
    {
        if (_activeEventBehaviour != null)
        {
            Destroy(_activeEventBehaviour.gameObject);
            _activeEventBehaviour = null;
        }
        SetNextEventTimer();
    }

    private void SetNextEventTimer()
    {
        _timer = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
    }
}
```

### ChaosEvent.cs
```csharp
using UnityEngine;

public enum ChaosEventType
{
    Storm,
    Levitation,
    EyeDazzle,
    LoseBalance,
    Hallucination
}

public class ChaosEvent : ScriptableObject, IScriptableObject
{
    [SerializeField] private ulong _id;
    public ulong ID => _id;
    public string Name => name;
    public ChaosEventType eventType;
    [TextArea] public string description;
    public float duration;

    [Header("Event Logic")]
    public GameObject behaviourPrefab;
}
```

## 7. UI Sistemi

### ScoreUI.cs
```csharp
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        scoreText.text = "Skor: 0";
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged += UpdateScoreText;
        }
    }

    private void OnDestroy()
    {
        if (ScoringManager.Instance != null)
        {
            ScoringManager.Instance.OnScoreChanged -= UpdateScoreText;
        }
    }

    private void UpdateScoreText(int newScore)
    {
        scoreText.text = "Skor: " + newScore;
    }
}
```

### InGameUIManager.cs
```csharp
using TMPro;
using Unity.Netcode;
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
    [SerializeField] private Button backButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Audio")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mainMixer;

    [Header("Settings UI")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Disconnect UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private bool isPaused = false;
    public static event System.Action<bool> OnGamePaused;

    private void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (backButton != null) backButton.onClick.AddListener(CloseSettings);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        
        // Audio slider'ları ayarla
        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        OnGamePaused?.Invoke(false);
    }

    private void SetMasterVolume(float volume)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    private void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
}
```

## 8. Audio Sistemi

### SoundManager.cs
```csharp
public enum SoundType
{
    OBJECT_GRAB,
    RECIPE_COMPLETE,
    BUTTON_CLICK
}

public static class SoundManager
{
    public static void PlaySound(SoundType soundType)
    {
        // Ses çalma mantığı
        Debug.Log($"Ses çalınıyor: {soundType}");
    }
}
```

### MusicManager.cs
```csharp
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameSceneMusic;
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
        audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("MusicVolume")[0];
        audioSource.loop = true;
        PlayMusic(MusicTrack.MainMenu);
    }

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
        
        if (clipToPlay != null && audioSource.clip != clipToPlay)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
}
```

## 9. Recipe Sistemi

### Recipe.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "ScriptableObjects/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public List<Ingredient> requiredIngredients;
    public Sprite recipeImage;
    [TextArea]
    public string cookingInstructions;
    public int scoreValue;
}
```

### RecipeValidator.cs
```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class RecipeValidator
{
    public static bool ValidateRecipe(Recipe targetRecipe, List<Ingredient> submittedIngredients)
    {
        if (targetRecipe == null || targetRecipe.requiredIngredients == null || submittedIngredients == null)
        {
            Debug.LogError("Tarif veya malzeme listesi NULL olamaz.");
            return false;
        }

        if (targetRecipe.requiredIngredients.Count != submittedIngredients.Count)
        {
            return false;
        }

        var checklist = new List<string>(targetRecipe.requiredIngredients.Select(x => x.Name));

        foreach (var submittedIngredient in submittedIngredients)
        {
            if (checklist.Contains(submittedIngredient.Name) && submittedIngredient.isSafe)
            {
                checklist.Remove(submittedIngredient.Name);
            }
            else
            {
                return false;
            }
        }

        return checklist.Count == 0;
    }
}
```

## 10. Player Controller'ları

### EyePlayerController.cs
```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class EyePlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float pitchLimit = 85f;

    [Header("Gravity & Jumping")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Component References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private float currentPitch = 0f;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isGrounded;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Input system setup for owner only
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Disable components for non-owners
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleMouseLook();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, -pitchLimit, pitchLimit);
        cameraTransform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    private void HandleGravity()
    {
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
```

### HandController.cs
```csharp
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class HandController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Component References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerBody;

    private Vector2 moveInput;
    private float mouseXInput;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float mouseX = mouseXInput * mouseSensitivity * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        mouseXInput = lookInput.x;
    }
}
```

## 11. Yeni Değişiklikler ve Güncellemeler

### 🔄 Son Değişiklikler (28 Temmuz 2025)

**1. CookingStation Trigger Sistemi:**
- ✅ **YENİ:** Otomatik trigger-based malzeme algılama
- ✅ **YENİ:** NetworkObjectId ile tam obje takibi  
- ❌ **ESKİ:** Manuel etkileşim sistemi kaldırıldı
- ❌ **ESKİ:** Ingredient ID'si ile takip (aynı malzemeleri ayırt edemiyordu)

**2. GrabbableItem Network Senkronizasyonu:**
- ✅ **YENİ:** Gelişmiş network ownership sistemi
- ✅ **YENİ:** Fırlatma mekaniği network üzerinden
- ✅ **YENİ:** `inFood` flag ile malzeme durumu takibi
- ❌ **ESKİ:** Basit grabbing sistemi

**3. Scoring Sistemi:**
- ✅ **YENİ:** NetworkVariable ile senkronize skor
- ✅ **YENİ:** ServerRpc ile güvenli skor ekleme
- ✅ **YENİ:** Event-based UI güncellemesi

**4. Cabinet Sistemi:**
- ✅ **YENİ:** Tek malzeme türü odaklı system
- ✅ **YENİ:** Network spawn ile otomatik eline verme
- ❌ **ESKİ:** Çoklu malzeme seçimi kaldırıldı

**5. Recipe Validation:**
- ✅ **YENİ:** `isSafe` kontrolü eklendi
- ✅ **YENİ:** Tam malzeme eşleştirme sistemi
- ✅ **YENİ:** Dinamik puanlama sistemi

### 🗑️ Kaldırılan Özellikler

**1. Manuel Interaction System:**
- Oyuncunun elle malzeme ekleme sistemisxs
- Button ile etkileşim (CookButton hariç)

**2. Visual Slot System:**
- İstasyonda malzemelerin görsel temsili
- Slot bazlı yerleştirme sistemi

**3. Ingredient ID Tracking:**
- Aynı malzeme türlerini ayırt edememe problemi
- Yanlış obje silme hataları

### 🎯 Gelecek Planları

**1. Chaos Event System:**
- Halüsinasyon efektleri
- Lever bazlı kontrolü bozma
- Zamanlı event sistemi

**2. Advanced UI:**
- Recipe kitap sistemi
- Crosshair feedback
- Pause menü sistemi

**3. Audio Integration:**
- Ambiyans müzikleri
- Etkileşim sesleri
- UI feedback sesleri

## 12. Teknik Notlar

### Network Architecture
- **Host:** Server + Client (EyePlayer)
- **Client:** Sadece client (HandPlayer)  
- **NetworkList:** ObjectId tracking için
- **ServerRpc:** Tüm game logic için
- **NetworkVariable:** Sync data için

### Performance Considerations
- NetworkObjectId kullanımı ile O(1) obje erişimi
- Trigger sisteminin optimizasyonu
- Network ownership transferleri

### Known Issues
- ScoringManager singleton network spawn problemleri
- Socket connection errors (geçici)
- NetworkPrefab registration gereklilikleri

---

**Not:** Bu belge aktif development sırasında güncellenir. Herhangi bir değişiklik veya ekleme için documentation'ı güncel tutmayı unutmayın.
