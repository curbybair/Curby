using System.Collections; //----Include necessary namespaces for collections---
using System.Net.Http; //----Include necessary namespaces for HTTP client functionality---
using UnityEngine; //----Include necessary namespaces for Unity specific functionalities---
using UnityEngine.UI; //----Include necessary namespaces for UI elements---

public class SendHomeCommand : MonoBehaviour //----Define a public class inheriting from MonoBehaviour---
{
    public Button homeButton; //----Public variable to reference the UI button---
    public Text cooldownText; //----Public variable to reference the cooldown Text element---
    private HttpClient client; //----Private variable to hold the HttpClient instance---
    private bool canClickButton = true; //----Private variable to track if the button can be clicked---

    void Start() //----Start method called on the frame when a script is enabled---
    {
        client = new HttpClient(); //----Instantiate the HttpClient---
        homeButton.onClick.AddListener(CheckPrinterStatusAndSendG28Command); //----Add a listener to the button that calls CheckPrinterStatusAndSendG28Command method on click---
        cooldownText.text = ""; //----Initialize the cooldown text---
    }

    async void CheckPrinterStatusAndSendG28Command() //----Async method to check printer status and send G28 command---
    {
        if (!canClickButton) //----Check if the button can be clicked---
        {
            return; //----Return if the button cannot be clicked---
        }

        canClickButton = false; //----Disable the button click---

        string statusUrl = "http://192.168.1.102/api/printer"; //----URL for checking the printer status---
        string commandUrl = "http://192.168.1.102/api/printer/command"; //----URL for sending the command to the printer---
        string apiKey = "49C7B194E15B41738324436780C19032"; //----Replace with your OctoPrint API key---

        client.DefaultRequestHeaders.Clear(); //----Clear any existing headers---
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey); //----Add the API key to the headers---

        HttpResponseMessage statusResponse = await client.GetAsync(statusUrl); //----Send a GET request to the printer status URL---
        if (statusResponse.IsSuccessStatusCode) //----Check if the status response is successful---
        {
            string statusResponseBody = await statusResponse.Content.ReadAsStringAsync(); //----Read the response content as a string---
            PrinterStatus status = JsonUtility.FromJson<PrinterStatus>(statusResponseBody); //----Deserialize the JSON response to a PrinterStatus object---

            if (status.state.flags.operational) //----Check if the printer state is operational---
            {
                var commandPayload = new CommandPayload { command = "G28" }; //----Create a new CommandPayload object with the G28 command---
                string jsonPayload = JsonUtility.ToJson(commandPayload); //----Serialize the command payload to a JSON string---

                HttpContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json"); //----Create the content for the POST request---
                HttpResponseMessage commandResponse = await client.PostAsync(commandUrl, content); //----Send the POST request to the command URL---

                if (commandResponse.IsSuccessStatusCode) //----Check if the command response is successful---
                {
                    homeButton.GetComponentInChildren<Text>().text = "Home position set"; //----Update the button text---
                }
            }
        }

        StartCoroutine(EnableButtonAfterCooldown(60)); //----Start the coroutine to enable the button after 60 seconds---
    }

    IEnumerator EnableButtonAfterCooldown(int seconds) //----Coroutine to enable the button after a cooldown period---
    {
        for (int i = seconds; i > 0; i--) //----Countdown loop for the cooldown timer---
        {
            cooldownText.text = $"Cooldown: {i} seconds"; //----Update the cooldown text---
            yield return new WaitForSeconds(1); //----Wait for 1 second---
        }

        canClickButton = true; //----Enable the button click---
        homeButton.GetComponentInChildren<Text>().text = "Send Home Command"; //----Reset the button text---
        cooldownText.text = ""; //----Clear the cooldown text---
    }

    void OnDestroy() //----OnDestroy method called when the script is destroyed---
    {
        if (client != null) //----Check if the client is not null---
        {
            client.Dispose(); //----Dispose the HttpClient instance---
        }
    }
}

[System.Serializable] //----Make the class serializable to convert it to and from JSON---
public class PrinterStatus //----Define the PrinterStatus class---
{
    public State state; //----Public variable to hold the state object---
}

[System.Serializable] //----Make the class serializable to convert it to and from JSON---
public class State //----Define the State class---
{
    public string text; //----Public variable to hold the state text---
    public Flags flags; //----Public variable to hold the flags object---
}

[System.Serializable] //----Make the class serializable to convert it to and from JSON---
public class Flags //----Define the Flags class---
{
    public bool operational; //----Public variable to indicate if the printer is operational---
}

[System.Serializable] //----Make the class serializable to convert it to and from JSON---
public class CommandPayload //----Define the CommandPayload class---
{
    public string command; //----Public variable to hold the command string---
}
