// Interactable.cs
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public string interactionPrompt = "Etkileþime Gir";

    // Parametreyi PlayerInteractor'dan HandInteractor'a çeviriyoruz.
    public abstract void Interact(HandInteractor interactor);
}