// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;

// public class TeleportationManager : MonoBehaviour
// {
//     [SerializeField] private InputActionAsset actionAsset;
//     private InputAction _thumbstick;


//     void Start()
//     {
//         var activate = actionAsset.FindActionMap("XRI LeftHand").FindAction("Teleport Mode Activate");
//         activate.Enable();

//         var cancel = actionAsset.FindActionMap("XRI LeftHand").FindAction("Teleport Mode Cancel");

//         _thumbstick = actionAsset.FindActionMap("XRI LeftHand").FindAction("Move");
//         _thumbstick.Enable();
//     }

//     void Update()
//     {

//     }

//     private void OnTeleportActivate()
//     {

//     }

//     private void OnTeleportCancel()
//     {

//     }
// }

