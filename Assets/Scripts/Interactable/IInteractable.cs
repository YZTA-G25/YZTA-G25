// Interactable.cs
using UnityEngine;

public interface IInteractable
{
    // Parametreyi PlayerInteractor'dan HandInteractor'a �eviriyoruz.
    public void Interact(HandInteractor interact);
    public void Grab(HandInteractor interactor); // New method for hold-to-grab
    public void Release();
}