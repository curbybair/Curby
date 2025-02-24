using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using System.Net.Http;

public class ComparingGraph : MonoBehaviour
{
    public LineChart xAxisChart, yAxisChart, zAxisChart;
    public float updateInterval = 1.0f;  // Update interval for graphs
    private SubscriberSocket subscriberSocket0;
    private SubscriberSocket subscriberSocket1;
    private Thread listenerThread;
    private HttpClient httpClient = new HttpClient();

    // Store data for the graphs
    private List<float> greyPrinterXData = new List<float>();
    private List<float> greyPrinterYData = new List<float>();
    private List<float> greyPrinterZData = new List<float>();
    private List<float> yellowPrinterXData = new List<float>();
    private List<float> yellowPrinterYData = new List<float>();
    private List<float> yellowPrinterZData = new List<float>();

    private List<float> octoGreyXData = new List<float>();
    private List<float> octoGreyYData = new List<float>();
    private List<float> octoGreyZData = new List<float>();
    private List<float> octoYellowXData = new List<float>();
    private List<float> octoYellowYData = new List<float>();
    private List<float> octoYellowZData = new List<float>();

    // OctoPrint endpoints
    private string printer1APIKey = "5D44B0739CB04110871B24C202512986";
    private string printer2APIKey = "D8F7EE4BAA8C4E67A41FFA537FC91C0B";
    private string printer1URL = "http://192.168.1.103/api/job";
    private string printer2URL = "http://192.168.1.105/api/job";

    void Start()
    {
        // Initialize NetMQ subscriber for Raspberry Pi data
        InitializeNetMQSubscriber();

        // Start listening for sensor data
        listenerThread = new Thread(ReceiveNetMQMessages);
        listenerThread.Start();

        // Start fetching OctoPrint data
        InvokeRepeating("FetchOctoPrintData", 0, updateInterval);

        // Start updating the graphs
        InvokeRepeating("UpdateGraphs", 0, updateInterval);
    }

    void InitializeNetMQSubscriber()
    {
        try
        {
            subscriberSocket0 = new SubscriberSocket();
            subscriberSocket0.Connect("tcp://192.168.1.111:5556"); // Raspberry Pi address
            subscriberSocket0.Subscribe("Port0_Grey_Position");

            subscriberSocket1 = new SubscriberSocket();
            subscriberSocket1.Connect("tcp://192.168.1.109:5556");
            subscriberSocket1.Subscribe("Port1_Yellow_Position");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize NetMQ Subscriber: {ex.Message}");
        }
    }

    void ReceiveNetMQMessages()
    {
        while (true)
        {
            var message = subscriberSocket0.ReceiveFrameString();
            ParseAndStoreSensorData(message);
        }
    }

    void ParseAndStoreSensorData(string message)
    {
        string[] messageParts = message.Split('%');
        string topic = messageParts[0];
        string[] data = messageParts[1].Split(',');

        if (topic == "Port0_Grey_Position")
        {
            greyPrinterXData.Add(float.Parse(data[0]));
            greyPrinterYData.Add(float.Parse(data[1]));
            greyPrinterZData.Add(float.Parse(data[2]));
        }
        else if (topic == "Port1_Yellow_Position")
        {
            yellowPrinterXData.Add(float.Parse(data[0]));
            yellowPrinterYData.Add(float.Parse(data[1]));
            yellowPrinterZData.Add(float.Parse(data[2]));
        }
    }

    async void FetchOctoPrintData()
    {
        await FetchDataFromOctoPrint(printer1URL, printer1APIKey, "grey");
        await FetchDataFromOctoPrint(printer2URL, printer2APIKey, "yellow");
    }

    async System.Threading.Tasks.Task FetchDataFromOctoPrint(string printerURL, string apiKey, string printer)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, printerURL);
            request.Headers.Add("X-Api-Key", apiKey);
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string jsonData = await response.Content.ReadAsStringAsync();
                ParseAndStoreOctoPrintData(jsonData, printer);
            }
            else
            {
                Debug.LogError($"Failed to fetch OctoPrint data for {printer}. Status code: {response.StatusCode}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error fetching OctoPrint data for {printer}: {ex.Message}");
        }
    }

    void ParseAndStoreOctoPrintData(string jsonData, string printer)
    {
        // Parse using Unity's built-in JsonUtility
        OctoPrintJobData jobData = JsonUtility.FromJson<OctoPrintJobData>(jsonData);

        if (printer == "grey")
        {
            octoGreyXData.Add(jobData.job.position.x);
            octoGreyYData.Add(jobData.job.position.y);
            octoGreyZData.Add(jobData.job.position.z);
        }
        else if (printer == "yellow")
        {
            octoYellowXData.Add(jobData.job.position.x);
            octoYellowYData.Add(jobData.job.position.y);
            octoYellowZData.Add(jobData.job.position.z);
        }
    }

    void UpdateGraphs()
    {
        // Update X Axis Graph
        xAxisChart.UpdateChart(new List<List<float>>() {
            greyPrinterXData, octoGreyXData, yellowPrinterXData, octoYellowXData });

        // Update Y Axis Graph
        yAxisChart.UpdateChart(new List<List<float>>() {
            greyPrinterYData, octoGreyYData, yellowPrinterYData, octoYellowYData });

        // Update Z Axis Graph
        zAxisChart.UpdateChart(new List<List<float>>() {
            greyPrinterZData, octoGreyZData, yellowPrinterZData, octoYellowZData });
    }

    void OnDestroy()
    {
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
        subscriberSocket0.Close();
        subscriberSocket1.Close();
        httpClient.Dispose();
    }

    // Classes for Unity's JsonUtility
    [System.Serializable]
    public class OctoPrintJobData
    {
        public JobData job;
    }

    [System.Serializable]
    public class JobData
    {
        public PositionData position;
    }

    [System.Serializable]
    public class PositionData
    {
        public float x;
        public float y;
        public float z;
    }
}