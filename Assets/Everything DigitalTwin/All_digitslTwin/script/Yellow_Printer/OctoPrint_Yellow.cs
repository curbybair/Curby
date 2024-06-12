using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class OctoPrint_Yellow : MonoBehaviour
{
    public string octoPrintUrl = "http://192.168.1.102/";
    public string apiKey = "CA29191915284A00A0207FB0213F0275";
    

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

    public float BedActualTemperature { get; private set; }
    public float ToolActualTemperature { get; private set; }
    public bool IsTemperatureEndpointActive { get; set; } = true;

    private float nextActionTime = 0.0f;
    public float period = 2f;
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

                /* Debug.Log("Bed Temperature for Yellow Printer: " + BedactualTemperature);
                 Debug.Log("Head Temperature for Yelllow Printer: " + ToolactualTemperature);*/


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


}

