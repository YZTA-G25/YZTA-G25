using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


[RequireComponent(typeof(FPController))]
public class FPCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FPController FPController;


    void OnLook(InputValue value)
    {
        FPController.LookInput = value.Get<Vector2>();
    }

    void OnValidate()
    {
        if(FPController == null)
            FPController = GetComponent<FPController>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
