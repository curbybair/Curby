using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FanButton : MonoBehaviour
{
    public GameObject button;
    public GameObject object1ToDeactivate;
    public GameObject object1ToActivate;
    public GameObject object2ToDeactivate;
    public GameObject object2ToActivate;
    public UnityEvent onPress;
    public UnityEvent onRelease;
    public FanMove fan; // Reference to the FanMove script

    private Renderer buttonRenderer;
    GameObject presser;
    //AudioSource sound;
    bool isPressed;

    // Define the colors for active and inactive states
    private Color activeColor = Color.green;
    private Color inactiveColor = Color.red;

    void Start()
    {
        //sound = GetComponent<AudioSource>();
        buttonRenderer = button.GetComponent<Renderer>();
        isPressed = false;

        // Set the initial button color to inactive (red)
        buttonRenderer.material.color = inactiveColor;
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.003f, 0);
            presser = other.gameObject;
            onPress.Invoke();
            //sound.Play();
            isPressed = true;

            if (TutorialManager.Instance.isTutorialActive)
            {
                object1ToDeactivate.SetActive(false);
                object2ToDeactivate.SetActive(false);
                object1ToActivate.SetActive(true);
                object2ToActivate.SetActive(true);
            }
            else
            {
                Debug.Log("Tutorial is not active. Interaction blocked.");
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            button.transform.localPosition = new Vector3(0, 0.015f, 0);
            onRelease.Invoke();
            isPressed = false;

            // Toggle the fan movement on buttomn release
            fan.ToggleMovement();

            // Toggle the button color
            ToggleButtonColor();


        }
    }

    private void ToggleButtonColor()
    {
        // Check the current color and toggle to the other state
        if (buttonRenderer.material.color == activeColor)
        {
            buttonRenderer.material.color = inactiveColor;
        }
        else
        {
            buttonRenderer.material.color = activeColor;
        }
    }
}
