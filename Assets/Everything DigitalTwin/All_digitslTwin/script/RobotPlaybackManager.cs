using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RobotPlaybackManager : MonoBehaviour
{
    public static RobotPlaybackManager Instance; // Singleton instance

    public double[] CurrentJointAngles = new double[6]; // Current joint angles
    public List<double[]> JointData = new List<double[]>(); // Parsed joint data
    public List<double> Timestamps = new List<double>(); // Parsed timestamps

    private int currentFrame = 0; // Current playback frame
    private float elapsedTime = 0.0f; // Timer for frame playback
    private bool isPlaying = false; // Playback state

    public string logFilePath = @"C:\Users\cbbair01\Downloads\Robot1MovementData.csv"; // Path to the log file
    public float playbackSpeedFactor = 10.0f; // Factor to adjust playback speed

    private bool hasCompleted = false; // Flag to track if animation is completed

    public GameObject completionPopup;

    // Timer-related fields
    private float animationStartTime = 0.0f; // Tracks when playback starts
    private float totalElapsedTime = 0.0f;  // Total time elapsed for the animation
    public TextMeshPro timeDisplay; // UI element to display the timer

    private void Awake()
    {
        Debug.Log("RobotPlaybackManager Awake called.");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadLogData(logFilePath); // Load log data on start

        if (completionPopup != null)
        {
            completionPopup.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (!isPlaying || JointData.Count == 0 || Timestamps.Count == 0)
            return;

        // Increment elapsed time scaled by playbackSpeedFactor
        elapsedTime += Time.fixedDeltaTime * playbackSpeedFactor;

        // Ensure we only advance frames that match the elapsed time
        while (currentFrame < Timestamps.Count - 1 && elapsedTime >= (Timestamps[currentFrame + 1] - Timestamps[currentFrame]))
        {
            elapsedTime -= (float)(Timestamps[currentFrame + 1] - Timestamps[currentFrame]);
            currentFrame++;
        }

        // Handle looping when reaching the end of the playback
        if (currentFrame == 3473 && !hasCompleted)
        {
            if (!hasCompleted)
            {
                hasCompleted = true;
                isPlaying = false;
                //Debug.Log("Animation complete, calling OnAnimationComplete.");
                totalElapsedTime = Time.time - animationStartTime; 
                OnAnimationComplete(); // Call the function when animation finishes
                return;
            }
            return; // Stop updating after completion
        }

        // Handle looping when reaching the end of the playback
        if (currentFrame >= JointData.Count)
        {
            currentFrame = 0; // Reset to the first frame
            elapsedTime = 0.0f; // Reset elapsed time to avoid leftover time
        }

        // Update robot's joint angles for the current frame
        CurrentJointAngles = JointData[currentFrame];

        if (timeDisplay != null && isPlaying)
        {
            float elapsedTimeSinceStart = Time.time - animationStartTime;
            timeDisplay.text = $"Elapsed Time: {elapsedTimeSinceStart:F2} seconds";
        }

        // Debug logs to verify playback behavior
        Debug.Log($"Playback Speed: {playbackSpeedFactor}, Current Frame: {currentFrame}, Elapsed Time: {elapsedTime}");
    }
    private void LoadLogData(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            {
                string header = reader.ReadLine(); // Read the header row
                var columns = header.Split(',');

                // Find the indices for timestamp, speed_scaling, and actual_q_0 to actual_q_5
                int timestampIndex = Array.IndexOf(columns, "timestamp");
                int speedIndex = Array.IndexOf(columns, "speed_scaling");
                int[] jointIndices = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    jointIndices[i] = Array.IndexOf(columns, $"actual_q_{i}");
                }

                string line;
                double scalingFactor = 2.0; // Increase the timestamp intervals by a factor of 2
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');

                    // Parse speed_scaling and filter out rows with speed_scaling <= 0
                    double speed = double.Parse(parts[speedIndex], CultureInfo.InvariantCulture);
                    if (speed <= 0)
                    {
                        continue;
                    }

                    // Scale timestamps
                    double timestamp = double.Parse(parts[timestampIndex], CultureInfo.InvariantCulture) * scalingFactor;

                    // Parse joint angles
                    double[] jointAngles = new double[6];
                    for (int i = 0; i < 6; i++)
                    {
                        jointAngles[i] = double.Parse(parts[jointIndices[i]], CultureInfo.InvariantCulture);
                    }

                    Timestamps.Add(timestamp);
                    JointData.Add(jointAngles);
                }
            }

            Debug.Log("Filtered log data loaded successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to load log data: " + ex.Message);
        }
    }

    public void TogglePlayback()
    {
        isPlaying = !isPlaying; // Toggle playback state
        
        if (isPlaying)
        {
            animationStartTime = Time.time; // Start the timer
            
        }
        else
        {
            totalElapsedTime = Time.time - animationStartTime; // Stop the timer
            
        }
    }

    // Call this method when the "Run Program" button is pressed
    public void OnRunProgramButtonPressed()
    {
        
        TogglePlayback(); // Start or stop playback
    }

    private void OnAnimationComplete()
    {
        Debug.Log("Part one has been placed!");

        if (timeDisplay != null)
        {
            timeDisplay.text = $"Elapsed Time: {totalElapsedTime:F2} seconds";
        }

        if (completionPopup != null)
        {
            // Enable the popup and set the message
            completionPopup.SetActive(true);

            // Find a Text component within the popup and set the text
            TMP_Text popupText = completionPopup.GetComponentInChildren<TMP_Text>();
            if (popupText != null)
            {
                popupText.text = "Congrats! Part one has been placed! Please disconnect from Robot 1.";
            }

            // Disable the popup after 7 seconds
            Invoke(nameof(HideCompletionPopup), 10f);
        }
        else
        {
            Debug.LogError("completionPopup is not assigned! Please assign a UI element in the Inspector.");
        }
        
    }

    // Function to hide the popup
    private void HideCompletionPopup()
    {
        if (completionPopup != null)
        {
            completionPopup.SetActive(false);
            Debug.Log("Popup hidden after 3 seconds.");
        }
    }
}
