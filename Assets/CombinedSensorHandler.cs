using System;
using UnityEngine;
using TMPro;
using NetMQ;
using NetMQ.Sockets;

public class CombinedSensorHandler : MonoBehaviour
{
    private SubscriberSocket subscriberSocketPort0;
    private SubscriberSocket subscriberSocketPort1;
    private SubscriberSocket subscriberSocketPort2;
    private SubscriberSocket subscriberSocketPort3;
    private SubscriberSocket subscriberSocketPort4;
    private SubscriberSocket subscriberSocketPort5;
    private NetMQPoller poller;

    public GameObject extruder; // GameObject representing the extruder
    public GameObject bed; // GameObject representing the bed
    public GameObject xLeadingRod; // GameObject representing the x leading rod
    public GameObject yellowExtruder; // GameObject representing the Yellow extruder
    public GameObject yellowBed; // GameObject representing the Yellow bed
    public GameObject yellowXLeadingRod; // GameObject representing the Yellow x leading rod
    
    public TMP_Text temperatureText;
    public TMP_Text pressureText;
    public TMP_Text humidityText;
    public TMP_Text airQualityText;
    public TMP_Text VibrationText;
    public TMP_Text temperatureTextYellow;
    public TMP_Text pressureTextYellow;
    public TMP_Text humidityTextYellow;
    public TMP_Text airQualityTextYellow;
    public TMP_Text VibrationTextYellow;

    public TMP_Text temperatureTextLCD;
    public TMP_Text pressureTextLCD;
    public TMP_Text humidityTextLCD;
    public TMP_Text airQualityTextLCD;
    public TMP_Text VibrationTextLCD;
    public TMP_Text temperatureTextYellowLCD;
    public TMP_Text pressureTextYellowLCD;
    public TMP_Text humidityTextYellowLCD;
    public TMP_Text airQualityTextYellowLCD;
    public TMP_Text VibrationTextYellowLCD;

    public float smoothSpeed = 0.125f; // Speed of interpolation
    public Vector3 bedOffset = Vector3.zero; // Public offset for the bed

    private Vector3 initialExtruderPosition;
    private Vector3 initialBedPosition;
    private Vector3 initialXLeadingRodPosition;

    private Vector3 targetExtruderPosition;
    private Vector3 targetBedPosition;
    private Vector3 targetXLeadingRodPosition;

    private Vector3 initialYellowExtruderPosition;
    private Vector3 initialYellowBedPosition;
    private Vector3 initialYellowXLeadingRodPosition;

    private Vector3 targetYellowExtruderPosition;
    private Vector3 targetYellowBedPosition;
    private Vector3 targetYellowXLeadingRodPosition;

