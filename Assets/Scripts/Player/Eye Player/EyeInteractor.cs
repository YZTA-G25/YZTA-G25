using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.EditorTools;
#endif

public class EyeInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [Tooltip("Kolun hassasiyeti - Yatay fare hareketi ile kontrol edilir")]
    [SerializeField] private float leverSensitivity = 2f;

    [Header("Component References")]
    [Tooltip("Etkileşim ışınının başlayacağı nokta. Genellikle PF'in kamera objesi")]
    [SerializeField] private Transform raycastOrigin;

    private LeverController leverInRange;
    private LeverController controlledLever;

    private void Update()
    {
        // Safety check for input system
        if (Mouse.current == null) return;

        if (controlledLever == null)
        {
            FindLever();
        }
        else
        {
            float mouseXInput = Mouse.current.delta.x.ReadValue();
            controlledLever.UpdateRotation(mouseXInput * leverSensitivity * Time.deltaTime);
        }
    }

    private void FindLever()
    {
        if (raycastOrigin == null) return;

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            leverInRange = hit.collider.GetComponent<LeverController>();
            
            if (leverInRange != null)
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else
            {
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
            }
        }
        else
        {
            leverInRange = null;
            CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Normal);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        try
        {
            if (controlledLever != null)
            {
                controlledLever.Release();
                controlledLever = null;
                Debug.Log("Lever released.");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Interactable);
            }
            else if (leverInRange != null)
            {
                controlledLever = leverInRange;
                controlledLever.Grab();
                Debug.Log($"{controlledLever.GetControlDescription()} lever grabbed: {controlledLever.name}");
                CrosshairUI.Instance.SetCrosshairState(CrosshairUI.CrosshairState.Grabbed);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnInteract: " + e.Message);
        }
    }
}
