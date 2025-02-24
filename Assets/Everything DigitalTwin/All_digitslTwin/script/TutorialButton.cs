using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutorialButton : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    public GameObject object1ToEnable;
    public GameObject object2ToEnable;
    public GameObject object3ToEnable;
    public GameObject object4ToEnable;
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

            if (!TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.ActivateTutorial();
            }
            else
            {
                TutorialManager.Instance.DeactivateTutorial();
            }

        }
        if (TutorialManager.Instance.isTutorialActive)
        {
            if (object1ToEnable != null)
            {
                object1ToEnable.SetActive(true);
            }
            if (object2ToEnable != null)
            {
                object2ToEnable.SetActive(true);
            }
            if (object3ToEnable != null)
            {
                object3ToEnable.SetActive(true);
            }
            if (object4ToEnable != null)
            {
                object4ToEnable.SetActive(true);
            }
        }
        if (!TutorialManager.Instance.isTutorialActive)
        {
            if (object1ToEnable != null)
            {
                object1ToEnable.SetActive(false);
            }
            if (object2ToEnable != null)
            {
                object2ToEnable.SetActive(false);
            }
            if (object3ToEnable != null)
            {
                object3ToEnable.SetActive(false);
            }
            if (object4ToEnable != null)
            {
                object4ToEnable.SetActive(false);
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
