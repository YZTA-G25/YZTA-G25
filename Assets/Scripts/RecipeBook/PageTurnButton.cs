using UnityEngine;

public class PageTurnButton : Interactable
{
    [SerializeField] private NotebookController notebookController;
    [SerializeField] private bool isNextButton;

    public override void Interact(HandInteractor interactor)
    {
        Debug.Log("Buton, deftere sayfa çevirme komutu gönderiyor.");
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
