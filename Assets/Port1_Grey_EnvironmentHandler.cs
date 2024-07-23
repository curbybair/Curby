using UnityEngine;
using TMPro;
public class Port1_Grey_EnvironmentHandler : MonoBehaviour
{
    public TMP_Text temperatureText;
    public TMP_Text pressureText;
    public TMP_Text humidityText;
    public TMP_Text airQualityText;

    //private ZMQSubscriber zmqSubscriber;

    void Start()
    {
       // zmqSubscriber = gameObject.AddComponent<ZMQSubscriber>();
        //zmqSubscriber.Initialize("tcp://192.168.1.111:5555", "Port1_Grey_Environment", ProcessMessage);
    }

    void ProcessMessage(string message)
    {
        string[] parts = message.Split('%');
        if (parts.Length > 1)
        {
            string inputString = parts[1];
            string cleanString = inputString.Replace("'", "").Replace(" ", "").Replace("[", "").Replace("]", "");
            string[] stringArray = cleanString.Split(',');

            if (stringArray.Length >= 4)
            {
                if (float.TryParse(stringArray[0], out float temperature) &&
                    float.TryParse(stringArray[1], out float pressure) &&
                    float.TryParse(stringArray[2], out float humidity) &&
                    float.TryParse(stringArray[3], out float airQuality))
                {
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
                    });
                }
            }
        }
    }
}