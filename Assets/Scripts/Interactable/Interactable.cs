// Scripts/Interaction/Interactable.cs
using UnityEngine;

// Bu script tek ba��na kullan�lmaz, di�erleri bunu miras al�r.
public abstract class Interactable : MonoBehaviour
{
    // Oyuncu bakt���nda g�sterilecek ipucu metni.
    public string interactionPrompt = "Etkile�ime Gir";

    // T�m miras alanlar�n sahip olmak zorunda oldu�u metot.
    // Etkile�ime giren oyuncunun elini referans olarak al�r.
    public abstract void Interact(PlayerInteractor interactor);
}