using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class SendMovementCommand : MonoBehaviour
{
    public Button buttonPosX;
    public Button buttonNegX;
    public Button buttonPosY;
    public Button buttonNegY;
    public Button buttonPosZ;
    public Button buttonNegZ;
    public Button homeButton;
    public Button cancelButton;
    public TMP_Text statusText;
    public TMP_Text xyProgressText;
    public TMP_Text zProgressText;
    public TMP_Text confirmationText;

    private HttpClient client;
    private string commandUrl = "http://192.168.1.102/api/printer/command";
    private string apiKey = "49C7B194E15B41738324436780C19032";

    private float currentX = 0f;
    private float currentY = 0f;
    private float currentZ = 0f;
    private bool canSendCommand = true;
    private bool confirmCancel = false;
    private Coroutine cancelCoroutine;

    void Start()
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        buttonPosX.onClick.AddListener(() => MoveAxis("G0 X10", "X", 10));
        buttonNegX.onClick.AddListener(() => MoveAxis("G0 X-10", "X", -10));
        buttonPosY.onClick.AddListener(() => MoveAxis("G0 Y10", "Y", 10));
        buttonNegY.onClick.AddListener(() => MoveAxis("G0 Y-10", "Y", -10));
        buttonPosZ.onClick.AddListener(() => MoveAxis("G0 Z10", "Z", 10));
        buttonNegZ.onClick.AddListener(() => MoveAxis("G0 Z-10", "Z", -10));
        homeButton.onClick.AddListener(HomeAllAxes);
        cancelButton.onClick.AddListener(CancelOrConfirm);
    }

    async void MoveAxis(string gCodeCommand, string axis, float moveAmount)
    {
        Debug.Log($"Attempting to send command: {gCodeCommand}");
        Debug.Log($"Axis: {axis}, Move Amount: {moveAmount}");

        if (!canSendCommand)
        {
            statusText.text = "Please wait for the cooldown period.";
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
                    xyProgressText.text = $"X Axis Position: {currentX}";
                }
                else if (axis == "Y")
                {
                    currentY += moveAmount;
                    xyProgressText.text = $"Y Axis Position: {currentY}";
                }
                else if (axis == "Z")
                {
                    currentZ += moveAmount;
                    zProgressText.text = $"Z Axis Position: {currentZ}";
                }

                statusText.text = $"Command {gCodeCommand} sent successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to send command {gCodeCommand}. Response: {response.StatusCode}, {errorContent}");
                statusText.text = $"Failed to send command {gCodeCommand}. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusText.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        canSendCommand = true;
        statusText.text = "Ready for next command.";
    }

    async void HomeAllAxes()
    {
        if (!canSendCommand)
        {
            statusText.text = "Please wait for the cooldown period.";
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
                xyProgressText.text = "X/Y Axis Position: 0";
                zProgressText.text = "Z Axis Position: 0";
                statusText.text = "Home command sent successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to send home command. Response: {response.StatusCode}, {errorContent}");
                statusText.text = $"Failed to send home command. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusText.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        canSendCommand = true;
        statusText.text = "Ready for next command.";
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
                confirmationText.text = ""; // Clear the confirmation text
            }
        }
        else
        {
            confirmationText.text = "Are you sure you want to cancel the print? Press again to confirm.";
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
                confirmationText.text = "";
                statusText.text = $"Print cancelled successfully.";
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                statusText.text = $"Failed to cancel print. Response: {response.StatusCode}, {errorContent}";
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error: {e.Message}");
            statusText.text = $"Request error: {e.Message}";
        }

        await Cooldown(2);
        statusText.text = "Ready for next command.";
    }

    IEnumerator ResetCancelConfirmation()
    {
        yield return new WaitForSeconds(5);
        confirmCancel = false;
        confirmationText.text = ""; // Clear the confirmation text after 5 seconds
    }

    async Task Cooldown(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            statusText.text = $"Cooldown: {i} seconds";
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
public class CommandPayload
{
    public string command;
}
