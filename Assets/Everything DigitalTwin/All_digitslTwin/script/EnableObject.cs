using System.Collections;
using UnityEngine;

public class EnableObject : MonoBehaviour
{
    // The GameObject to enable after the delay
    public GameObject objectToEnable;
    // The GameObject to disable after the delay
    public GameObject objectToDisable;
    // Arrow to disable because of Tutorial
    public GameObject object2ToDisable;
    // Arrow to enable because of Tutorial
    public GameObject object2ToEnable;

    // Reference to the TaskBlockEnable to call manual override
    public TaskBlockEnable taskBlockEnableScript;

    // Time delay in seconds
    private float delay = 6f;

    // Function to start the delay and the override
    public void OnButtonPress()
    {
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Disable the specified GameObject (arrow)
            if (object2ToDisable != null)
            {
                object2ToDisable.SetActive(false);
            }
            
            // Start the coroutine to enable the object after a delay
            StartCoroutine(EnableObjectAfterDelay());

            // Call manual disable from TaskBlockEnable (optional)
            if (taskBlockEnableScript != null)
            {
                taskBlockEnableScript.ManualDisable(); // This will disable the task blocks
            }
        }
    }

    // Coroutine to handle the delay
    private IEnumerator EnableObjectAfterDelay()
    {
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(delay);
            
            // Enable the specified GameObject (arrow)
            if (object2ToEnable != null)
            {
                object2ToEnable.SetActive(true);
            }
            // Enable the GameObject
            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            // Disable the specified GameObject
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
        }

        
    }
}
