using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using ViveSR.anipal.Lip;

public class FaceRecorder : MonoBehaviour
{
    private Dictionary<LipShape, float> lipWeightings = new Dictionary<LipShape, float>();
    private string filePath;

    void Start()
    {
        // Define the file path to the Downloads folder
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        filePath = Path.Combine(downloadsPath, "FacialTrackingData.json");
        Debug.Log($"Data will be saved to: {filePath}");
    }

    void Update()
    {
        // Check if lip tracking framework is working
        if (SRanipal_Lip_Framework.Status == SRanipal_Lip_Framework.FrameworkStatus.WORKING)
        {
            // Retrieve lip weightings
            bool success = SRanipal_Lip.GetLipWeightings(out lipWeightings);

            if (success)
            {
                WriteDataToFile();
            }
            else
            {
                Debug.LogWarning("Failed to retrieve lip weightings.");
            }
        }
    }

    private void WriteDataToFile()
    {
        // Convert the dictionary to a JSON-like string
        string jsonData = "{";
        foreach (var pair in lipWeightings)
        {
            jsonData += $"\"{pair.Key}\": {pair.Value}, ";
        }
        jsonData = jsonData.TrimEnd(',', ' ') + "}";

        // Append the JSON data to the file using StreamWriter
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(jsonData);
        }
    }
}

