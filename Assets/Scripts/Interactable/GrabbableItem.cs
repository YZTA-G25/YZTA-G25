// Scripts/Interaction/GrabbableItem.cs
using UnityEngine;

// Bu da Interactable s�n�f�ndan miras al�yor.
public class GrabbableItem : Interactable
{
    public override void Interact(PlayerInteractor interactor)
    {
        Debug.Log($"Yerden al�nd�: {gameObject.name}");
        // Bu objenin kendisini oyuncunun eline tutmas� i�in yolla.
        interactor.HoldItem(this.gameObject);
    }
}