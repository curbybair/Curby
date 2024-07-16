using System;
using System.Threading;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;

public class DistanceSensorHandler : MonoBehaviour
{
    private SubscriberSocket subscriberSocket;
    private Thread receiveThread;
    private bool running = false;

    public GameObject extruder; // GameObject representing the extruder
    public GameObject bed; // GameObject representing the bed
    public GameObject xLeadingRod; // GameObject representing the x leading rod

    public float smoothSpeed = 0.125f; // Speed of interpolation
    public Vector3 bedOffset = Vector3.zero; // Public offset for the bed

    private Vector3 initialExtruderPosition;
    private Vector3 initialBedPosition;
    private Vector3 initialXLeadingRodPosition;

    private Vector3 targetExtruderPosition;
    private Vector3 targetBedPosition;
    private Vector3 targetXLeadingRodPosition;

    void Start()
    {
        Debug.Log("Starting DistanceSensorHandler...");

        // Initialize initial positions with current positions
        if (extruder != null)
        {
            initialExtruderPosition = extruder.transform.position;
            targetExtruderPosition = initialExtruderPosition;
        }
        else
        {
            Debug.LogError("Extruder GameObject is not assigned!");
        }

        if (bed != null)
        {
            initialBedPosition = bed.transform.position + bedOffset;
            targetBedPosition = initialBedPosition;
        }
        else
        {
            Debug.LogError("Bed GameObject is not assigned!");
        }

        if (xLeadingRod != null)
        {
            initialXLeadingRodPosition = xLeadingRod.transform.position;
            targetXLeadingRodPosition = initialXLeadingRodPosition;
        }
        else
        {
            Debug.LogError("xLeadingRod GameObject is not assigned!");
        }

        ConnectToZMQ();
    }

    void Update()
    {
        // Interpolate positions
        if (extruder != null)
            extruder.transform.position = Vector3.Lerp(extruder.transform.position, targetExtruderPosition, smoothSpeed * Time.deltaTime);

        if (bed != null)
            bed.transform.position = Vector3.Lerp(bed.transform.position, targetBedPosition, smoothSpeed * Time.deltaTime);

        if (xLeadingRod != null)
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
            string topic = "Port0_Grey_Position";
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
                if (subscriberSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(2000), out string message)) // Increased timeout to 2000 ms
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
                    // Convert cm to meters
                    float xPositionMeters = xPositionCm / 100.0f;
                    float zPositionMeters = zPositionCm / 100.0f;
                    float yPositionMeters = yPositionCm / 100.0f;

                    Debug.Log($"Converted Positions - X: {xPositionMeters} m, Z: {zPositionMeters} m, Y: {yPositionMeters} m");

                    // Update target positions relative to initial positions with negative adjustments for x and y
                    Vector3 newExtruderPosition = initialExtruderPosition + new Vector3(-xPositionMeters, -yPositionMeters, 0);
                    Vector3 newBedPosition = initialBedPosition + new Vector3(0, 0, zPositionMeters);
                    Vector3 newXLeadingRodPosition = initialXLeadingRodPosition + new Vector3(0, -yPositionMeters, 0);

                    // Update target positions on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (extruder != null)
                        {
                            targetExtruderPosition = newExtruderPosition;
                        }

                        if (bed != null)
                        {
                            targetBedPosition = newBedPosition;
                        }

                        if (xLeadingRod != null)
                        {
                            targetXLeadingRodPosition = newXLeadingRodPosition;
                        }

                        Debug.Log($"Updated Positions - Extruder: {extruder.transform.position}, Bed: {bed.transform.position}, xLeadingRod: {xLeadingRod.transform.position}");
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
