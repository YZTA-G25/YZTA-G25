using UnityEngine;

public class PageTurnButton : MonoBehaviour, IInteractable
{
    [SerializeField] private NotebookController notebookController;
    [SerializeField] private bool isNextButton;

    public void Interact(HandInteractor interactor)
    {
        if (notebookController == null) return;

        if (isNextButton)
        {
            notebookController.TurnToNextPage();
        }
        else
        {
            notebookController.TurnToPreviousPage();
        }
    }

    public void Release()
    {
        // PageTurnButton doesn't need any release functionality
        // This is only for grabbable objects
    }
}
