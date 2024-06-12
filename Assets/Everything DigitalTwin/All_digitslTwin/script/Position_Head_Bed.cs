using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Position_Head_Bed : MonoBehaviour
{
    public string octopiUrl = "http://octopi.local"; 
    public string apiKey = "151B3189E551406F9C93A218FD50FD0E"; 

    public void SendM114Command()
    {
        StartCoroutine(SendCommand());
    }

    private IEnumerator SendCommand()
    {
        string url = octopiUrl + "/api/terminal/commands";
        WWWForm form = new WWWForm();
        form.AddField("command", "M114");

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("X-Api-Key", apiKey);
        Debug.Log("Command sent successfully 1.");
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Failed to send the command: " + request.error);
        }
        else
        {
            Debug.Log("Command sent successfully.");
            Debug.Log("Response from OctoPrint: " + request.downloadHandler.text);
        }
    }
}