    void Start()
    {
        temperatureTextYellowLCD = GameObject.Find("Temperature_LCD").GetComponent<TMP_Text>();
        Debug.Log("Temperature LCD reference: " + (temperatureTextYellowLCD != null ? "Found" : "Not Found"));
        pressureTextYellowLCD = GameObject.Find("Pressure_LCD").GetComponent<TMP_Text>();
        humidityTextYellowLCD = GameObject.Find("Humidity_LCD").GetComponent<TMP_Text>();
        airQualityTextYellowLCD = GameObject.Find("Air Quality_LCD").GetComponent<TMP_Text>();
        VibrationTextYellowLCD = GameObject.Find("Vibration_LCD").GetComponent<TMP_Text>();

        temperatureTextLCD = GameObject.Find("Temperature_LCD_Grey").GetComponent<TMP_Text>();
        Debug.Log("Temperature LCD reference: " + (temperatureTextLCD != null ? "Found" : "Not Found"));
        pressureTextLCD = GameObject.Find("Pressure_LCD_Grey").GetComponent<TMP_Text>();
        humidityTextLCD = GameObject.Find("Humidity_LCD_Grey").GetComponent<TMP_Text>();
        airQualityTextLCD = GameObject.Find("Air Quality_LCD_Grey").GetComponent<TMP_Text>();
        VibrationTextLCD = GameObject.Find("Vibration_LCD_Grey").GetComponent<TMP_Text>();

        //yellowExtruder = GameObject.Find("ExtruderYellow");
        //yellowBed = GameObject.Find("Printer Bed Yellow");
        //yellowXLeadingRod = GameObject.Find("X Leading Bar Yellow");
        temperatureTextYellow = GameObject.Find("Tempreture(°C) Yellow:").GetComponent<TMP_Text>();
        pressureTextYellow = GameObject.Find("Pressure(InchHg) Yellow:").GetComponent<TMP_Text>();
        humidityTextYellow = GameObject.Find("Humidity(g/kg) Yellow:").GetComponent<TMP_Text>();
        airQualityTextYellow = GameObject.Find("Air Quality(kΩ) Yellow:").GetComponent<TMP_Text>();
        VibrationTextYellow = GameObject.Find("Vibration Yellow:").GetComponent<TMP_Text>();
        VibrationText = GameObject.Find("Vibration:").GetComponent<TMP_Text>();

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
        if (yellowExtruder != null)
        {
            initialYellowExtruderPosition = yellowExtruder.transform.position;
            targetYellowExtruderPosition = initialYellowExtruderPosition;
        }
        else
        {
            Debug.LogError("Yellow extruder GameObject is not assigned!");
        }

        if (yellowBed != null)
        {
            initialYellowBedPosition = yellowBed.transform.position + bedOffset;
            targetYellowBedPosition = initialYellowBedPosition;
        }
        else
        {
            Debug.LogError("Yellow bed GameObject is not assigned!");
        }

        if (yellowXLeadingRod != null)
        {
            initialYellowXLeadingRodPosition = yellowXLeadingRod.transform.position;
            targetYellowXLeadingRodPosition = initialYellowXLeadingRodPosition;
        }
        else
        {
            Debug.LogError("Yellow xLeadingRod GameObject is not assigned!");
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
        
        if (yellowExtruder != null)
            yellowExtruder.transform.position = Vector3.Lerp(yellowExtruder.transform.position, targetYellowExtruderPosition, smoothSpeed * Time.deltaTime);

        if (yellowBed != null)
            yellowBed.transform.position = Vector3.Lerp(yellowBed.transform.position, targetYellowBedPosition, smoothSpeed * Time.deltaTime);

        if (yellowXLeadingRod != null)
            yellowXLeadingRod.transform.position = Vector3.Lerp(yellowXLeadingRod.transform.position, targetYellowXLeadingRodPosition, smoothSpeed * Time.deltaTime);
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
            subscriberSocketPort0.Connect("tcp://192.168.1.111:5556");
            subscriberSocketPort0.Subscribe("Port0_Grey_Position");
            Debug.Log("Subscribed to topic: Port0_Grey_Position");

            subscriberSocketPort1 = new SubscriberSocket();
            subscriberSocketPort1.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort1.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort1.Connect("tcp://192.168.1.111:5556");
            subscriberSocketPort1.Subscribe("Port1_Grey_Environment");
            Debug.Log("Subscribed to topic: Port1_Grey_Environment");

            subscriberSocketPort2 = new SubscriberSocket();
            subscriberSocketPort2.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort2.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort2.Connect("tcp://192.168.1.108:5556");
            subscriberSocketPort2.Subscribe("Port0_Yellow_Position");
            Debug.Log("Subscribed to topic: Port0_Yellow_Position");

            subscriberSocketPort3 = new SubscriberSocket();
            subscriberSocketPort3.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort3.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort3.Connect("tcp://192.168.1.108:5556");
            subscriberSocketPort3.Subscribe("Port1_Yellow_Environment");
            Debug.Log("Subscribed to topic: Port1_Yellow_Environment");

            subscriberSocketPort4 = new SubscriberSocket();
            subscriberSocketPort4.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort4.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort4.Connect("tcp://192.168.1.111:5556");
            subscriberSocketPort4.Subscribe("Port2_Grey_Vibration");
            Debug.Log("Subscribed to topic: Port2_Grey_Vibration");

            subscriberSocketPort5 = new SubscriberSocket();
            subscriberSocketPort5.Options.ReceiveHighWatermark = 1000;
            subscriberSocketPort5.Options.Linger = TimeSpan.Zero;
            subscriberSocketPort5.Connect("tcp://192.168.1.108:5556");
            subscriberSocketPort5.Subscribe("Port2_Yellow_Vibration");
            Debug.Log("Subscribed to topic: Port2_Yellow_Vibration");





            // Initialize poller
            poller = new NetMQPoller { subscriberSocketPort0, subscriberSocketPort1, subscriberSocketPort2, subscriberSocketPort3, subscriberSocketPort4, subscriberSocketPort5 };

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

            subscriberSocketPort2.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port0_Yellow_Position: " + message);
                ProcessPositionMessage(message);
            };

            subscriberSocketPort3.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port1_Yellow_Environment: " + message);
                ProcessEnvironmentMessage(message);
            };

