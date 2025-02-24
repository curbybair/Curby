using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class octoPrintJobData
{
    public string state;
}

public class TaskBlockEnable : MonoBehaviour
{
    public string printer1APIKey = "5D44B0739CB04110871B24C202512986";
    public string printer2APIKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";
    public string printer1URL = "http://192.168.1.103/api/job";
    public string printer2URL = "http://192.168.1.105/api/job";

    public GameObject TaskBlockYellow;  // Block to activate for Printer 2
    public GameObject TaskBlockGrey;    // Block to activate for Printer 1

    private float nextActionTime = 0.0f;
    public float period = 2f;

    private bool manualOverride = false; // Flag to track if manual override is active

    void Update()
    {
        if (!manualOverride && Time.time > nextActionTime)
        {
            nextActionTime += period;
            StartCoroutine(CheckPrinterStatus(printer1URL, printer1APIKey, "Printer1"));
            StartCoroutine(CheckPrinterStatus(printer2URL, printer2APIKey, "Printer2"));
        }
    }

    IEnumerator CheckPrinterStatus(string url, string apiKey, string printerName)
    {
        if (manualOverride) yield break; // Skip if override is active

        string requestUrl = $"{url}?apikey={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Failed to fetch data from {printerName}: {request.error}");
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        try
        {
            OctoPrintJobData jobData = JsonUtility.FromJson<OctoPrintJobData>(jsonResponse);
            string state = jobData.state;

            if (printerName == "Printer1")
            {
                TaskBlockGrey.SetActive(state == "Operational");
                if (state == "Operational") Debug.Log("Printer 1 finished, TaskBlockGrey enabled.");
            }
            else if (printerName == "Printer2")
            {
                TaskBlockYellow.SetActive(state == "Operational");
                if (state == "Operational") Debug.Log("Printer 2 finished, TaskBlockYellow enabled.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse {printerName} data: {e.Message}");
        }
    }

    // Method to be called by the button to manually disable both blocks
    public void ManualDisable()
    {
        manualOverride = true; // Temporarily enable manual override

        // Disable both Task Blocks
        TaskBlockGrey.SetActive(false);
        TaskBlockYellow.SetActive(false);
        Debug.Log("Manual override activated, both Task Blocks disabled.");

        // Start coroutine to revert to automatic control after a delay
        StartCoroutine(RevertToAutomaticControl());
    }

    // Coroutine to turn off manual override after a delay
    private IEnumerator RevertToAutomaticControl()
    {
        yield return new WaitForSeconds(1000f); // Adjust delay as needed
        manualOverride = false; // Revert to print status checking
        Debug.Log("Manual override deactivated, reverting to print status control.");
    }
}
