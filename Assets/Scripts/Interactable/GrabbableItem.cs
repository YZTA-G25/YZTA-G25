// Scripts/Interaction/GrabbableItem.cs
using UnityEngine;

// Bu da Interactable sýnýfýndan miras alýyor.
public class GrabbableItem : Interactable
{
    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log($"Yerden alýndý: {gameObject.name}");
        // Bu objenin kendisini oyuncunun eline tutmasý için yolla.
        interactor.HoldItem(this.gameObject);
    }
}