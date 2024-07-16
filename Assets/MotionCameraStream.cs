using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

public class MotionCameraStream : MonoBehaviour
{
    public RawImage displayImage;  // Public Field for the Raw Image
    public Button startButton;  // Public Field for the On/Off Button
    public string streamUrl = "http://192.168.1.137:8081/";  // Stream URL Link

    private HttpClient client;  // Client to capture the URL data
    private Texture2D texture;  // Texture for displaying video frames

    void Start()
    {
        client = new HttpClient();  // Initialize HttpClient
        startButton.onClick.AddListener(StartStream);  // Add listener to button
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);  // Initialize texture
        displayImage.texture = texture;  // Assign the texture to the RawImage component
    }

    async void StartStream()
    {
        await Task.Run(() => FetchMJPEGStream());  // Call FetchMJPEGStream on a background thread
    }

    async Task FetchMJPEGStream()
    {
        try
        {
            var response = await client.GetStreamAsync(streamUrl);
            var memoryStream = new MemoryStream();  // Create a stream of memory to store the fetched data

            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await response.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                    byte[] data = memoryStream.ToArray();  // Convert the memory stream to a byte array
                    int start = FindJPEGHeader(data);
                    int end = FindJPEGFooter(data, start);

                    if (start >= 0 && end >= 0)
                    {
                        byte[] jpegData = new byte[end - start + 1];
                        System.Array.Copy(data, start, jpegData, 0, jpegData.Length);  // Copy the JPEG data
                        memoryStream.SetLength(0);
                        memoryStream.Write(data, end + 1, data.Length - end - 1);  // Write remaining data to the memory stream

                        UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateTexture(jpegData));
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);  // Logging Errors
        }
    }

    void UpdateTexture(byte[] jpegData)
    {
        texture.LoadImage(jpegData);  // Load the JPEG data into the texture
        texture.Apply();
    }

    int FindJPEGHeader(byte[] data)  // Find the JPEG header in the data
    {
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0xFF && data[i + 1] == 0xD8)  // JPEG header
            {
                return i;
            }
        }
        return -1;  // Return -1 if no header is found
    }

    int FindJPEGFooter(byte[] data, int start)  // Find the JPEG footer in the data
    {
        for (int i = start + 1; i < data.Length - 1; i++)
        {
            if (data[i] == 0xFF && data[i + 1] == 0xD9)  // JPEG footer
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
            client.Dispose();  // Dispose of the HttpClient to release resources
        }
    }
}
