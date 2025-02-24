using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

[Serializable]
public class TerminalOutput
{
    public string[] logs;
}

[Serializable]
public class GCodeJob
{
    public FileData file;

    [Serializable]
    public class FileData
    {
        public string[] gcodeLines;
    }
}

public class OctoPrintManager : MonoBehaviour
{
    [Serializable]
    public class PrinterConfig
    {
        public string octoPrintUrl; // Base URL of your OctoPrint server
        public string apiKey;       // OctoPrint API key
    }

    public PrinterConfig[] printers; // Array of printer configurations

    private Dictionary<string, WebSocket> webSockets = new Dictionary<string, WebSocket>();

    void Start()
    {
        foreach (var printer in printers)
        {
            ConnectToWebSocket(printer);
        }
    }

    void ConnectToWebSocket(PrinterConfig printerConfig)
    {
        string socketUrl = $"{printerConfig.octoPrintUrl}/sockjs/websocket";
        WebSocket webSocket = new WebSocket(socketUrl);

        // Add the API key as a custom header
        webSocket.SetCookie(new WebSocketSharp.Net.Cookie("apiKey", printerConfig.apiKey));

        // WebSocket events
        webSocket.OnOpen += (sender, e) =>
        {
            Debug.Log($"WebSocket connected to {printerConfig.octoPrintUrl}.");
        };

        webSocket.OnMessage += (sender, e) =>
        {
            ProcessWebSocketMessage(e.Data, printerConfig.octoPrintUrl);
        };

        webSocket.OnClose += (sender, e) =>
        {
            Debug.LogWarning($"WebSocket connection closed for {printerConfig.octoPrintUrl}.");
        };

        webSocket.OnError += (sender, e) =>
        {
            Debug.LogError($"WebSocket error for {printerConfig.octoPrintUrl}: {e.Message}");
        };

        webSocket.ConnectAsync();
        webSockets[printerConfig.octoPrintUrl] = webSocket;
    }

    void ProcessWebSocketMessage(string message, string printerUrl)
    {
        try
        {
            // Use JsonUtility to parse WebSocket message into terminal output or G-code structures
            TerminalOutput terminalOutput = JsonUtility.FromJson<TerminalOutput>(message);
            if (terminalOutput != null && terminalOutput.logs != null)
            {
                foreach (var log in terminalOutput.logs)
                {
                    Debug.Log($"[{printerUrl}] Terminal Output: {log}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing WebSocket message from {printerUrl}: {ex.Message}");
        }
    }

    void OnDestroy()
    {
        foreach (var webSocket in webSockets.Values)
        {
            if (webSocket != null && webSocket.IsAlive)
            {
                webSocket.Close();
            }
        }
    }

    public IEnumerator FetchTerminalOutput(PrinterConfig printerConfig)
    {
        string url = $"{printerConfig.octoPrintUrl}/api/printer/command";

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("X-Api-Key", printerConfig.apiKey);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError ||
                request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"Error fetching terminal output for {printerConfig.octoPrintUrl}: {request.error}");
            }
            else
            {
                try
                {
                    TerminalOutput terminalOutput = JsonUtility.FromJson<TerminalOutput>(request.downloadHandler.text);
                    if (terminalOutput != null && terminalOutput.logs != null)
                    {
                        foreach (var log in terminalOutput.logs)
                        {
                            Debug.Log($"[{printerConfig.octoPrintUrl}] Terminal Log: {log}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing terminal output for {printerConfig.octoPrintUrl}: {ex.Message}");
                }
            }
        }
    }

    public IEnumerator FetchGCodeLines(PrinterConfig printerConfig)
    {
        string url = $"{printerConfig.octoPrintUrl}/api/job";

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("X-Api-Key", printerConfig.apiKey);

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError ||
                request.result == UnityEngine.Networking.UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError($"Error fetching G-code lines for {printerConfig.octoPrintUrl}: {request.error}");
            }
            else
            {
                try
                {
                    GCodeJob gCodeJob = JsonUtility.FromJson<GCodeJob>(request.downloadHandler.text);
                    if (gCodeJob != null && gCodeJob.file != null && gCodeJob.file.gcodeLines != null)
                    {
                        foreach (var line in gCodeJob.file.gcodeLines)
                        {
                            Debug.Log($"[{printerConfig.octoPrintUrl}] G-code Line: {line}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing G-code lines for {printerConfig.octoPrintUrl}: {ex.Message}");
                }
            }
        }
    }
}
