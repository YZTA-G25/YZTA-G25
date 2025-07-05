using UnityEngine;

public class PageTurnButton : Interactable
{
    [SerializeField] private NotebookController notebookController;
    [SerializeField] private bool isNextButton;

    public override void Interact(HandInteractor interactor)
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
}
