using UnityEngine;
using Valve.VR;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 2.0f; // Speed of player movement

    public SteamVR_Action_Vector2 joystickInput; // SteamVR Action for joystick movement
    public SteamVR_Input_Sources handType = SteamVR_Input_Sources.Any; // Choose the controller type

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (!characterController)
        {
            characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    void Update()
    {
        // Get the input from the joystick
        Vector2 input = joystickInput.GetAxis(handType);

        // Convert the joystick input to movement in the forward direction of the headset
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0; // Prevent vertical movement
        Vector3 right = Camera.main.transform.right;

        // Calculate the desired movement direction
        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;

        // Apply movement to the player
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}