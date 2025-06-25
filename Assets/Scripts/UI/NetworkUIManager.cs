using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    private void Awake()
    {
        //Butonlara tıkladığında hangi fonksiyonların çalışacağını belirliyoruz.
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            HideButtons();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            HideButtons();
        });
    }

    //Oyuncular bağlandıktan sonra butonları gizle
    private void HideButtons()
    {
        gameObject.SetActive(false);
    }
}
