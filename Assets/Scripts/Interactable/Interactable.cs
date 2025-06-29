// Interactable.cs
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string interactionPrompt = "Etkile�ime Gir";

    // Parametreyi PlayerInteractor'dan HandInteractor'a �eviriyoruz.
    public abstract void Interact(HandInteractor interactor);
}