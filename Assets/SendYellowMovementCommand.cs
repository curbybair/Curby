using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class SendYellowMovementCommand : MonoBehaviour
{
    public Button buttonPosXYel;
    public Button buttonNegXYel;
    public Button buttonPosYYel;
    public Button buttonNegYYel;
    public Button buttonPosZYel;
    public Button buttonNegZYel;
    public Button homeButtonYel;
    public Button cancelButtonYel;
    public TMP_Text statusTextYel;
    public TMP_Text xyProgressTextYel;
    public TMP_Text zProgressTextYel;
    public TMP_Text confirmationTextYel;

    private HttpClient client;
    private string commandUrl = "http://192.168.1.105/api/printer/command";
    private string apiKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";

    private float currentX = 0f;
    private float currentY = 0f;
    private float currentZ = 0f;
    private bool canSendCommand = true;
    private bool confirmCancel = false;
    private Coroutine cancelCoroutine;

    void Start()
    {
        buttonPosXYel = GameObject.Find("Right Yellow").GetComponent<Button>();
        buttonNegXYel = GameObject.Find("Left Yellow").GetComponent<Button>();
        buttonPosYYel = GameObject.Find("Up Yellow").GetComponent<Button>();
        buttonNegYYel = GameObject.Find("Down Yellow").GetComponent<Button>();
        buttonPosZYel = GameObject.Find("Up Z Yellow").GetComponent<Button>();
        buttonNegZYel = GameObject.Find("Down Z Yellow").GetComponent<Button>();
        homeButtonYel = GameObject.Find("Home Yellow").GetComponent<Button>();
        //cancelButtonYel = GameObject.Find("").GetComponent<Button>();
        statusTextYel = GameObject.Find("Status text Yellow").GetComponent<TMP_Text>();
        xyProgressTextYel = GameObject.Find("X/Y Text Yellow").GetComponent<TMP_Text>();
        zProgressTextYel = GameObject.Find("Z Text Yellow").GetComponent<TMP_Text>();
        //confirmationTextYel = GameObject.Find("").GetComponent<TMP_Text>();

        client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        buttonPosXYel.onClick.AddListener(() => MoveAxis("G0 X10", "X", 10));
        buttonNegXYel.onClick.AddListener(() => MoveAxis("G0 X-10", "X", -10));
        buttonPosYYel.onClick.AddListener(() => MoveAxis("G0 Y10", "Y", 10));
        buttonNegYYel.onClick.AddListener(() => MoveAxis("G0 Y-10", "Y", -10));
        buttonPosZYel.onClick.AddListener(() => MoveAxis("G0 Z10", "Z", 10));
        buttonNegZYel.onClick.AddListener(() => MoveAxis("G0 Z-10", "Z", -10));
        homeButtonYel.onClick.AddListener(HomeAllAxes);
        cancelButtonYel.onClick.AddListener(CancelOrConfirm);
    }

    async void MoveAxis(string gCodeCommand, string axis, float moveAmount)
    {
        Debug.Log($"Attempting to send command: {gCodeCommand}");
        Debug.Log($"Axis: {axis}, Move Amount: {moveAmount}");

        if (!canSendCommand)
        {
            statusTextYel.text = "Please wait for the cooldown period.";
            return;
        }

        canSendCommand = false;

        var commandPayload = new CommandPayload { command = gCodeCommand };
        string jsonPayload = JsonUtility.ToJson(commandPayload);
        Debug.Log($"JSON Payload: {jsonPayload}");

        HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = null;

        try
        {
            response = await client.PostAsync(commandUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.Log($"Command {gCodeCommand} sent successfully.");
                if (axis == "X")
                {
                    currentX += moveAmount;
                    xyProgressTextYel.text = $"X Axis Position: {currentX}";
                }
                else if (axis == "Y")
                {
                    currentY += moveAmount;
                    xyProgressTextYel.text = $"Y Axis Position: {currentY}";
                }
                else if (axis == "Z")
                {
                    currentZ += moveAmount;
                    zProgressTextYel.text = $"Z Axis Position: {currentZ}";
                }

                statusTextYel.text = $"Command {gCodeCommand} sent successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to send command {gCodeCommand}. Response: {response.StatusCode}, {errorContent}");
                statusTextYel.text = $"Failed to send command {gCodeCommand}. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusTextYel.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        canSendCommand = true;
        statusTextYel.text = "Ready for next command.";
    }

    async void HomeAllAxes()
    {
        if (!canSendCommand)
        {
            statusTextYel.text = "Please wait for the cooldown period.";
            return;
        }

        canSendCommand = false;

        var commandPayload = new CommandPayload { command = "G28" };
        string jsonPayload = JsonUtility.ToJson(commandPayload);
        Debug.Log($"Sending home command: G28");

        HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = null;

        try
        {
            response = await client.PostAsync(commandUrl, content);

            if (response.IsSuccessStatusCode)
            {
                currentX = 0f;
                currentY = 0f;
                currentZ = 0f;
                xyProgressTextYel.text = "X/Y Axis Position: 0";
                zProgressTextYel.text = "Z Axis Position: 0";
                statusTextYel.text = "Home command sent successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to send home command. Response: {response.StatusCode}, {errorContent}");
                statusTextYel.text = $"Failed to send home command. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusTextYel.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        canSendCommand = true;
        statusTextYel.text = "Ready for next command.";
    }

    void CancelOrConfirm()
    {
        if (confirmCancel)
        {
            CancelObject();
            confirmCancel = false;
            if (cancelCoroutine != null)
            {
                StopCoroutine(cancelCoroutine);
                confirmationTextYel.text = ""; // Clear the confirmation text
            }
        }
        else
        {
            confirmationTextYel.text = "Are you sure you want to cancel the print? Press again to confirm.";
            confirmCancel = true;
            cancelCoroutine = StartCoroutine(ResetCancelConfirmation());
        }
    }

    async void CancelObject()
    {
        string gCodeCommand = "M486 C";  // Assuming this command cancels the current print
        var commandPayload = new CommandPayload { command = gCodeCommand };
        string jsonPayload = JsonUtility.ToJson(commandPayload);
        Debug.Log($"Sending cancel command: M486 C");

        HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = null;

        try
        {
            response = await client.PostAsync(commandUrl, content);

            if (response.IsSuccessStatusCode)
            {
                confirmationTextYel.text = "";
                statusTextYel.text = $"Print cancelled successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                statusTextYel.text = $"Failed to cancel print. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusTextYel.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        statusTextYel.text = "Ready for next command.";
    }

    IEnumerator ResetCancelConfirmation()
    {
        yield return new WaitForSeconds(5);
        confirmCancel = false;
        confirmationTextYel.text = ""; // Clear the confirmation text after 5 seconds
    }

    async Task Cooldown(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            statusTextYel.text = $"Cooldown: {i} seconds";
            await Task.Delay(1000);
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Dispose();
        }
    }
}

[System.Serializable]
public class yellowCommandPayload
{
    public string command;
}

