// Interactable.cs
using UnityEngine;

public interface IInteractable
{
    // Parametreyi PlayerInteractor'dan HandInteractor'a �eviriyoruz.
    public void Interact(HandInteractor interact);
}