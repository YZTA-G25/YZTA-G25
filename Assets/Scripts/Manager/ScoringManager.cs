using UnityEngine;
using Unity.Netcode; // <-- AĞ ÖZELLİKLERİ İÇİN GEREKLİ
using System;

// SINIF ARTIK NETWORKBEHAVIOUR'DAN MİRAS ALIYOR
public class ScoringManager : NetworkBehaviour
{
    public static ScoringManager Instance { get; private set; }

    // Skoru tutan değişkeni NetworkVariable yapıyoruz.
    // Sadece sunucu yazabilir (WritePermission.Server), herkes okuyabilir (ReadPermission.Everyone).
    private NetworkVariable<int> networkScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Event'i hala kullanabiliriz, UI'ı güncellemek için çok kullanışlı.
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else 
        { 
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // OnNetworkSpawn, obje ağda doğduğunda çalışır.
    public override void OnNetworkSpawn()
    {
        // Skor değiştiğinde, OnScoreChanged event'ini tetikle.
        networkScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(newValue);
        };

        // Oyuna yeni başlanan client'lar için mevcut skoru bir kez tetikle
        OnScoreChanged?.Invoke(networkScore.Value);
    }

    // OnNetworkDespawn, obje ağdan kaldırıldığında çalışır.
    public override void OnNetworkDespawn()
    {
        // Event aboneliklerini temizle
        OnScoreChanged = null;
    }

    // Puan ekleme işlemini sunucuda yapan bir ServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int amount)
    {
        if (amount <= 0) return;

        // Bu kod sadece sunucuda çalışır.
        networkScore.Value += amount;
        Debug.Log($"Skor sunucuda eklendi: +{amount}. Yeni Toplam Skor: {networkScore.Value}");
    }

    // Skoru sıfırlama işlemi (sadece sunucu)
    [ServerRpc(RequireOwnership = false)]
    public void ResetScoreServerRpc()
    {
        networkScore.Value = 0;
        Debug.Log("Skor sunucuda sıfırlandı.");
    }

    // Mevcut skoru almak için public property
    public int CurrentScore => networkScore.Value;

    // Skor çıkarma işlemi (isteğe bağlı)
    [ServerRpc(RequireOwnership = false)]
    public void SubtractScoreServerRpc(int amount)
    {
        if (amount <= 0) return;

        networkScore.Value = Mathf.Max(0, networkScore.Value - amount);
        Debug.Log($"Skor sunucuda çıkarıldı: -{amount}. Yeni Toplam Skor: {networkScore.Value}");
    }

    // Belirli bir skora set etme işlemi (admin/debug için)
    [ServerRpc(RequireOwnership = false)]
    public void SetScoreServerRpc(int newScore)
    {
        networkScore.Value = Mathf.Max(0, newScore);
        Debug.Log($"Skor sunucuda ayarlandı: {networkScore.Value}");
    }
}