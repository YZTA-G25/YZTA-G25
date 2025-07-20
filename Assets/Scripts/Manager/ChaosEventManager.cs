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
        if (Instance != null) { Destroy(gameObject); } else { Instance = this; }
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

        Debug.Log($"KAOS OLAYI BAŞLATILIYOR: {selectedEvent.eventType.ToString()}");

        //Olayın mantığını içeren Instance'ı oluştur.
        GameObject behaviourInstance = Instantiate(selectedEvent.behaviourPrefab);

        //Bu objeyi ağ üzerinde de oluştur ve sahipliği sunucuya ver.
        behaviourInstance.GetComponent<NetworkObject>().Spawn(true);



        //Oluşturulan Script'i ayarla ve başlat.
        _activeEventBehaviour = behaviourInstance.GetComponent<ChaosBehaviour>();
        if (_activeEventBehaviour != null)
        {
            _activeEventBehaviour.Initialize(selectedEvent);
            _activeEventBehaviour.StartChaosEvent();
            _activeEventBehaviour.OnChaosEventStarted.Invoke();

            // Olay bittiğinde kendini yok etmesi için bir zamanlayıcı ayarla.
            Invoke(nameof(EndCurrentEvent), selectedEvent.duration);
        }
        else
        {
            Debug.LogError($"Oluşturulan '{selectedEvent.behaviourPrefab.name}' prefab'ında ChaosBehaviour veya türevi bir script bulunamadı!");

            if (behaviourInstance != null)
            {
                behaviourInstance.GetComponent<NetworkObject>().Despawn();
            }
            SetNextEventTimer();
        }
    }

    private void EndCurrentEvent()
    {
        if (_activeEventBehaviour != null)
        {
            Debug.Log($"KAOS OLAYI BİTTİ: {_activeEventBehaviour.eventData.eventType.ToString()}");
            _activeEventBehaviour.EndEvent();
            _activeEventBehaviour.OnChaosEventEnded.Invoke();

            _activeEventBehaviour.GetComponent<NetworkObject>().Despawn();

            _activeEventBehaviour = null;
        }

        SetNextEventTimer();
    }

    private void SetNextEventTimer()
    {
        _timer = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
        Debug.Log($"Bir sonraki kaos olayı yaklaşık {_timer:F0} saniye içerisinde başlayacak.");
    }
}
