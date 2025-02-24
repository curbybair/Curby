using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonPressDeactivator : MonoBehaviour
{
    public GameObject button; // The button that moves when pressed
    public GameObject objectToDeactivate; // The object to activate
    public UnityEvent onPress;
    public UnityEvent onRelease;
    
    public Color pressedColor = Color.white; // Color to change to when pressed
    public Color defaultColor = Color.red;  // Default color of the button
    public Color deactivatedColor = Color.green; // Color when objectToDeactivate is deactivated

    private Renderer buttonRenderer;
    

    private GameObject presser;
    //private AudioSource sound;
    private bool isPressed;

    void Start()
    {
        //sound = GetComponent<AudioSource>();
        isPressed = false;

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
        // Change the button color to green if the objectToDeactivate is inactive
        if (objectToDeactivate != null && !objectToDeactivate.activeSelf)
        {
            if (buttonRenderer != null)
            {
                buttonRenderer.material.color = deactivatedColor;
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
            if (objectToDeactivate != null)
            {
                objectToDeactivate.SetActive(false);
                Debug.Log($"{objectToDeactivate.name} deactivated!");
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
