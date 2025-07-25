// using satýrlarý...

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CookingStation : NetworkBehaviour, IInteractable
{
    [Header("Data")]
    [Tooltip("Oluþturduðumuz IngredientDatabase asset'ini buraya sürükleyin.")]
    [SerializeField] private IngredientDatabase ingredientDatabase; // VERÝTABANI REFERANSI

    [Header("Recipe")]
    [SerializeField] private Recipe currentTargetRecipe;

    [Header("Visuals")]
    [SerializeField] private Transform[] visualSlots;

    private NetworkList<ushort> ingredientsOnStation;
    private List<GameObject> visualIngredientObjects = new List<GameObject>();


    // --- BAÞLANGIÇ METOTLARI ---

    // Bu obje ilk oluþturulduðunda çalýþýr.
    private void Awake()
    {
        // NetworkList'i burada new() ile oluþturuyoruz.
        ingredientsOnStation = new NetworkList<ushort>();
    }

    // Bu obje að üzerinde spawn olduðunda çalýþýr.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Að üzerindeki liste her deðiþtiðinde OnIngredientsChanged metodunu çaðýr.
        ingredientsOnStation.OnListChanged += OnIngredientsChanged;
    }

    // Bu obje aðdan kaldýrýldýðýnda çalýþýr.
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        // Hafýza sýzýntýsýný önlemek için olay aboneliðini iptal et.
        ingredientsOnStation.OnListChanged -= OnIngredientsChanged;
    }


    /// <summary>
    /// Oyuncu bu objeyle etkileþime girdiðinde (client üzerinde) çaðrýlýr.
    /// Tek görevi, sunucuya bir istek göndermektir.
    /// </summary>
    public void Interact(HandInteractor interactor)
    {
        // Oyuncunun eli boþsa hiçbir þey yapma.
        if (!interactor.IsHoldingSomething())
        {
            Debug.Log("Oyuncunun eli boþ, istasyona bir þey eklenemez.");
            return;
        }

        // Elindeki objenin bilgilerini al.
        GameObject heldObject = interactor.GetHeldObject();
        IngredientHolder ingredientHolder = heldObject.GetComponent<IngredientHolder>();
        NetworkObject heldObjectNetworkObject = heldObject.GetComponent<NetworkObject>();

        // Eðer tutulan obje bir malzeme deðilse veya að objesi deðilse iþlem yapma.
        if (ingredientHolder == null || heldObjectNetworkObject == null)
        {
            Debug.LogWarning("Tutulan obje geçerli bir malzeme deðil.");
            return;
        }

        Debug.Log($"Oyuncu, {ingredientHolder.ingredientData.ingredientName} malzemesini istasyona eklemek istiyor. Sunucuya istek gönderiliyor...");

        // Sunucudan bu malzemeyi istasyona eklemesini ve elimizdekini yok etmesini isteyelim.
        AddIngredientServerRpc(ingredientHolder.ingredientData.ingredientId, heldObjectNetworkObject.NetworkObjectId);
    }

    /// <summary>
    /// Sadece sunucu üzerinde çalýþan ve malzemeyi istasyonun listesine ekleyen metot.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(ushort ingredientId, ulong heldObjectNetworkId)
    {
        Debug.Log($"Sunucu, {ingredientId} ID'li malzemeyi istasyona ekleme isteði aldý.");

        // 1. Malzeme ID'sini að üzerindeki listeye ekle.
        //    Bu deðiþiklik otomatik olarak tüm client'lara bildirilecek.
        ingredientsOnStation.Add(ingredientId);

        // 2. Oyuncunun elindeki orijinal NetworkObject'i bul.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(heldObjectNetworkId, out NetworkObject objectToDespawn))
        {
            // 3. O objeyi aðdan kaldýr ve yok et.
            objectToDespawn.Despawn(true);
            Debug.Log($"Sunucu, {heldObjectNetworkId} Network ID'li objeyi baþarýyla yok etti.");
        }
        else
        {
            Debug.LogError($"Sunucu, {heldObjectNetworkId} Network ID'li objeyi bulamadý ve yok edemedi!");
        }
    }

    public void Release() 
    { 

    }

    // NetworkList deðiþtiðinde otomatik olarak tetiklenir.
    private void OnIngredientsChanged(NetworkListEvent<ushort> changeEvent)
    {
        // Önce istasyon üzerindeki eski görselleri temizle.
        foreach (var visualObject in visualIngredientObjects)
        {
            Destroy(visualObject);
        }
        visualIngredientObjects.Clear();

        Debug.Log($"Görseller güncelleniyor. Ýstasyonda {ingredientsOnStation.Count} adet malzeme var.");

        // Þimdi güncel listedeki her bir malzeme ID'si için yeni bir görsel oluþtur.
        for (int i = 0; i < ingredientsOnStation.Count; i++)
        {
            // Eðer slot sayýsýndan fazla malzeme varsa daha fazla görsel oluþturma.
            if (i >= visualSlots.Length) break;

            ushort ingredientId = ingredientsOnStation[i];
            Ingredient ingredientData = ingredientDatabase.GetIngredientById(ingredientId);

            if (ingredientData != null)
            {
                // Malzemenin prefab'ýný doðru slotun içine oluþtur.
                GameObject visualInstance = Instantiate(ingredientData.prefab, visualSlots[i]);
                visualInstance.transform.localPosition = Vector3.zero; // Slot'un tam ortasýna yerleþtir.
                visualInstance.transform.localRotation = Quaternion.identity;

                // --- ÇOK ÖNEMLÝ ADIM: Görsel kopyayý "etkisizleþtirme" ---
                // Bu kopya sadece bir görüntü olmalý, tekrar tutulmamalý veya fizikle etkileþmemeli.
                if (visualInstance.GetComponent<NetworkObject>()) Destroy(visualInstance.GetComponent<NetworkObject>());
                if (visualInstance.GetComponent<GrabbableItem>()) Destroy(visualInstance.GetComponent<GrabbableItem>());
                if (visualInstance.GetComponent<Rigidbody>()) Destroy(visualInstance.GetComponent<Rigidbody>());
                if (visualInstance.GetComponent<Collider>()) Destroy(visualInstance.GetComponent<Collider>());

                // Oluþturulan bu görsel objeyi daha sonra silebilmek için listeye ekle.
                visualIngredientObjects.Add(visualInstance);
            }
        }
    }


    public void ValidateAndCook()
    {
        Debug.Log("Piþirme isteði alýndý. Sunucuya gönderiliyor...");
        ValidateAndCookServerRpc();
    }

    /// <summary>
    /// Sunucu üzerinde çalýþýr, tarifi doðrular, puan ekler ve istasyonu temizler.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void ValidateAndCookServerRpc()
    {
        // Hedef tarif atanmamýþsa iþlem yapma.
        if (currentTargetRecipe == null)
        {
            Debug.LogError("CookingStation'da currentTargetRecipe atanmamýþ!");
            return;
        }

        // 1. NetworkList'teki ID'leri gerçek Ingredient listesine dönüþtür.
        List<Ingredient> submittedIngredients = new List<Ingredient>();
        foreach (ushort id in ingredientsOnStation)
        {
            Ingredient ingredient = ingredientDatabase.GetIngredientById(id);
            if (ingredient != null)
            {
                submittedIngredients.Add(ingredient);
            }
        }

        // 2. RecipeValidator'ý kullanarak tarifi doðrula.
        bool isCorrect = RecipeValidator.ValidateRecipe(currentTargetRecipe, submittedIngredients);

        if (isCorrect)
        {
            // 3. Tarif DOÐRUYSA: Puan ekle!
            Debug.Log($"Tarif doðru! {currentTargetRecipe.scoreValue} puan ekleniyor.");
            ScoringManager.Instance.AddScoreServerRpc(currentTargetRecipe.scoreValue);

            SoundManager.PlaySound(SoundType.RECIPE_COMPLETE);

        }
        else
        {
            // 4. Tarif YANLIÞSA: Hata mesajý ver. (Gelecekte buraya ceza mekaniði eklenebilir)
            Debug.Log("Tarif yanlýþ! Malzemeler ziyan oldu.");
        }

        // 5. Her iki durumda da istasyonu temizle.
        ingredientsOnStation.Clear();
    }
}