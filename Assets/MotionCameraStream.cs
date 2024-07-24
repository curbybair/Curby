using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

public class MotionCameraStream : MonoBehaviour
{
    public RawImage displayImage;
    public Button startButton;
    public string streamUrl = "http://192.168.1.137:8081/";

    private HttpClient client;
    private Texture2D texture;
    private Queue<byte[]> frameQueue = new Queue<byte[]>();
    private bool isStreaming = false;
    private const int BufferSize = 16384;  // Adjust buffer size as needed
    private const int MaxQueueSize = 1;  // Limit the queue size to reduce latency

    void Start()
    {
        client = new HttpClient();
        startButton.onClick.AddListener(StartStream);
        texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);  // Set texture size based on expected stream resolution
        displayImage.texture = texture;
    }

    async void StartStream()
    {
        if (!isStreaming)
        {
            isStreaming = true;
            await Task.Run(() => FetchMJPEGStream());
        }
    }

    async Task FetchMJPEGStream()
    {
        try
        {
            var response = await client.GetStreamAsync(streamUrl);
            var memoryStream = new MemoryStream();

            while (isStreaming)
            {
                byte[] buffer = new byte[BufferSize];
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

                        lock (frameQueue)
                        {
                            if (frameQueue.Count < MaxQueueSize)
                            {
                                frameQueue.Enqueue(jpegData);
                            }
                        }
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);
        }
    }

    void Update()
    {
        if (frameQueue.Count > 0)
        {
            byte[] jpegData;
            lock (frameQueue)
            {
                jpegData = frameQueue.Dequeue();
            }
            UpdateTexture(jpegData);
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
            if (data[i] == 0xFF && data[i + 1] == 0xD8)
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
            if (data[i] == 0xFF && data[i + 1] == 0xD9)
            {
                return i + 1;
            }
        }
        return -1;
    }

    void OnDestroy()
    {
        isStreaming = false;
        if (client != null)
        {
            client.Dispose();
        }
    }
}
