using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PowerOnYellow : MonoBehaviour
{
    public GameObject rockerSwitch;
    public GameObject lcdScreen; // Reference to the LCD screen GameObject
    public GameObject controllerBoard;
    public Material onMaterial;
    public Material offMaterial;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    GameObject presser;
    bool isPressed;
    bool printerOn = true;
    string apiUrl = "http://192.168.1.102/api/printer/command"; 
    string apiKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";

    HttpClient client;
    Renderer rockerRenderer;

    void Start()
    {
        isPressed = false;

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        rockerRenderer = rockerSwitch.GetComponent<Renderer>();
        UpdateRockerState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            // Rotate the rocker switch to simulate pressing
            rockerSwitch.transform.localRotation = Quaternion.Euler(0, 104.948f, 0);
            presser = other.gameObject;
            onPress.Invoke();
            isPressed = true;

            // Send G-code command to toggle the printer state
            if (printerOn)
            {
                SendGCodeCommand("M81");
                printerOn = false;
                lcdScreen.SetActive(false);
                // Turn off the LCD screen
                controllerBoard.SetActive(false);
            }
            else
            {
                SendGCodeCommand("M80");
                printerOn = true;
                lcdScreen.SetActive(true); // Turn on the LCD screen
                controllerBoard.SetActive(true);
            }

            UpdateRockerState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            // Rotate the rocker switch back to the original position
            rockerSwitch.transform.localRotation = Quaternion.Euler(0, 117.613f, 0);
            onRelease.Invoke();
            isPressed = false;

            UpdateRockerState();
        }
    }

    async void SendGCodeCommand(string command)
    {
        var commandPayload = new CommandPayload { command = command };
        string jsonPayload = JsonUtility.ToJson(commandPayload);
        HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log($"Command {command} sent successfully.");
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to send command {command}. Response: {response.StatusCode}, {errorContent}");
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
        }
    }

    void UpdateRockerState()
    {
        // Update the material based on the printer state
        if (printerOn)
        {
            rockerRenderer.material = onMaterial;
        }
        else
        {
            rockerRenderer.material = offMaterial;
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Dispose();
        }
    }

    [System.Serializable]
    public class CommandPayload
    {
        public string command;
    }
}