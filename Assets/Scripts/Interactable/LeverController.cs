using UnityEngine;
using Unity.Netcode;
using UnityEditor.EditorTools;

public class LeverController : NetworkBehaviour
{
    [Header("Lever Settings")]
    [Tooltip("Kolun dönebileceği minimum açı.")]
    [SerializeField] private float minAngle = -45f;

    [Tooltip("Kolun dönebileceği maksimum açı.")]
    [SerializeField] private float maxAngle = 45f;

    [Tooltip("Dönülecek eksen (Genellikle X ekseni: 1,0,0).")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    private float currentAngle = 0f;
    private HeadController headController;

    public void Grab()
    {
        // Kolun tutulduğunda yapılacaklar
    }

    public void Release()
    {
        // Oyuncu kolu bıraktığında yapılacak bir şey varsa
    }

    // EyeInteractor'dan gelen mouse input'u ile kolun açısını güncelliyoruz
    public void UpdateRotation(float input)
    {
        // Mevcut açıyı güncelle ve limitler içinde kalmasını sağla
        currentAngle -= input; // Mouse genellikle ters çalışır o nedenle eksi
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Kolun kendi görselini döndür
        transform.localRotation = Quaternion.Euler(rotationAxis * currentAngle);

        // Ağ üzerinden kafa rotasyonunu güncelleme talebi gönder
        SendHeadRotationUpdate();
    }

    private void SendHeadRotationUpdate()
    {
        if (headController == null)
        {
            headController = FindFirstObjectByType<HeadController>();
        }

        if (headController != null)
        {
            headController.SetHeadRotationServerRpc(rotationAxis, currentAngle);
        }
    }
}
