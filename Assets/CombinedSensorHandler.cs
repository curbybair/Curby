using System;
using UnityEngine;
using TMPro;
using NetMQ;
using NetMQ.Sockets;

public class CombinedSensorHandler : MonoBehaviour
{
    private SubscriberSocket subscriberSocketPort0;
    private SubscriberSocket subscriberSocketPort1;
    private NetMQPoller poller;

    public GameObject extruder; // GameObject representing the extruder
    public GameObject bed; // GameObject representing the bed
    public GameObject xLeadingRod; // GameObject representing the x leading rod

    public TMP_Text temperatureText;
    public TMP_Text pressureText;
    public TMP_Text humidityText;
    public TMP_Text airQualityText;

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
        Debug.Log("Starting CombinedSensorHandler...");

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

            // Initialize the subscriber sockets
            subscriberSocketPort0 = new SubscriberSocket();
            subscriberSocketPort0.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort0.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort0.Connect("tcp://192.168.1.111:5555");
            subscriberSocketPort0.Subscribe("Port0_Grey_Position");
            Debug.Log("Subscribed to topic: Port0_Grey_Position");

            subscriberSocketPort1 = new SubscriberSocket();
            subscriberSocketPort1.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort1.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort1.Connect("tcp://192.168.1.111:5555");
            subscriberSocketPort1.Subscribe("Port1_Grey_Environment");
            Debug.Log("Subscribed to topic: Port1_Grey_Environment");

            // Initialize poller
            poller = new NetMQPoller { subscriberSocketPort0, subscriberSocketPort1 };

            subscriberSocketPort0.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port0_Grey_Position: " + message);
                ProcessPositionMessage(message);
            };

            subscriberSocketPort1.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port1_Grey_Environment: " + message);
                ProcessEnvironmentMessage(message);
            };

            poller.RunAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to ZMQ server: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    void ProcessPositionMessage(string message)
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

    void ProcessEnvironmentMessage(string message)
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

            // Process data for temperature, pressure, humidity, and air quality
            if (stringArray.Length >= 4)
            {
                if (float.TryParse(stringArray[0], out float temperature) &&
                    float.TryParse(stringArray[1], out float pressure) &&
                    float.TryParse(stringArray[2], out float humidity) &&
                    float.TryParse(stringArray[3], out float airQuality))
                {
                    // Update UI text fields on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (temperatureText != null)
                            temperatureText.text = $"Temperature: {temperature} °C";
                        if (pressureText != null)
                            pressureText.text = $"Pressure: {pressure} hPa";
                        if (humidityText != null)
                            humidityText.text = $"Humidity: {humidity} %";
                        if (airQualityText != null)
                            airQualityText.text = $"Air Quality: {airQuality} ppm";

                        Debug.Log($"Updated Environment Data - Temperature: {temperature} °C, Pressure: {pressure} hPa, Humidity: {humidity} %, Air Quality: {airQuality} ppm");
                    });
                }
                else
                {
                    Debug.LogWarning($"Failed to parse environment values: {inputString}");
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
        poller?.Stop();
        poller?.Dispose();
        subscriberSocketPort0?.Close();
        subscriberSocketPort0?.Dispose();
        subscriberSocketPort1?.Close();
        subscriberSocketPort1?.Dispose();
        NetMQConfig.Cleanup(false);
        Debug.Log("Cleanup completed for CombinedSensorHandler.");
    }
}
