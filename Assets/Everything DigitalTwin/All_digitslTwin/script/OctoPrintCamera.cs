using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;


public class OctoPrintCamera : MonoBehaviour
{
    public RawImage rawImage;
    public Button startButton;
    public string streamUrl = "http://192.168.1.101/webcam/?action=stream";

    private HttpClient client;
    private Texture2D texture;

    void Start()
    {
        client = new HttpClient();
        startButton.onClick.AddListener(StartStream);
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        rawImage.texture = texture;
    }

    async void StartStream()
    {
        await Task.Run(() => FetchMJPEGStream());
    }

    async Task FetchMJPEGStream()
    {
        try
        {
            var response = await client.GetStreamAsync(streamUrl);
            var memoryStream = new MemoryStream();

            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await response.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                    byte[] data = memoryStream.ToArray();
                    int start = FindJPEGHeader(data);
                    int end = FindJPEGFooter(data, start);

                    if (start >= 0 && end >= 0)
                    {
                        byte[] jpegData = new byte[end - start + 1];
                        System.Array.Copy(data, start, jpegData, 0, jpegData.Length);
                        memoryStream.SetLength(0);
                        memoryStream.Write(data, end + 1, data.Length - end - 1);

                        // Schedule the update to the main thread
                        UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateTexture(jpegData));
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);
        }
    }

    void UpdateTexture(byte[] jpegData)
    {
        texture.LoadImage(jpegData);
        texture.Apply();
    }

    int FindJPEGHeader(byte[] data)
    {
        for (int i = 0; i < data.Length - 1; i++)
        {
            if (data[i] == 0xFF && data[i + 1] == 0xD8) // JPEG header
            {
                return i;
            }
        }
        return -1;
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
            client.Dispose();
        }
    }
}