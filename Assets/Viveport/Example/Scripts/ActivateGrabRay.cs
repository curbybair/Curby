using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateGrabRay : MonoBehaviour
{
    public GameObject leftGrab;
    public GameObject rightGrab;

    public InputActionProperty leftActivate;
    public InputActionProperty rightActivate;

    public InputActionProperty leftDeactivate;
    public InputActionProperty rightDeactivate;



    // Update is called once per frame
    void Update()
    {
        leftGrab.SetActive(leftDeactivate.action.ReadValue<float>() == 0 && leftActivate.action.ReadValue<float>() > 0.1f);
        rightGrab.SetActive(rightDeactivate.action.ReadValue<float>() == 0 && rightActivate.action.ReadValue<float>() > 0.1f);
    }
}