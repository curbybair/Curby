using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ClickLog : MonoBehaviour
{
    [Header("Interaction Tracking")]
    public int interactionCount = 0; // Tracks button clicks or collider presses
    public Dropdown dropdown; // Reference to the Dropdown component (optional)
    public bool isColliderButton = false; // Is this object a collider-based button?

    [Header("Optional: Display Counter")]
    public Text displayText; // UI Text to display the counter value

    private static string logFilePath; // Path to the shared CSV file

    void Awake()
    {
        // Set the path for the CSV file in the Downloads folder
        string downloadsPath = GetDownloadsPath();
        logFilePath = Path.Combine(downloadsPath, "InteractionLog.csv");

        // Initialize the file with headers if it doesn't exist
        if (!new FileInfo(logFilePath).Exists)
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, false)) // Overwrite file
            {
                writer.WriteLine("ObjectName,Action,Value,Time");
            }
        }
    }

    void Start()
    {
        // Ensure the counter starts at 0 for buttons or colliders
        interactionCount = 0;

        // Update the display text if assigned
        UpdateDisplayText();

        // Add a listener to the dropdown if it exists
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(LogDropdownInteraction);
        }
    }

    // Method to record button interactions
    public void RecordButtonInteraction()
    {
        interactionCount++;
        UpdateDisplayText();

        // Log the button interaction
        LogToCSV(gameObject.name, "ButtonClick", interactionCount.ToString());

        Debug.Log($"Button {gameObject.name} clicked {interactionCount} times.");
    }

    // Method to log dropdown interactions
    private void LogDropdownInteraction(int selectedIndex)
    {
        string selectedOption = dropdown.options[selectedIndex].text;

        // Log the dropdown interaction
        LogToCSV(gameObject.name, "DropdownSelect", selectedOption);

        Debug.Log($"Dropdown {gameObject.name} selected option: {selectedOption}");
    }

    // Method to log collider button interactions
    private void OnCollisionEnter(Collision collision)
    {
        if (isColliderButton)
        {
            interactionCount++;

            // Log the collider interaction
            LogToCSV(gameObject.name, "ColliderClick", interactionCount.ToString());

            Debug.Log($"Collider button {gameObject.name} interacted with by {collision.gameObject.name}.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isColliderButton)
        {
            interactionCount++;
            LogToCSV(gameObject.name, "TriggerClick", interactionCount.ToString());
            Debug.Log($"Trigger button {gameObject.name} interacted with by {other.gameObject.name}.");
        }
    }


    // Updates the display text (if any) to show the current counter value
    private void UpdateDisplayText()
    {
        if (displayText != null)
        {
            displayText.text = $"Interactions: {interactionCount}";
        }
    }

    // Logs the interaction to the shared CSV file
    private void LogToCSV(string objectName, string action, string value)
    {
        string logEntry = $"{objectName},{action},{value},{System.DateTime.Now}";
        using (StreamWriter writer = new StreamWriter(logFilePath, true)) // Append to file
        {
            writer.WriteLine(logEntry);
        }
    }

    // Utility function to get the Downloads folder path
    private string GetDownloadsPath()
    {
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
               .Replace("Documents", "Downloads");
    }

    void OnDestroy()
    {
        // Remove the listener for dropdowns to avoid memory leaks
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(LogDropdownInteraction);
        }
    }
}
