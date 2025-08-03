using UnityEngine;
using Unity.Netcode; // <-- A� �ZELL�KLER� ���N GEREKL�
using System;

// SINIF ARTIK NETWORKBEHAVIOUR'DAN M�RAS ALIYOR
public class ScoringManager : NetworkBehaviour
{
    public static ScoringManager Instance { get; private set; }

    // Skoru tutan de�i�keni NetworkVariable yap�yoruz.
    // Sadece sunucu yazabilir (WritePermission.Server), herkes okuyabilir (ReadPermission.Everyone).
    private NetworkVariable<int> networkScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Event'i hala kullanabiliriz, UI'� g�ncellemek i�in �ok kullan��l�.
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; }
    }

    // OnNetworkSpawn, obje a�da do�du�unda �al���r.
    public override void OnNetworkSpawn()
    {
        // Skor de�i�ti�inde, OnScoreChanged event'ini tetikle.
        networkScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(newValue);
        };

        // Oyuna yeni ba�lanan client'lar i�in mevcut skoru bir kez tetikle
        OnScoreChanged?.Invoke(networkScore.Value);
    }

    // Puan ekleme i�lemini sunucuda yapan bir ServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int amount)
    {
        if (amount <= 0) return;

        // Bu kod sadece sunucuda �al���r.
        networkScore.Value += amount;
        Debug.Log($"Skor sunucuda eklendi: +{amount}. Yeni Toplam Skor: {networkScore.Value}");
    }
}