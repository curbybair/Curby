using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ViveSR.anipal.Eye;

public class GazeRecorder : MonoBehaviour
{
    public string fileName;
    private string currentObjectName = "";
    private float gazeStartTime = 0f;
    private Dictionary<string, float> gazeDurations = new Dictionary<string, float>();

    public LayerMask gazeLayerMask;

    private VerboseData eyeVerboseData;  // Holds detailed eye-tracking data
    private Vector3 previousGazeDirection = Vector3.zero;  // For gaze velocity calculation
    private float lastBlinkTime = 0f;                      // For blink rate calculation
    private int blinkCount = 0;

    void Start()
    {
        // Ensure SRanipal is initialized
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            Debug.LogError("SRanipal Eye Module is not enabled!");
            return;
        }

        Debug.Log("SRanipal Eye Framework initialized successfully.");

        // Set the fileName to the Downloads folder
        string downloadsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
        fileName = Path.Combine(downloadsPath, "GazeData.csv");

        Debug.Log($"Gaze data will be stored in: {fileName}");

        // Create the file and write the header
        using (StreamWriter sw = new StreamWriter(fileName, false))
        {
            sw.WriteLine("ObjectName,StartTime,EndTime,Duration,PupilSize,BlinkRate,GazeVelocity,SaccadeVelocity");
        }

        // Retrieve layer indices
        int handsLayer = LayerMask.NameToLayer("Hands");
        int teleportableLayer = LayerMask.NameToLayer("Teleportable");
        int noEyeLayer = LayerMask.NameToLayer("No Eye");

        int excludedLayers = (1 << handsLayer) | (1 << teleportableLayer) | (1 << noEyeLayer);
        gazeLayerMask = ~excludedLayers;

        // Ensure SRanipal is initialized
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            Debug.LogError("SRanipal Eye Module is not enabled!");
        }
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Get eye tracking data
        if (SRanipal_Eye.GetVerboseData(out eyeVerboseData))
        {
            // Pupil size (validate and calculate)
            float leftPupilDiameter = eyeVerboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) 
                ? eyeVerboseData.left.pupil_diameter_mm : 0f;

            float rightPupilDiameter = eyeVerboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) 
                ? eyeVerboseData.right.pupil_diameter_mm : 0f;

            float pupilSize = (leftPupilDiameter > 0 && rightPupilDiameter > 0) 
                ? (leftPupilDiameter + rightPupilDiameter) / 2.0f 
                : -1f;

            bool isLeftPupilValid = eyeVerboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY);
            bool isRightPupilValid = eyeVerboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY);

            //Debug.Log($"Left Pupil Valid: {eyeVerboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY)}, " + $"Right Pupil Valid: {eyeVerboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY)}");
            
            // Blink detection
            bool isBlinking = eyeVerboseData.left.eye_openness < 0.1f && eyeVerboseData.right.eye_openness < 0.1f;
            if (isBlinking && Time.time - lastBlinkTime > 0.2f)
            {
                blinkCount++;
                lastBlinkTime = Time.time;
            }
            float blinkRate = blinkCount / Mathf.Max(Time.time, 1f); // Avoid division by zero

            // Gaze velocity
            Vector3 currentGazeDirection = eyeVerboseData.combined.eye_data.gaze_direction_normalized;
            float gazeVelocity = (currentGazeDirection - previousGazeDirection).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            previousGazeDirection = currentGazeDirection;

            // Saccade velocity
            float saccadeVelocity = gazeVelocity > 3f ? gazeVelocity : 0f; // Threshold for saccades

            // Cast the ray and ignore objects in excluded layers
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gazeLayerMask))
            {
                string hitObjectName = hit.collider.gameObject.name;

                if (hitObjectName != currentObjectName)
                {
                    if (currentObjectName != "")
                    {
                        float gazeEndTime = Time.time;
                        float gazeDuration = gazeEndTime - gazeStartTime;

                        // Log gaze data
                        LogGazeData(currentObjectName, gazeStartTime, gazeEndTime, gazeDuration, pupilSize, blinkRate, gazeVelocity, saccadeVelocity);
                    }

                    currentObjectName = hitObjectName;
                    gazeStartTime = Time.time;

                    Debug.Log($"Started tracking: {currentObjectName}");
                }
            }
            else
            {
                if (currentObjectName != "")
                {
                    float gazeEndTime = Time.time;
                    float gazeDuration = gazeEndTime - gazeStartTime;

                    LogGazeData(currentObjectName, gazeStartTime, gazeEndTime, gazeDuration, pupilSize, blinkRate, gazeVelocity, saccadeVelocity);
                    Debug.Log($"Stopped tracking: {currentObjectName}");
                    currentObjectName = "";
                }
            }
        }
        else
        {
            Debug.LogWarning("Failed to retrieve eye data.");
        }
        


    }
    


    void LogGazeData(string objectName, float startTime, float endTime, float duration, float pupilSize, float blinkRate, float gazeVelocity, float saccadeVelocity)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                sw.WriteLine($"{objectName},{startTime},{endTime},{duration},{pupilSize},{blinkRate},{gazeVelocity},{saccadeVelocity}");
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Failed to write to file: {ex.Message}");
        }
    }
}