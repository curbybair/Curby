using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdatedCameraScript : MonoBehaviour
{
    public RawImage display1;
    public RawImage display2;
    public RawImage display3;

    public TMP_Text startStopText;

    private WebCamTexture tex1;
    private WebCamTexture tex2;
    private WebCamTexture tex3;

    public void StartStopCams_Clicked()
    {
        if (tex1 != null && tex2 != null && tex3 != null) // Stop all Cameras
        {
            StopWebcams();
            startStopText.text = "Start Cameras";
        }
        else // Start all Cameras
        {
            StartWebcams();
            startStopText.text = "Stop Cameras";
        }
    }

    private void StartWebcams()
    {
        if (WebCamTexture.devices.Length >= 3)
        {
            tex1 = new WebCamTexture(WebCamTexture.devices[0].name);
            tex2 = new WebCamTexture(WebCamTexture.devices[1].name);
            tex3 = new WebCamTexture(WebCamTexture.devices[2].name);

            display1.texture = tex1;
            display2.texture = tex2;
            display3.texture = tex3;

            tex1.Play();
            tex2.Play();
            tex3.Play();
        }
        else
        {
            Debug.LogError("Not enough cameras detected.");
        }
    }

    private void StopWebcams()
    {
        if (tex1 != null)
        {
            tex1.Stop();
            display1.texture = null;
            tex1 = null;
        }

        if (tex2 != null)
        {
            tex2.Stop();
            display2.texture = null;
            tex2 = null;
        }

        if (tex3 != null)
        {
            tex3.Stop();
            display3.texture = null;
            tex3 = null;
        }
    }
}
