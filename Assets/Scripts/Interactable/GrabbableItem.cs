// GrabbableItem.cs
using UnityEngine;

public class GrabbableItem : MonoBehaviour, IInteractable
{
    public bool inFood = false;

    public void Interact(HandInteractor interactor)
    {
        Debug.Log($"Yerden alındı: {gameObject.name}");
        interactor.HoldItem(this.gameObject);
    }
}