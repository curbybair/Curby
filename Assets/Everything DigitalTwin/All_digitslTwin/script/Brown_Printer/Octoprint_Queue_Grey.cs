using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Octoprint_Queue_Grey : MonoBehaviour
{
    public Dropdown GreyFiles;
    public TextMeshProUGUI fileBeingPrintedTextGrey;
    public GameObject instructionText;
    public GameObject popupText;
    public GameObject popupBackgroundCircle;
    public GameObject greyArrow;
    
    public Button confirmButton;
    private string printer1url = "http://192.168.1.103/api/files";
    private string printer1key = "5D44B0739CB04110871B24C202512986";

    private string selectedFile = "";

    void Start()
    {
        fileBeingPrintedTextGrey = GameObject.Find("fileBeingPrintedTextGrey").GetComponent<TextMeshProUGUI>();
        if (TutorialManager.Instance.isTutorialActive)
        {
            popupText.SetActive(false);
            //popupBackgroundCircle.SetActive(false);
            instructionText.SetActive(true);
        }
        StartCoroutine(GetFiles());
        GreyFiles.onValueChanged.AddListener(delegate { OnFileSelected(); });

        confirmButton.onClick.AddListener(OnConfirm);
    }

    IEnumerator GetFiles()
    {
        UnityWebRequest request = UnityWebRequest.Get(printer1url);
        request.SetRequestHeader("X-Api-Key", printer1key);
        yield return request.SendWebRequest();

        
        if (request.result == UnityWebRequest.Result.ConnectionError || 
        request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching files: " + request.error);
        }
        else
        {
            Debug.Log("Files fetched successfully from the printer.");
            
            var jsonResponse = JsonUtility.FromJson<FilesResponse>(request.downloadHandler.text);
            List<string> fileNames = new List<string>();

            foreach (var file in jsonResponse.files)
            {
                fileNames.Add(file.name);
            }

            GreyFiles.ClearOptions();
            GreyFiles.AddOptions(fileNames);
        }
    }

    public void OnFileSelected()
    {
        selectedFile = GreyFiles.options[GreyFiles.value].text;
        Debug.Log("Selected File: " + selectedFile);
        if (TutorialManager.Instance.isTutorialActive)
        {
            // Show popup if the selected file is "BlockPartOne"
            if (selectedFile == "BlockPartTwo.gcode")
            {
                instructionText.SetActive(false);
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
            if (selectedFile == "BlockPartTwo.gcode")
            {
                ShowPopup("Starting print for 'BlockPartTwo'...");
                greyArrow.SetActive(false);
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

        string printJobUrl = "http://192.168.1.103/api/files/local/" + filePath;

        Debug.Log("Attempting to start print with file: " + filePath);

        string jsonBody = "{\"command\": \"select\", \"print\": true}";

        UnityWebRequest request = new UnityWebRequest(printJobUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Api-Key", printer1key);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error starting print: " + request.error);
        }
        else
        {
            Debug.Log("Print started successfully");
            fileBeingPrintedTextGrey.text = "Printing: " + filePath;
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
        instructionText.SetActive(true);
        popupBackgroundCircle.SetActive(true);// Show instruction text again
    }

}


