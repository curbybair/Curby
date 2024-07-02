using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateTeleportationRay : MonoBehaviour
{
    public GameObject leftTeleportaion;
    public GameObject rightTeleportaion;

    public InputActionProperty leftActivate;
    public InputActionProperty rightActivate;

    public InputActionProperty leftDeactivate;
    public InputActionProperty rightDeactivate;



    // Update is called once per frame
    void Update()
    {
        leftTeleportaion.SetActive(leftDeactivate.action.ReadValue<float>() == 0 && leftActivate.action.ReadValue<float>() > 0.1f);
        rightTeleportaion.SetActive(rightDeactivate.action.ReadValue<float>() == 0 && rightActivate.action.ReadValue<float>() > 0.1f);
    }
}
