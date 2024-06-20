using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class Camera : MonoBehaviour
{
    public RawImage display;
    public TMP_Text startStopText;
    public string apiKey = "151B3189E551406F9C93A218FD50FD0E"; // Replace with your actual API key
    public string webcamURL = "http://192.168.1.101/webcam/?action=stream";

    private bool cameraRunning = false;

    void Start()
    {
        startStopText.text = "Start Camera";
    }

    public void StartStopCams_Clicked()
    {
        if (cameraRunning) // Stop the Camera
        {
            StopAllCoroutines();
            ClearDisplay();
            startStopText.text = "Start Camera";
            cameraRunning = false;
            Debug.Log("Camera stopped");
        }
        else // Start the Camera
        {
            cameraRunning = true;
            StartCoroutine(StreamCamera(webcamURL, apiKey));
            startStopText.text = "Stop Camera";
            Debug.Log("Camera started");
        }
    }

    private IEnumerator StreamCamera(string url, string apiKey)
    {
        Debug.Log("Coroutine started"); // Check if coroutine starts
        while (cameraRunning)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            www.SetRequestHeader("X-Api-Key", apiKey);
            yield return www.SendWebRequest();

            Debug.Log("Request sent"); // Check if request is sent

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError($"Error: {www.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (texture != null)
                {
                    display.texture = texture;
                    Debug.Log("Texture updated");
                }
                else
                {
                    Debug.LogWarning("Failed to load texture from received data");
                }
            }

            yield return new WaitForSeconds(0.1f); // Adjust delay to control frame rate
        }
    }



    private void ClearDisplay()
    {
        display.texture = null;
        Debug.Log("Display cleared");
    }
}