// Scripts/Interaction/Interactable.cs
using UnityEngine;

// Bu script tek baþýna kullanýlmaz, diðerleri bunu miras alýr.
public abstract class Interactable : MonoBehaviour
{
    // Oyuncu baktýðýnda gösterilecek ipucu metni.
    public string interactionPrompt = "Etkileþime Gir";

    // Tüm miras alanlarýn sahip olmak zorunda olduðu metot.
    // Etkileþime giren oyuncunun elini referans olarak alýr.
    public abstract void Interact(PlayerInteractor interactor);
}