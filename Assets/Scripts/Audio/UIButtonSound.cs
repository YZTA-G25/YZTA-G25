using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Types")]
    [SerializeField] private SoundType hoverSound = SoundType.MENU_HOVER;
    [SerializeField] private SoundType clickSound = SoundType.MENU_CLICK;

    [Header("Volume Levels")]
    [Tooltip("Üzerine gelme sesinin seviyesi (0.0 ile 1.0 arasýnda)")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.01f; // Sesi varsayýlan olarak %50'ye ayarladýk

    [Tooltip("Týklama sesinin seviyesi (0.0 ile 1.0 arasýnda)")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1.0f; // Týklama sesi tam seste kalsýn

    // Fare butonun üzerine geldiðinde...
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ...SoundManager'a hover sesini ve istediðimiz ses seviyesini gönderiyoruz.
        SoundManager.PlaySound(hoverSound, hoverVolume);
    }

    // Butona týklandýðýnda...
    public void OnPointerClick(PointerEventData eventData)
    {
        // ...SoundManager'a týklama sesini ve istediðimiz ses seviyesini gönderiyoruz.
        SoundManager.PlaySound(clickSound, clickVolume);
    }
    
}