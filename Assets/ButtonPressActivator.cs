using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPressActivator : MonoBehaviour
{
    public GameObject button; // The button that moves when pressed
    public GameObject objectToActivate; // The object to activate
    public GameObject object2ToActivate;
    public GameObject object3ToActivate;
    public GameObject objectToDeactivate;
    public GameObject object2ToDeactivate;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    public Color pressedColor = Color.white; // Color to change to when pressed
    public Color defaultColor = Color.red;  // Default color of the button
    public Color activeColor = Color.green; // Color when objectToActivate is active

    private Renderer buttonRenderer; // Renderer of the button

    private GameObject presser;
    //private AudioSource sound;
    private bool isPressed;

    void Start()
    {
        //sound = GetComponent<AudioSource>();
        isPressed = false;

        // Ensure the objectToActivate is initially inactive
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
        // Get the Renderer of the button and set its initial color
        if (button != null)
        {
            buttonRenderer = button.GetComponent<Renderer>();
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = defaultColor;
            }
        }
    }

    void Update()
    {
        // Change button color to activeColor if objectToActivate is active
        if (objectToActivate != null && objectToActivate.activeSelf)
        {
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = activeColor;
            }
        }
        else if (!isPressed) // Reset to defaultColor only if button is not pressed
        {
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = defaultColor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.003f, 0); // Simulate button press
            presser = other.gameObject;

            // Change the button's color to pressedColor
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = pressedColor;
            }

            onPress.Invoke();
            //sound?.Play();
            isPressed = true;

            // Activate the object
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
                Debug.Log($"{objectToActivate.name} activated!");
            }
            if (TutorialManager.Instance.isTutorialActive)
            {
                object2ToActivate.SetActive(true);
                object3ToActivate.SetActive(true);
                objectToDeactivate.SetActive(false);
                object2ToDeactivate.SetActive(false);
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
            button.transform.localPosition = new Vector3(0, 0.015f, 0); // Reset button position

            // Reset the button's color to defaultColor
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = defaultColor;
            }
            
            onRelease.Invoke();
            isPressed = false;
        }
    }
}
