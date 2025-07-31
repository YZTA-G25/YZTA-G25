// using satirlari...

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CookingStation : NetworkBehaviour, IInteractable
{
    [Header("Data")]
    [Tooltip("Olusturduğumuz IngredientDatabase asset'ini buraya sürükleyin.")]
    [SerializeField] private IngredientDatabase ingredientDatabase; // VERITABANI REFERANSI

    [Header("Recipe")]
    [SerializeField] private Recipe currentTargetRecipe;

    private NetworkList<ulong> objectsOnStation; // Store NetworkObjectIds instead of ingredient IDs

    private void Awake()
    {
        objectsOnStation = new NetworkList<ulong>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Sadece IInteractable componentine sahip objeler kabul edilir.
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (interactable is GrabbableItem grabbable)
        {
            Debug.Log($"{grabbable.data.Name} malzemesi istasyona girdi.");

            // NetworkObject componentini al
            NetworkObject networkObject = grabbable.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Sunucuya malzemeyi listeye ekleme isteği gönder.
                ulong networkId = networkObject.NetworkObjectId;
                AddIngredientServerRpc(networkId);
            }
            else
            {
                Debug.LogError("GrabbableItem'da NetworkObject componenti bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("GİREN GRABBABLE DEĞİL");
            throw new System.Exception("İÇİNE GİREN GRABBABLE DEĞİL. GİREMEMELİYDİ.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Sadece IInteractable componentine sahip objeler kabul edilir.
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        if (interactable is GrabbableItem grabbable)
        {
            Debug.Log($"{grabbable.data.Name} malzemesi istasyondan çıktı.");

            // NetworkObject componentini al
            NetworkObject networkObject = grabbable.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Sunucuya malzemeyi listeden çıkarma isteği gönder.
                RemoveIngredientServerRpc(networkObject.NetworkObjectId);
            }
            else
            {
                Debug.LogError("GrabbableItem'da NetworkObject componenti bulunamadı!");
            }
        }
        else
        {
            Debug.LogError("ÇIKAN GRABBABLE DEĞİL");
            throw new System.Exception("ÇIKAN GRABBABLE DEĞİL, GİREMEMELİYDİ");
        }
    }

    /// <summary>
    /// Sadece sunucu üzerinde çalışan ve malzemeyi istasyonun listesine ekleyen metot.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(ulong networkObjectId)
    {
        // NetworkObject'i NetworkObjectId'den bul
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            GrabbableItem grabbable = networkObject.GetComponent<GrabbableItem>();
            if (grabbable != null)
            {
                Debug.Log($"Sunucu, {grabbable.data.ID} ID'li {grabbable.data.Name} 'i istasyona ekleme isteği aldı.");

                // NetworkObjectId'yi listeye ekle (ingredient ID'yi değil)
                // Bu değişiklik otomatik olarak tüm client'lara bildirilecek.
                if (!objectsOnStation.Contains(networkObjectId))
                {
                    objectsOnStation.Add(networkObjectId);
                    grabbable.inFood = true;
                    Debug.Log($"NetworkObjectId {networkObjectId} istasyona eklendi. Toplam obje sayısı: {objectsOnStation.Count}");
                }
            }
            else
            {
                Debug.LogError("NetworkObject'te GrabbableItem componenti bulunamadı!");
            }
        }
        else
        {
            Debug.LogError($"NetworkObjectId {networkObjectId} bulunamadı!");
        }
    }

    /// <summary>
    /// Sadece sunucu üzerinde çalışan ve malzemeyi istasyonun listesinden çıkaran metot.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RemoveIngredientServerRpc(ulong networkObjectId)
    {
        // NetworkObject'i NetworkObjectId'den bul
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            GrabbableItem grabbable = networkObject.GetComponent<GrabbableItem>();
            if (grabbable != null)
            {
                Debug.Log($"Sunucu, {grabbable.data.ID} ID'li {grabbable.data.Name} 'i istasyondan çıkarma isteği aldı.");

                // NetworkObjectId'yi listeden çıkar
                if (objectsOnStation.Contains(networkObjectId))
                {
                    objectsOnStation.Remove(networkObjectId);
                    grabbable.inFood = false;
                    Debug.Log($"NetworkObjectId {networkObjectId} istasyondan çıkarıldı. Kalan obje sayısı: {objectsOnStation.Count}");
                }
            }
            else
            {
                Debug.LogError("NetworkObject'te GrabbableItem componenti bulunamadı!");
            }
        }
        else
        {
            Debug.LogError($"NetworkObjectId {networkObjectId} bulunamadı!");
        }
    }

    public void Release() 
    { 

    }
    public void Interact(HandInteractor interactor)
    {
        // Bu metot artık kullanılmıyor, trigger sistemi ile otomatik çalışıyor.
    }

    public void ValidateAndCook()
    {
        Debug.Log("Pişirme isteği alındı. Sunucuya gönderiliyor...");
        ValidateAndCookServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ValidateAndCookServerRpc()
    {
        // Hedef tarif atanmamışsa işlem yapma.
        if (currentTargetRecipe == null)
        {
            Debug.LogError("CookingStation'da currentTargetRecipe atanmamış!");
            return;
        }

        // 1. NetworkList'teki NetworkObjectId'leri gerçek Ingredient listesine dönüştür.
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
                        objectsToDestroy.Add(networkObject); // Aynı zamanda destroy listesine ekle
                    }
                }
            }
        }

        Debug.Log($"Submitted ingredients are : [ {string.Join(" , ", submittedIngredients.Select(x => x.Name))} ]");

        // 2. RecipeValidator'ı kullanarak tarifi doğrula.
        bool isCorrect = RecipeValidator.ValidateRecipe(currentTargetRecipe, submittedIngredients);

        if (isCorrect)
        {
            // 3. Tarif DOĞRUYSA: Puan ekle!
            Debug.Log($"Tarif doğru! {currentTargetRecipe.scoreValue} puan ekleniyor.");
            
            // Try to deliver to customer first
            if (CustomerManager.Instance != null)
            {
                CustomerManager.Instance.TryDeliverOrder(currentTargetRecipe);
            }
            else
            {
                // Fallback to old scoring system if no CustomerManager
                ScoringManager.Instance.AddScoreServerRpc(currentTargetRecipe.scoreValue);
            }

            SoundManager.PlaySound(SoundType.RECIPE_COMPLETE);

        }
        else
        {
            // 4. Tarif YANLIŞSA: Hata mesajı ver. (Gelecekte buraya ceza mekaniği eklenebilir)
            Debug.Log("Tarif yanlış! Malzemeler ziyan oldu.");
        }

        // 5. Her iki durumda da istasyonu temizle.
        // Bulunan objeleri yok et (artık exact objeleri biliyoruz)
        foreach (NetworkObject obj in objectsToDestroy)
        {
            // Objenin hala spawn edilmiş olduğunu kontrol et
            if (obj != null && obj.IsSpawned)
            {
                obj.Despawn(true); // true = destroy on despawn
            }
            else
            {
                Debug.LogWarning("Despawn edilmeye çalışılan obje zaten spawn edilmemiş veya null!");
            }
        }
        
        // Listeyi temizle
        objectsOnStation.Clear();
    }

    public void Grab(HandInteractor interactor)
    {
    }
}