using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro; 

public class OctoPrint_Yellow : MonoBehaviour
{
    public string octoPrintUrl = "http://192.168.1.105/";
    public string apiKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";
    
    public TextMeshProUGUI bedTemperatureText;
    public TextMeshProUGUI toolTemperatureText;
    

    [System.Serializable]
    public class BedResponse
    {
        public BedData bed;
    }
    public class ToolResponse
    {
        public ToolData tool0;
    }


    [System.Serializable]
    public class BedData
    {
        public float actual;
        public float offset;
        public float target;
    }
    [System.Serializable]
    public class ToolData
    {
        public float actual;
        public float offset;
        public float target;
    }
    [System.Serializable]
    public class JobResponse
    {
        public JobData job;
    }

    [System.Serializable]
    public class JobData
    {
        public JobFile file;
    }

    [System.Serializable]
    public class JobFile
    {
        public string name; // Assuming the response has a name field for the file
    }

    public float BedActualTemperature { get; private set; }
    public float ToolActualTemperature { get; private set; }
    public bool IsTemperatureEndpointActive { get; set; } = true;

    private float nextActionTime = 0.0f;
    public float period = 2f;

    void Start()
    {
        // Find and assign TextMeshProUGUI components from children
        bedTemperatureText = GameObject.Find("bedTemperatureText").GetComponent<TextMeshProUGUI>();
        toolTemperatureText = GameObject.Find("toolTemperatureText").GetComponent<TextMeshProUGUI>();
        
    }
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            StartCoroutine(FetchTemperatureData());

        }
    }
    public IEnumerator FetchTemperatureData()
    {
        if (IsTemperatureEndpointActive)
        {
            string TooltemperatureEndpoint = "api/printer/tool";
            string BedtemperatureEndpoint = "api/printer/bed";

            string ToolfullUrl = octoPrintUrl + TooltemperatureEndpoint;
            string BedfullUrl = octoPrintUrl + BedtemperatureEndpoint;
            //Debug.Log("Yellow printer (" + octoPrintUrl + ") is ON");

            UnityWebRequest Toolrequest = UnityWebRequest.Get(ToolfullUrl);
            UnityWebRequest Bedrequest = UnityWebRequest.Get(BedfullUrl);

            Toolrequest.SetRequestHeader("X-Api-Key", apiKey);
            Bedrequest.SetRequestHeader("X-Api-Key", apiKey);


            yield return Toolrequest.SendWebRequest();
            yield return Bedrequest.SendWebRequest();

            if (Toolrequest.responseCode == 200 && Bedrequest.responseCode == 200)
            {


                string Tooldata = Toolrequest.downloadHandler.text;
                string Beddata = Bedrequest.downloadHandler.text;

                ToolResponse ToolResponse = JsonUtility.FromJson<ToolResponse>(Tooldata);
                BedResponse bedResponse = JsonUtility.FromJson<BedResponse>(Beddata);

                float BedactualTemperature = bedResponse.bed.actual;
                float ToolactualTemperature = ToolResponse.tool0.actual;

                float BedtargetTemperature = bedResponse.bed.target; 
                float TooltargetTemperature = ToolResponse.tool0.target;

                // Debug.Log("Bed Temperature for Yellow Printer: " + BedactualTemperature);
                // Debug.Log("Head Temperature for Yelllow Printer: " + ToolactualTemperature);*/


                if (bedTemperatureText != null && toolTemperatureText != null)
                {
                    bedTemperatureText.text = $"Bed Target: {BedtargetTemperature}°C";
                    toolTemperatureText.text = $"Tool Target: {TooltargetTemperature}°C";
                }
                else
                {
                    Debug.LogError("Temperature Text components are not assigned!");
                }

                BedActualTemperature = BedactualTemperature;
                ToolActualTemperature = ToolactualTemperature;

                

            }
            else if (Toolrequest.responseCode == 409 && Bedrequest.responseCode == 409)
            {
                Debug.Log("Printer with " + octoPrintUrl + " is not connected to the Network");
                //yield break;
            }
            else
            {
                Debug.LogError("Failed to fetch temperature data. Response code: " + Toolrequest.responseCode);
                Debug.LogError("Failed to fetch temperature data. Response code: " + Bedrequest.responseCode);
                //yield break;

            }
        }
    
        else
        {
            Debug.Log("Power button is pressed. Yellow printer (" + octoPrintUrl + ") is offline.");
        }
    }
    // public IEnumerator FetchFileData()
    // {
    //     string fileEndpoint = "api/job";
    //     string fullUrl = octoPrintUrl + fileEndpoint;

    //     UnityWebRequest fileRequest = UnityWebRequest.Get(fullUrl);
    //     fileRequest.SetRequestHeader("X-Api-Key", apiKey);

    //     yield return fileRequest.SendWebRequest();

    //     if (fileRequest.responseCode == 200)
    //     {
    //         string fileData = fileRequest.downloadHandler.text;
    //         Debug.Log("File Data Response: " + fileData);
    //         JobResponse jobResponse = JsonUtility.FromJson<JobResponse>(fileData);

    //         if (jobResponse != null && jobResponse.job != null && jobResponse.job.file != null)
    //         {
    //             string fileName = jobResponse.job.file.name;

    //             // Update UI
    //             fileBeingPrintedText.text = "Printing: " + fileName;
    //         }
    //         else
    //         {
    //             Debug.LogError("Job Response or its properties are null");

                    
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to fetch file data. Response code: " + fileRequest.responseCode);
    //     }
    // }


    


}

