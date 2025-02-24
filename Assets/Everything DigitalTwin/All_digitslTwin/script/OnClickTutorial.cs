using UnityEngine;
using UnityEngine.Events;

public class OnClickTutorial : MonoBehaviour
{
    public UnityEvent alwaysActiveEvents;   // Events that should always run 
    public UnityEvent conditionalEvents;   // Events that only run when tutorial is active

    public void OnButtonClicked()
    {
        // Always execute these events
        alwaysActiveEvents.Invoke();
        Debug.Log("Always-active events executed.");

        // Only execute these if the tutorial is active
        if (TutorialManager.Instance.isTutorialActive)
        {
            conditionalEvents.Invoke();
            Debug.Log("Tutorial is active. Conditional events executed.");
        }
        else
        {
            Debug.Log("Tutorial is not active. Conditional events blocked.");
        }
    }
}