            subscriberSocketPort4.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port2_Grey_Vibration: " + message);
                ProcessVibrationMessage(message);
            };

            subscriberSocketPort5.ReceiveReady += (s, e) =>
            {
                string message = e.Socket.ReceiveFrameString();
                Debug.Log("Received message on Port2_Yellow_Vibration: " + message);
                ProcessVibrationMessage(message);
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

                    if (topic.Contains("Grey"))
                    {
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
                    else if (topic.Contains("Yellow"))
                    {
                        
                        // Yellow Printer

                        // Apply offsets to the Yellow printer data
                        xPositionMeters -= 0.20f; // Subtract  cm from X
                        yPositionMeters -= 0.28f; // Subtract  cm from Y
                        zPositionMeters -= 0.02f; // Subtract  cm from Z
                    

                        Vector3 newYellowExtruderPosition = initialYellowExtruderPosition + new Vector3(-xPositionMeters, -yPositionMeters, 0);
                        Vector3 newYellowBedPosition = initialYellowBedPosition + new Vector3(0, 0, zPositionMeters);
                        Vector3 newYellowXLeadingRodPosition = initialYellowXLeadingRodPosition + new Vector3(0, -yPositionMeters, 0);

                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            if (yellowExtruder != null)
                                targetYellowExtruderPosition = newYellowExtruderPosition;

                            if (yellowBed != null)
                                targetYellowBedPosition = newYellowBedPosition;

                            if (yellowXLeadingRod != null)
                                targetYellowXLeadingRodPosition = newYellowXLeadingRodPosition;

                            Debug.Log($"Updated Yellow Printer Positions - Extruder: {yellowExtruder.transform.position}, Bed: {yellowBed.transform.position}, xLeadingRod: {yellowXLeadingRod.transform.position}");
                        });
                    }
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

            // Process data for temperature, pressure, humidity, and air quality,
            if (stringArray.Length >= 4)
            {
                if (float.TryParse(stringArray[0], out float temperature) &&
                    float.TryParse(stringArray[1], out float pressure) &&
                    float.TryParse(stringArray[2], out float humidity) &&
                    float.TryParse(stringArray[3], out float airQuality))
                {
                    if (topic.Contains("Grey"))
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
                            if (temperatureTextLCD != null)
                                temperatureTextLCD.text = $"Temperature: {temperature} °C";
                            if (pressureTextLCD != null)
                                pressureTextLCD.text = $"Pressure: {pressure} hPa";
                            if (humidityTextLCD != null)
                                humidityTextLCD.text = $"Humidity: {humidity} %";
                            if (airQualityTextLCD != null)
                                airQualityTextLCD.text = $"Air Quality: {airQuality} ppm";

                            Debug.Log($"Updated Environment Data - Temperature: {temperature} °C, Pressure: {pressure} hPa, Humidity: {humidity} %, Air Quality: {airQuality} ppm");
                        });
                    }
                    else if (topic.Contains("Yellow"))
                    {
                        // Update UI text fields for Yellow printer
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            if (temperatureTextYellow != null)
                                temperatureTextYellow.text = $"Temperature: {temperature} °C";
                            if (pressureTextYellow != null)
                                pressureTextYellow.text = $"Pressure: {pressure} hPa";
                            if (humidityTextYellow != null)
                                humidityTextYellow.text = $"Humidity: {humidity} %";
                            if (airQualityTextYellow != null)
                                airQualityTextYellow.text = $"Air Quality: {airQuality} ppm";
                            if (temperatureTextYellowLCD != null)
                                temperatureTextYellowLCD.text = $"Temperature: {temperature} °C";
                            if (pressureTextYellowLCD != null)
                                pressureTextYellowLCD.text = $"Pressure: {pressure} hPa";
                            if (humidityTextYellowLCD != null)
                                humidityTextYellowLCD.text = $"Humidity: {humidity} %";
                            if (airQualityTextYellowLCD != null)
                                airQualityTextYellowLCD.text = $"Air Quality: {airQuality} ppm";
                            Debug.Log($"Updated Yellow Environment Data - Temperature: {temperature} °C, Pressure: {pressure} hPa, Humidity: {humidity} %, Air Quality: {airQuality} ppm");
                        });
                    }
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

    void ProcessVibrationMessage(string message)
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

            // Process data for vibration, x, y, and z
            if (stringArray.Length >= 3)
            {
                if (float.TryParse(stringArray[0], out float x) &&
                    float.TryParse(stringArray[1], out float y) &&
                    float.TryParse(stringArray[2], out float z))
                {
                    // Update UI text fields on the main thread
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (topic.Contains("Grey"))
                        {
                            if (VibrationText != null)
                                VibrationText.text = $"Vibration: X: {x}, Y: {y}, Z: {z}";
                            if (VibrationTextLCD != null)
                                VibrationTextLCD.text = $"Vibration: X: {x}, Y: {y}, Z: {z}";
                        }
                        else if (topic.Contains("Yellow"))
                        {
                            if (VibrationTextYellow != null)
                                VibrationTextYellow.text = $"Vibration: X: {x}, Y: {y}, Z: {z}";
                            if (VibrationTextYellowLCD != null)
                                VibrationTextYellowLCD.text = $"Vibration: X: {x}, Y: {y}, Z: {z}";
                        }
                        Debug.Log($"Updated Vibration Data - Topic: {topic}, X: {x}, Y: {y}, Z: {z}");
                    });
                }
                else
                {
                    Debug.LogWarning($"Failed to parse vibration values: {inputString}");
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
        subscriberSocketPort2?.Close();
        subscriberSocketPort2?.Dispose();
        subscriberSocketPort3?.Close();
        subscriberSocketPort3?.Dispose();
        subscriberSocketPort4?.Close();
        subscriberSocketPort4?.Dispose();
        subscriberSocketPort5?.Close();
        subscriberSocketPort5?.Dispose();
        NetMQConfig.Cleanup(false);
        Debug.Log("Cleanup completed for CombinedSensorHandler.");
    }
}