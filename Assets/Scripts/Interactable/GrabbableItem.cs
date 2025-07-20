// GrabbableItem.cs
using UnityEngine;

public class GrabbableItem : Interactable
{
    public bool inFood = false;

    public override void Interact(HandInteractor interactor)
    {
        Debug.Log($"Yerden alındı: {gameObject.name}");
        interactor.HoldItem(this.gameObject);
    }
}