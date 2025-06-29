// GrabbableItem.cs
using UnityEngine;

public class GrabbableItem : Interactable
{
    // Parametreyi PlayerInteractor'dan HandInteractor'a çeviriyoruz.
    public override void Interact(HandInteractor interactor)
    {
        Debug.Log($"Yerden alýndý: {gameObject.name}");
        interactor.HoldItem(this.gameObject);
    }
}