// GrabbableItem.cs
using UnityEngine;

public class GrabbableItem : Interactable
{
    // Parametreyi PlayerInteractor'dan HandInteractor'a �eviriyoruz.
    public override void Interact(HandInteractor interactor)
    {
        Debug.Log($"Yerden al�nd�: {gameObject.name}");
        interactor.HoldItem(this.gameObject);
    }
}