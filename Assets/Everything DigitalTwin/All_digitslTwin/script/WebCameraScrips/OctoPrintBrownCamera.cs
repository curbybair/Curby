using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;                //System.IO is used for the stored data read form client...var memoryStream = new MemoryStream();

public class OctoPrintBrownCamera : MonoBehaviour
{
    public RawImage Display_Image;                                                                              //Public Field for the raw Image
    public Button startButton;                                                                                 //Public Field for the On/off Button 
    public string streamUrl = "http://192.168.1.103/webcam/?action=stream";                                   //Stream Url Link

    private HttpClient client;                          //Client to capture the URL data
    private Texture2D texture;                         //Texture for displaying video fames

    void Start()
    {
        client = new HttpClient();                                          //adds a listener to the button, so when it is clicked, the StartStream method is called.
        startButton.onClick.AddListener(StartStream);
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        Display_Image.texture = texture;                                   // Assign the texture to the RawImage component
    }

    async void StartStream()
    {
        await Task.Run(() => FetchMJPEGStream());                          //the method responible the MJPEG stream. It calls FetchMJPEGStream on a background thread.
    }

    async Task FetchMJPEGStream()
    {
        try
        {
            var response = await client.GetStreamAsync(streamUrl);
            var memoryStream = new MemoryStream();                                      // Createa a stream of memeory to store the fetched data


            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await response.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                    byte[] data = memoryStream.ToArray();                                  // Convert the memory stream to a byte array
                    int start = FindJPEGHeader(data);
                    int end = FindJPEGFooter(data, start);

                    if (start >= 0 && end >= 0)
                    {
                        byte[] jpegData = new byte[end - start + 1];
                        System.Array.Copy(data, start, jpegData, 0, jpegData.Length);          // Write the remaining data to the memory stream
                        memoryStream.SetLength(0);
                        memoryStream.Write(data, end + 1, data.Length - end - 1);


                        UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateTexture(jpegData));
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);                                  //Logging Errors
        }
    }

    void UpdateTexture(byte[] jpegData)
    {
        texture.LoadImage(jpegData);                                                            //Load the JPEG data into the texture so that you are able to see the image
        texture.Apply();
    }

    int FindJPEGHeader(byte[] data)                                                             //Using JPEGHeader & Footers are used to organize each frame as a JPEG sent so we know when the images start and stop 
    {
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0xFF && data[i + 1] == 0xD8) // JPEG header
            {
                return i;
            }
        }
        return -1;                                     // Return -1 if no header is found
    }

    int FindJPEGFooter(byte[] data, int start)
    {
        for (int i = start + 1; i < data.Length - 1; i++)
        {
            if (data[i] == 0xFF && data[i + 1] == 0xD9) // JPEG footer
            {
                return i + 1;
            }
        }
        return -1;
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.Dispose();               // Distruction of the HTTP to release resources
        }
    }
}
