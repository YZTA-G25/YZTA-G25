using UnityEngine;

// Bu düðme de IInteractable olmalý ki oyuncu etkileþime girebilsin.
public class CookButton : MonoBehaviour, IInteractable
{
    [Tooltip("Bu düðmenin baðlý olduðu CookingStation.")]
    [SerializeField] private CookingStation cookingStation;

    // Oyuncu bu düðmeye bastýðýnda...
    public void Interact(HandInteractor interactor)
    {
        // Eðer cookingStation referansý atanmamýþsa bir þey yapma.
        if (cookingStation == null)
        {
            Debug.LogError("CookButton'a CookingStation atanmamýþ!");
            return;
        }

        // ...CookingStation'daki tarif doðrulama ve piþirme metodunu çaðýr.
        cookingStation.ValidateAndCook();
    }

    public void Release() { /* Gerekli Deðil */ }
}