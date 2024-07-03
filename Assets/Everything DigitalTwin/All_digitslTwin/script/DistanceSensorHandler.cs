using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using TMPro;

public class DistanceSensorHandler : MonoBehaviour
{
    private SubscriberSocket subscriberSocket;
    private Thread receiveThread;
    private bool running = false;

    public GameObject xObject;
    public GameObject yObject;
    public GameObject zObject;
    public TextMeshProUGUI positionText;

    private Vector3 initialXPosition;
    private Vector3 initialYPosition;
    private Vector3 initialZPosition;

    private Vector3 targetXPosition;
    private Vector3 targetYPosition;
    private Vector3 targetZPosition;

    public float smoothSpeed = 0.125f; // Speed of interpolation

    void Start()
    {
        Debug.Log("Starting DistanceSensorHandler...");

        // Store initial positions
        initialXPosition = xObject.transform.position;
        initialYPosition = yObject.transform.position;
        initialZPosition = zObject.transform.position;

        // Initialize target positions with initial positions
        targetXPosition = initialXPosition;
        targetYPosition = initialYPosition;
        targetZPosition = initialZPosition;

        ConnectToZMQ();
    }

    void Update()
    {
        // Interpolate positions
        xObject.transform.position = Vector3.Lerp(xObject.transform.position, targetXPosition, smoothSpeed * Time.deltaTime);
        yObject.transform.position = Vector3.Lerp(yObject.transform.position, targetYPosition, smoothSpeed * Time.deltaTime);
        zObject.transform.position = Vector3.Lerp(zObject.transform.position, targetZPosition, smoothSpeed * Time.deltaTime);
    }

    void ConnectToZMQ()
    {
        try
        {
            Debug.Log("Initializing AsyncIO and NetMQ...");
            AsyncIO.ForceDotNet.Force(); // Ensure AsyncIO library is initialized
            NetMQConfig.Cleanup(); // Clean up any previous NetMQ configurations

            // Initialize the subscriber socket
            subscriberSocket = new SubscriberSocket();
            subscriberSocket.Options.ReceiveHighWatermark = 1000;
            subscriberSocket.Options.Linger = TimeSpan.Zero;

            // Connect to the ZMQ server
            string raspberryPiIp = "tcp://192.168.1.111:5555";  // Replace with your server address
            subscriberSocket.Connect(raspberryPiIp);

            // Subscribe to the specific topic
            string topic = "Port1_Brown_Direc";
            subscriberSocket.Subscribe(topic);
            Debug.Log("Subscribed to topic: " + topic);

            // Start listening for messages
            running = true;
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to ZMQ server: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    void ReceiveMessages()
    {
        while (running)
        {
            try
            {
                if (subscriberSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(1000), out string message)) // Increased timeout to 1000 ms
                {
                    Debug.Log("Received message: " + message);
                    ProcessMessage(message);
                }
                else
                {
                    Debug.Log("No message received within timeout period.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in ReceiveMessages: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }

    void ProcessMessage(string message)
    {
        // Split the message to get the data part
        string[] parts = message.Split('%');
        if (parts.Length > 1)
        {
            string topic = parts[0];
            string inputString = parts[1];
            Debug.Log("Extracted string for topic " + topic + ": " + inputString);

            // Clean and parse the string
            string cleanString = inputString.Replace("'", "").Replace(" ", "").Replace("[", "").Replace("]", "");
            Debug.Log("Parsed list: " + cleanString);

            string[] stringArray = cleanString.Split(',');

            // Process data for x, y, and z coordinates
            if (stringArray.Length >= 3)
            {
                if (float.TryParse(stringArray[0], out float xPositionCm) &&
                    float.TryParse(stringArray[1], out float yPositionCm) &&
                    float.TryParse(stringArray[2], out float zPositionCm))
                {
                    // Convert cm to meters
                    float xPositionMeters = xPositionCm / 100.0f;
                    float yPositionMeters = yPositionCm / 100.0f;
                    float zPositionMeters = zPositionCm / 100.0f;

                    // Calculate target positions by adding to initial positions
                    Vector3 newXPosition = initialXPosition + new Vector3(xPositionMeters, 0, 0);
                    Vector3 newYPosition = initialYPosition + new Vector3(0, yPositionMeters, 0);
                    Vector3 newZPosition = initialZPosition + new Vector3(0, 0, zPositionMeters);

                    // Update target positions on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        targetXPosition = newXPosition;
                        targetYPosition = newYPosition;
                        targetZPosition = newZPosition;

                        positionText.text = $"X: {newXPosition.x:F2} m, Y: {newYPosition.y:F2} m, Z: {newZPosition.z:F2} m";

                        Debug.Log($"New Positions - X: {newXPosition.x} m, Y: {newYPosition.y} m, Z: {newZPosition.z} m");
                    });
                }
                else
                {
                    Debug.LogWarning($"Failed to parse position values: {inputString}");
                }
            }
            else
            {
                Debug.LogWarning($"Parsed list does not contain enough elements: {cleanString}");
            }
        }
        else
        {
            Debug.LogWarning("Received message format is incorrect.");
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    void OnDisable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        running = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(2000);
        }
        subscriberSocket?.Close();
        subscriberSocket?.Dispose();
        NetMQConfig.Cleanup(false);
    }
}
