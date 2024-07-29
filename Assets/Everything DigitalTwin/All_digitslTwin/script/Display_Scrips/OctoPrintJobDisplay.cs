using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OctoPrintJobData
{
    public string state;
    public ProgressData progress;
}

[Serializable]
public class ProgressData
{
    public string printTime;
}

public class OctoPrintJobDisplay : MonoBehaviour
{
    public string printer1APIKey = "5D44B0739CB04110871B24C202512986";
    public string printer2APIKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";
    public string printer1URL = "http://192.168.1.103/api/job";
    public string printer2URL = "http://192.168.1.105/api/job";

    public string Printer1State { get; private set; }
    public string Printer2State { get; private set; }

    public string Printer1Progress { get; private set; }
    public string Printer2Progress { get; private set; }

    private float nextActionTime = 0.0f;
    public float period = 2f;

    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            StartCoroutine(GetState(printer1URL, printer1APIKey, "Printer1"));
            StartCoroutine(GetState(printer2URL, printer2APIKey, "Printer2"));
        }
    }
   

    IEnumerator GetState(string url, string apiKey, string printerName)
    {
        string requestUrl = $"{url}?apikey={apiKey}";

        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError($"Failed to fetch data from {printerName}: {request.error}");
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;

        try
        {
            OctoPrintJobData jobData = JsonUtility.FromJson<OctoPrintJobData>(jsonResponse);
            string state = jobData.state;
            string progress = jobData.progress.printTime;


            if (printerName == "Printer1")
            {
                Printer1State = state;
                Printer1Progress = progress;
               // Debug.Log($"Printer1 - Status: {Printer1State}, Progress: {Printer1Progress}");
            }
            else if (printerName == "Printer2")
            {
                Printer2State = state;
                Printer2Progress = progress;
                //Debug.Log($"Printer2 - Status: {Printer2State}, Progress: {Printer2Progress}");
            }

        }

         catch (Exception e)
        {
            Debug.LogError($"Failed to parse {printerName} data: {e.Message}");
        }
    }
}



