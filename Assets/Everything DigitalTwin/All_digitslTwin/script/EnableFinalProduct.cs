using System.Collections;
using UnityEngine;

public class EnableFinalProduct : MonoBehaviour
{
    // The GameObjects to enable after the delay
    public GameObject objectToEnable1;
    public GameObject objectToEnable2;

    // The GameObject to disable after the delay
    public GameObject objectToDisable1;
    public GameObject objectToDisable2;

    // Arrow to disable because of Tutorial
    public GameObject objectToDisable3;
    // Arrow to enable because of Tutorial
    public GameObject objectToEnable3;
    //Activate Yellow Pop-up Bubble
    public GameObject objectToEnable4;


    // Time delay in seconds
    private float delay = 6f;

    // Function to start the delay
    public void OnButtonPress()
    {
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Disable the specified GameObject (arrow)
            if (objectToDisable3 != null)
            {
                objectToDisable3.SetActive(false);
            }

            // Start the coroutine to handle enabling and disabling after a delay
            StartCoroutine(EnableDisableAfterDelay());
        }
    }

    // Coroutine to handle the delay
    private IEnumerator EnableDisableAfterDelay()
    {
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(delay);

            // Enable the first specified GameObject
            if (objectToEnable1 != null)
            {
                objectToEnable1.SetActive(true);
            }

            // Enable the second specified GameObject
            if (objectToEnable2 != null)
            {
                objectToEnable2.SetActive(true);
            }

            // Disable the specified GameObject
            if (objectToDisable1 != null)
            {
                objectToDisable1.SetActive(false);
            }

            // Disable the specified GameObject
            if (objectToDisable2 != null)
            {
                objectToDisable2.SetActive(false);
            }

            
            // Enable the specified GameObject (arrow)
            if (objectToEnable3 != null)
            {
                objectToEnable3.SetActive(true);
            }

            // Enable the specified GameObject (Bubble)
            if (objectToEnable4 != null)
            {
                objectToEnable4.SetActive(true);
            }
        }
    }
}

