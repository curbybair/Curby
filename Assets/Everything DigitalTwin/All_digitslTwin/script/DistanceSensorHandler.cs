using System;
using System.Threading;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using TMPro;

public class DistanceSensorHandler : MonoBehaviour
{
    private SubscriberSocket subscriberSocket;
    private Thread receiveThread;
    private bool running = false;

    public GameObject extruder; // GameObject representing the extruder
    public GameObject bed; // GameObject representing the bed
    public GameObject xLeadingRod; // GameObject representing the x leading rod
    public TextMeshProUGUI positionText;

    public float smoothSpeed = 0.125f; // Speed of interpolation
    public float scalingFactor = 1.0f; // Scaling factor to control the movement scale

    private Vector3 initialExtruderPosition;
    private Vector3 targetExtruderPosition;

    private Vector3 initialBedPosition;
    private Vector3 targetBedPosition;

    private Vector3 initialXLeadingRodPosition;
    private Vector3 targetXLeadingRodPosition;

    void Start()
    {
        Debug.Log("Starting DistanceSensorHandler...");

        // Store initial positions
        initialExtruderPosition = extruder.transform.position;
        initialBedPosition = bed.transform.position;
        initialXLeadingRodPosition = xLeadingRod.transform.position;

        // Initialize target positions with initial positions
        targetExtruderPosition = initialExtruderPosition;
        targetBedPosition = initialBedPosition;
        targetXLeadingRodPosition = initialXLeadingRodPosition;

        ConnectToZMQ();
    }

    void Update()
    {
        // Interpolate positions
        extruder.transform.position = Vector3.Lerp(extruder.transform.position, targetExtruderPosition, smoothSpeed * Time.deltaTime);
        bed.transform.position = Vector3.Lerp(bed.transform.position, targetBedPosition, smoothSpeed * Time.deltaTime);
        xLeadingRod.transform.position = Vector3.Lerp(xLeadingRod.transform.position, targetXLeadingRodPosition, smoothSpeed * Time.deltaTime);
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
            string raspberryPiIp = "tcp://192.168.1.111:5555";
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
                    float.TryParse(stringArray[1], out float zPositionCm) &&
                    float.TryParse(stringArray[2], out float yPositionCm))
                {
                    // Convert cm to meters and apply scaling factor
                    float xPositionMeters = (xPositionCm / 100.0f) * scalingFactor;
                    float zPositionMeters = (zPositionCm / 100.0f) * scalingFactor;
                    float yPositionMeters = (yPositionCm / 100.0f) * scalingFactor;

                    Debug.Log($"Converted Positions - X: {xPositionMeters} m, Z: {zPositionMeters} m, Y: {yPositionMeters} m");

                    // Calculate target positions by adding to initial positions
                    Vector3 newExtruderPosition = initialExtruderPosition;
                    newExtruderPosition.x = initialExtruderPosition.x + xPositionMeters;

                    Vector3 newBedPosition = initialBedPosition;
                    newBedPosition.z = initialBedPosition.z + zPositionMeters;

                    Vector3 newXLeadingRodPosition = initialXLeadingRodPosition;
                    newXLeadingRodPosition.y = initialXLeadingRodPosition.y + yPositionMeters;

                    // Update target positions on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        targetExtruderPosition = newExtruderPosition;
                        targetBedPosition = newBedPosition;
                        targetXLeadingRodPosition = newXLeadingRodPosition;
                        positionText.text = $"X: {newExtruderPosition.x:F2} m, Y: {newXLeadingRodPosition.y:F2} m, Z: {newBedPosition.z:F2} m";

                        Debug.Log($"New Positions - X: {newExtruderPosition.x} m, Y: {newXLeadingRodPosition.y} m, Z: {newBedPosition.z} m");
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
