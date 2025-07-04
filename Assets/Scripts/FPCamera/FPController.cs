using Unity.Cinemachine;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    [Header("Movement Detail")]
    public float MaxSpeed = 5f;
    public float Acceleration = 15f;

    public Vector3 CurrentVelocity {  get; private set; }
    public float CurrentSpeed { get; private set; }

    [Header("Looking Input")]
    public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);

    public float PitchLimit = 85f;

    [SerializeField] float currentPitch = 0f;

    public float CurrentPitch
    {
        get => currentPitch;

        set
        {
            currentPitch = Mathf.Clamp(value, -PitchLimit, PitchLimit); 
        }
    }

    [Header("Input")]
    public Vector2 MoveInput;
    public Vector2 LookInput;

    [Header("Components")]
    [SerializeField] CinemachineCamera fpCamera;
    [SerializeField] CharacterController characterController;


    private void OnValidate()
    {
        if(characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        MoveUpdate();
        LookUpdate();
    }


    void MoveUpdate()
    {
        Vector3 motion = transform.forward * MoveInput.y + transform.right * MoveInput.x;
        motion.y = 0f;
        motion.Normalize();

        if(motion.sqrMagnitude > 0.01f)
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, motion * MaxSpeed, Acceleration * Time.deltaTime);
        }
        else
        {
            CurrentVelocity = Vector3.MoveTowards(CurrentVelocity,Vector3.zero,Acceleration * Time.deltaTime);
        }

        float verticalVelocity = Physics.gravity.y * 20f * Time.deltaTime;

        Vector3 fullVelocity = new Vector3(CurrentVelocity.x ,verticalVelocity,CurrentVelocity.z);  

        characterController.Move(fullVelocity * Time.deltaTime);

        //update speed
        CurrentSpeed = CurrentVelocity.magnitude;
    }

    void LookUpdate()
    {
        Vector2 input = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);

        // yukar� a�a�� bakma
        CurrentPitch -= input.y;

        fpCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);

        //sa�a sola bakma
        transform.Rotate(Vector3.up * input.x);
    }

    // FPController.cs i�ine eklenecek metotlar

    public void SetMoveInput(Vector2 input)
    {
        MoveInput = input;
    }

    public void SetLookInput(Vector2 input)
    {
        LookInput = input;
    }

}
