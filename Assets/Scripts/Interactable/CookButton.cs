using UnityEngine;

// Bu düğme de IInteractable olmalı ki oyuncu etkileşime girebilsin.
public class CookButton : MonoBehaviour, IInteractable
{
    [Tooltip("Bu düğmenin bağlı olduğu CookingStation.")]
    [SerializeField] private CookingStation cookingStation;

    public void Grab(HandInteractor interactor)
    {
        Interact(interactor);
    }

    // Oyuncu bu düğmeye bastığında...
    public void Interact(HandInteractor interactor)
    {
        // Eğer cookingStation referansı atanmamışsa bir şey yapma.
        if (cookingStation == null)
        {
            Debug.LogError("CookButton'a CookingStation atanmamış!");
            return;
        }

        // ...CookingStation'daki tarif doğrulama ve pişirme metodunu çağır.
        cookingStation.ValidateAndCook();
    }

    public void Release() { }
}