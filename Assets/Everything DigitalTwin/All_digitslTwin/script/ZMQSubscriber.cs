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

    void Start()
    {
        ConnectToZMQ();
    }

    void ConnectToZMQ()
    {
        try
        {
            // Initialize the subscriber socket
            subscriberSocket = new SubscriberSocket();
            subscriberSocket.Options.ReceiveHighWatermark = 1000;
            subscriberSocket.Options.Linger = TimeSpan.Zero;

            // Connect to the ZMQ server
            string raspberryPiIp = "tcp://192.168.1.111:5555";  // Replace with your server address
            subscriberSocket.Connect(raspberryPiIp);

            // Subscribe to the topic
            string topic = "Port0_Brown_Temp";
            subscriberSocket.Subscribe(topic);

            Debug.Log("Connected to ZMQ server and subscribed to topic: " + topic);

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
        var poller = new NetMQPoller { subscriberSocket };
        subscriberSocket.ReceiveReady += OnReceiveReady;
        poller.Run();
    }

    void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        if (subscriberSocket.TryReceiveFrameString(out string message))
        {
            Debug.Log("Received message: " + message);

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

                    // Update UI elements on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (temperatureTextUI != null) temperatureTextUI.text = $"Temperature: {temperature}";
                        if (pressureTextUI != null) pressureTextUI.text = $"Pressure: {pressure}";
                        if (humidityTextUI != null) humidityTextUI.text = $"Humidity: {humidity}";
                        if (airQualityTextUI != null) airQualityTextUI.text = $"Air Quality: {airQuality}";
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
        else
        {
            Debug.Log("No message received within timeout period.");
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