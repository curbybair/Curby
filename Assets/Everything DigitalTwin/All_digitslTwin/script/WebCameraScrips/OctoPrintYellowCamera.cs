using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

public class OctoPrintYellowCamera : MonoBehaviour
{
    public RawImage Display_Image;
    public Button startButton;
    public string streamUrl = "http://192.168.1.105/webcam/?action=stream";

    private HttpClient client;
    private Texture2D texture;
    private CancellationTokenSource cancellationTokenSource;
    private bool isStreaming = false;

    private const int FrameSkip = 2; // Process every 2nd frame
    private int frameCounter = 0;

    void Start()
    {
        client = new HttpClient();
        startButton.onClick.AddListener(StartStream);
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        Display_Image.texture = texture;
    }

    async void StartStream()
    {
        if (isStreaming) return;

        isStreaming = true;
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Run(() => FetchMJPEGStream(cancellationTokenSource.Token));
        }
        catch (TaskCanceledException)
        {
            Debug.Log("Stream canceled.");
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);
        }
        finally
        {
            isStreaming = false;
        }
    }

    async Task FetchMJPEGStream(CancellationToken cancellationToken)
    {
        try
        {
            using (var response = await client.GetStreamAsync(streamUrl))
            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                while (!cancellationToken.IsCancellationRequested)
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

                            // Skip frames to optimize
                            if (frameCounter++ % FrameSkip == 0)
                            {
                                UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateTexture(jpegData));
                            }
                        }
                    }
                }
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("FetchMJPEGStream task canceled.");
        }
        catch (IOException e)
        {
            Debug.LogError("Stream read error: " + e.Message);
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
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }
    }
}