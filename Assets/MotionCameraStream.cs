using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Threading;

public class MotionCameraStream : MonoBehaviour
{
    public RawImage displayImage;
    public Button startButton;
    public string streamUrl = "http://192.168.1.137:8081/";

    private HttpClient client;
    private Texture2D texture;
    private Queue<byte[]> frameQueue = new Queue<byte[]>();
    private CancellationTokenSource cancellationTokenSource;
    private bool isStreaming = false;
    private const int BufferSize = 16384;
    private const int MaxQueueSize = 1;
    private int frameSkip = 2;  // Process every 2nd frame
    private int frameCounter = 0;

    void Start()
    {
        client = new HttpClient();
        startButton.onClick.AddListener(StartStream);
        texture = new Texture2D(640, 480, TextureFormat.RGB24, false);
        displayImage.texture = texture;
    }

    async void StartStream()
    {
        if (!isStreaming)
        {
            isStreaming = true;
            cancellationTokenSource = new CancellationTokenSource();
            await Task.Run(() => FetchMJPEGStream(cancellationTokenSource.Token));
        }
    }

    async Task FetchMJPEGStream(CancellationToken cancellationToken)
    {
        try
        {
            using (var response = await client.GetStreamAsync(streamUrl))
            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead;

                while (isStreaming && !cancellationToken.IsCancellationRequested)
                {
                    bytesRead = await response.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
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
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Stream fetching canceled.");
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);
        }
        catch (IOException e)
        {
            Debug.LogError("Stream read error: " + e.Message);
        }
        finally
        {
            isStreaming = false;
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
            if (frameCounter++ % frameSkip == 0)
            {
                UpdateTexture(jpegData);
            }
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
        StopStream();
        if (client != null)
        {
            client.Dispose();
        }
    }

    public void StopStream()
    {
        if (isStreaming)
        {
            isStreaming = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}
