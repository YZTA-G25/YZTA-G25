using UnityEngine;
using Unity.Netcode; // <-- AÐ ÖZELLÝKLERÝ ÝÇÝN GEREKLÝ
using System;

// SINIF ARTIK NETWORKBEHAVIOUR'DAN MÝRAS ALIYOR
public class ScoringManager : NetworkBehaviour
{
    public static ScoringManager Instance { get; private set; }

    // Skoru tutan deðiþkeni NetworkVariable yapýyoruz.
    // Sadece sunucu yazabilir (WritePermission.Server), herkes okuyabilir (ReadPermission.Everyone).
    private NetworkVariable<int> networkScore = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Event'i hala kullanabiliriz, UI'ý güncellemek için çok kullanýþlý.
    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; }
    }

    // OnNetworkSpawn, obje aðda doðduðunda çalýþýr.
    public override void OnNetworkSpawn()
    {
        // Skor deðiþtiðinde, OnScoreChanged event'ini tetikle.
        networkScore.OnValueChanged += (int previousValue, int newValue) =>
        {
            OnScoreChanged?.Invoke(newValue);
        };

        // Oyuna yeni baðlanan client'lar için mevcut skoru bir kez tetikle
        OnScoreChanged?.Invoke(networkScore.Value);
    }

    // Puan ekleme iþlemini sunucuda yapan bir ServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int amount)
    {
        if (amount <= 0) return;

        // Bu kod sadece sunucuda çalýþýr.
        networkScore.Value += amount;
        Debug.Log($"Skor sunucuda eklendi: +{amount}. Yeni Toplam Skor: {networkScore.Value}");
    }
}