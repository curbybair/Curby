using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using TMPro;

public class ZMQSubscriber : MonoBehaviour
{
    private SubscriberSocket subscriberSocket;
    private Thread receiveThread;
    private bool running = false;

    // Use TextMeshProUGUI for UI TextMeshPro elements
    public TextMeshProUGUI temperatureTextUI;
    public TextMeshProUGUI pressureTextUI;
    public TextMeshProUGUI humidityTextUI;
    public TextMeshProUGUI airQualityTextUI;

    // Additional TextMeshProUGUI for second topic
    public TextMeshProUGUI yellowTemperatureTextUI;
    public TextMeshProUGUI yellowPressureTextUI;
    public TextMeshProUGUI yellowHumidityTextUI;
    public TextMeshProUGUI yellowAirQualityTextUI;

    void Start()
    {
        Debug.Log("Starting ZMQSubscriber...");
        ConnectToZMQ();
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

            // Subscribe to the topics
            string topic1 = "Port0_Brown_Temp";
            string topic2 = "Port3_Yellow_Temp";
            subscriberSocket.Subscribe(topic1);
            subscriberSocket.Subscribe(topic2);

            Debug.Log("Connected to ZMQ server and subscribed to topics: " + topic1 + " and " + topic2);

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
                if (subscriberSocket.TryReceiveFrameString(out string message))
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
        // Check the topic from the message and process accordingly
        if (message.StartsWith("Port0_Brown_Temp"))
        {
            ProcessPort0Message(message);
        }
        else if (message.StartsWith("Port3_Yellow_Temp"))
        {
            ProcessPort3Message(message);
        }
        else
        {
            Debug.LogWarning("Received message with unknown topic.");
        }
    }

    void ProcessPort0Message(string message)
    {
        // Split the message to get the data part
        string[] parts = message.Split('%');
        if (parts.Length > 1)
        {
            string inputString = parts[1];
            Debug.Log("Extracted string: " + inputString);

            // Clean and parse the string
            string cleanString = inputString.Replace("'", "").Replace(" ", "").Replace("[", "").Replace("]", "");
            Debug.Log("Parsed list: " + cleanString);

            string[] stringArray = cleanString.Split(',');

            // Convert each component to a float
            List<float> decimalList = new List<float>();
            foreach (string item in stringArray)
            {
                if (float.TryParse(item, out float value))
                {
                    decimalList.Add(value);
                }
            }

            if (decimalList.Count >= 4)
            {
                float temperature = decimalList[0];
                float pressure = decimalList[1];
                float humidity = decimalList[2];
                float airQuality = decimalList[3];

                Debug.Log($"Updating UI with values - Temperature: {temperature}, Pressure: {pressure}, Humidity: {humidity}, Air Quality: {airQuality}");

                // Update UI elements on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (temperatureTextUI != null) temperatureTextUI.text = $"Temperature (°C): {temperature}";
                    if (pressureTextUI != null) pressureTextUI.text = $"Pressure (InchHg): {pressure}";
                    if (humidityTextUI != null) humidityTextUI.text = $"Humidity (g/kg): {humidity}";
                    if (airQualityTextUI != null) airQualityTextUI.text = $"Air Quality (kΩ): {airQuality}";
                });
            }
            else
            {
                Debug.LogWarning("Received data does not contain enough elements.");
            }
        }
        else
        {
            Debug.LogWarning("Received message format is incorrect.");
        }
    }

    void ProcessPort3Message(string message)
    {
        // Split the message to get the data part
        string[] parts = message.Split('%');
        if (parts.Length > 1)
        {
            string inputString = parts[1];
            Debug.Log("Extracted string: " + inputString);

            // Clean and parse the string
            string cleanString = inputString.Replace("'", "").Replace(" ", "").Replace("[", "").Replace("]", "");
            Debug.Log("Parsed list: " + cleanString);

            string[] stringArray = cleanString.Split(',');

            // Convert each component to a float
            List<float> decimalList = new List<float>();
            foreach (string item in stringArray)
            {
                if (float.TryParse(item, out float value))
                {
                    decimalList.Add(value);
                }
            }

            if (decimalList.Count >= 4)
            {
                float temperature = decimalList[0];
                float pressure = decimalList[1];
                float humidity = decimalList[2];
                float airQuality = decimalList[3];

                Debug.Log($"Updating Yellow UI with values - Temperature: {temperature}, Pressure: {pressure}, Humidity: {humidity}, Air Quality: {airQuality}");

                // Update UI elements on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (yellowTemperatureTextUI != null) yellowTemperatureTextUI.text = $"Temperature (°C): {temperature}";
                    if (yellowPressureTextUI != null) yellowPressureTextUI.text = $"Pressure (InchHg): {pressure}";
                    if (yellowHumidityTextUI != null) yellowHumidityTextUI.text = $"Humidity (g/kg): {humidity}";
                    if (yellowAirQualityTextUI != null) yellowAirQualityTextUI.text = $"Air Quality (kΩ): {airQuality}";
                });
            }
            else
            {
                Debug.LogWarning("Received data does not contain enough elements.");
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
