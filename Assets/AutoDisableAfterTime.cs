using UnityEngine;

public class AutoDisableAfterTime : MonoBehaviour
{
    // Duration before disabling the object
    public float disableAfterSeconds = 3f;

    void OnEnable()
    {
        // Start the countdown to disable the object
        Invoke(nameof(DisableObject), disableAfterSeconds);
    }

    void OnDisable()
    {
        // Cancel any pending invoke when the object is disabled
        CancelInvoke(nameof(DisableObject));
    }

    void DisableObject()
    {
        // Disable the GameObject
        gameObject.SetActive(false);
    }
}
