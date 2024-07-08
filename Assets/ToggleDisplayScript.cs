using UnityEngine;
using UnityEngine.UI;

public class ToggleDisplayScript : MonoBehaviour
{
    public RawImage displayImageTV;
    public RawImage displayImageLeftMonitor;
    public RawImage displayImageRightMonitor;
    public Button toggleButton;

    private bool isDisplaying = false; // To track the display state

    void Start()
    {
        toggleButton.onClick.AddListener(ToggleDisplay); // Add listener to the button
        displayImageTV.enabled = false;
        displayImageLeftMonitor.enabled = false;
        displayImageRightMonitor.enabled = false;
        // Initially disable the RawImage
    }

    void ToggleDisplay()
    {
        isDisplaying = !isDisplaying; // Toggle the display state
        displayImageTV.enabled = isDisplaying;
        displayImageLeftMonitor.enabled = isDisplaying;
        displayImageRightMonitor.enabled = isDisplaying; // Enable or disable the RawImage based on the state
    }
}