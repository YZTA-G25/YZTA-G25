using System.Net.Mime;

using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[RequireComponent(typeof(CharacterController))]
public class EyePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    private CharacterController characterController;
    private Vector2 moveInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        characterController.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);
    }
}
