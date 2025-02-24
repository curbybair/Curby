using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Octoprint_Queue_Yellow : MonoBehaviour
{
    public Dropdown YellowFiles;
    public TextMeshProUGUI fileBeingPrintedText;
    public GameObject instructionText;
    public GameObject popupText;
    public GameObject popupBackgroundCircle;
    public GameObject yellowArrow;
    public GameObject greyArrow;
    public GameObject greyBubble;
    public Button confirmButton;
    private string printer2url = "http://192.168.1.105/api/files"; 
    private string printer2key = "D8F7EE4BAA8C4E67A41FFA537FC91C0B"; 

    private string selectedFile = "";
    void Start()
    {
        fileBeingPrintedText = GameObject.Find("fileBeingPrintedText").GetComponent<TextMeshProUGUI>();
        //instructionText = GameObject.Find("InstructionYellow"); // Reference to instruction text
        if (TutorialManager.Instance.isTutorialActive)
        {
            popupText.SetActive(false);
            instructionText.SetActive(true); 
        }
        StartCoroutine(GetFiles());
        YellowFiles.onValueChanged.AddListener(delegate { OnFileSelected(); });

        confirmButton.onClick.AddListener(OnConfirm);
    }

    IEnumerator GetFiles()
    {
        UnityWebRequest request = UnityWebRequest.Get(printer2url);
        request.SetRequestHeader("X-Api-Key", printer2key);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching files: " + request.error);
        }
        else
        {
            Debug.Log("Files fetched successfully from the Yellow printer.");

            var jsonResponse = JsonUtility.FromJson<FilesResponse>(request.downloadHandler.text);
            List<string> fileNames = new List<string>();

            foreach (var file in jsonResponse.files)
            {
                fileNames.Add(file.name);
            }

            YellowFiles.ClearOptions();
            YellowFiles.AddOptions(fileNames);
        }
    }

    public void OnFileSelected()
    {
        selectedFile = YellowFiles.options[YellowFiles.value].text;
        Debug.Log("Selected File: " + selectedFile);
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Show popup if the selected file is "BlockPartOne"
            if (selectedFile == "BlockPartOne.gcode")
            {
                instructionText.SetActive(false);
                yellowArrow.SetActive(false);
                ShowPopup("Correct file chosen! Click confirm print!");
            }
            else
            {
                StartCoroutine(HandleWrongFile());
                ShowPopup("Wrong file chosen! Try again!");
            }
        }

        
    }
    void OnConfirm()
    {
        if (string.IsNullOrEmpty(selectedFile))
        {
            ShowPopup("No file selected!");
            return;
        }

        Debug.Log("Confirmed File: " + selectedFile);
        if (TutorialManager.Instance.isTutorialActive)
        {
            if (selectedFile == "BlockPartOne.gcode")
            {
                ShowPopup("Starting print for 'BlockPartOne'... Please proceed to the Grey Printer");
                greyArrow.SetActive(true);
                greyBubble.SetActive(true);
                StartCoroutine(StartPrint(selectedFile));
            }
            else
            {
                ShowPopup("Cannot start print. Wrong file!");
            }
        }
        else
        {
            StartCoroutine(StartPrint(selectedFile));
        }
    }

    IEnumerator StartPrint(string filePath)
    {
        string escapedFilePath = UnityWebRequest.EscapeURL(filePath);

        string printJobUrl = "http://192.168.1.105/api/files/local/" + filePath;

        Debug.Log("Attempting to start print with file: " + filePath); 

        string jsonBody = "{\"command\": \"select\", \"print\": true}";

        UnityWebRequest request = new UnityWebRequest(printJobUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", printer2key);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error starting print: " + request.error);
        }
        else
        {
            Debug.Log("Yellow printer started successfully.");
            fileBeingPrintedText.text = "Printing: " + filePath;
        }
    }
    void ShowPopup(string message)
    {
        popupText.GetComponent<TextMeshProUGUI>().text = message;

        popupBackgroundCircle.SetActive(true);
        popupText.SetActive(true);

        // Hide popup after 3 seconds
        Invoke(nameof(HidePopup), 3f);
    }

    void HidePopup()
    {
        popupText.SetActive(false);
        popupBackgroundCircle.SetActive(false);
    }

    IEnumerator HandleWrongFile()
    {
        instructionText.SetActive(false);
        popupBackgroundCircle.SetActive(false); // Temporarily hide instruction text
        yield return new WaitForSeconds(3f); // Wait for 3 seconds
        instructionText.SetActive(true); // Show instruction text again
        popupBackgroundCircle.SetActive(true);
    }
}

