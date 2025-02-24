using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Logs eye tracking data from OVRPlugin to a CSV hfile.
/// </summary>
public class EyeTrackingLogger : MonoBehaviour
{
    public string LogFileName = "EyeTrackingLog.csv"; // File to save logs
    public float LogInterval = 0.1f; // Time between log entries in seconds

    private float _nextLogTime;
    private StreamWriter _csvWriter;
    private bool _isLogging;

    void Start()
    {
        // Automatically start logging when the game starts
        StartLogging();
    }

    void Update()
    {
        // Log data periodically based on LogInterval
        if (_isLogging && Time.time >= _nextLogTime)
        {
            LogEyeData();
            _nextLogTime = Time.time + LogInterval;
        }
    }

    void OnDestroy()
    {
        // Stop logging and clean up resources when the object is destroyed
        StopLogging();
    }

    /// <summary>
    /// Starts logging eye-tracking data to the CSV file.
    /// </summary>
    private void StartLogging()
    {
        if (!OVRPlugin.eyeTrackingEnabled)
        {
            Debug.LogError("Eye tracking is not enabled. Ensure it is configured in the Oculus settings.");
            return;
        }

        try
        {
            _csvWriter = new StreamWriter(LogFileName);
            _csvWriter.WriteLine("Timestamp,Eye,Confidence,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW");
            _isLogging = true;
            Debug.Log($"Eye tracking logging started. Writing to: {LogFileName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start logging: {e.Message}");
        }
    }

    /// <summary>
    /// Stops logging and closes the CSV file.
    /// </summary>
    private void StopLogging()
    {
        if (_csvWriter != null)
        {
            _csvWriter.Close();
            _csvWriter = null;
        }

        _isLogging = false;
        Debug.Log("Eye tracking logging stopped.");
    }

    /// <summary>
    /// Logs the current eye-tracking data.
    /// </summary>
    private void LogEyeData()
    {
        var eyeGazesState = new OVRPlugin.EyeGazesState();
        if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref eyeGazesState))
        {
            Debug.LogWarning("Failed to retrieve eye-gaze state.");
            return;
        }

        foreach (OVRPlugin.Eye eye in Enum.GetValues(typeof(OVRPlugin.Eye)))
        {
            var eyeGaze = eyeGazesState.EyeGazes[(int)eye];

            if (!eyeGaze.IsValid || eyeGaze.Confidence < 0.5f) // Adjust confidence threshold as needed
            {
                continue;
            }

            var position = eyeGaze.Pose.Position;
            var rotation = eyeGaze.Pose.Orientation;
            string logEntry = $"{Time.time},{eye},{eyeGaze.Confidence},{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w}";

            _csvWriter.WriteLine(logEntry);
        }

        _csvWriter.Flush(); // Ensure data is written to the file immediately
    }
}
