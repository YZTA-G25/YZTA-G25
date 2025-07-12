using UnityEngine;

public class NotebookController : MonoBehaviour
{

    [SerializeField] private GameObject[] pages;
    private int currentPageIndex = 0;

    void Start()
    {
        ShowPage(0);
    }

    public void ShowPage(int pageIndex)
    {
        currentPageIndex = Mathf.Clamp(pageIndex, 0, pages.Length - 1);
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
    }

    public void TurnToNextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    public void TurnToPreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }
}