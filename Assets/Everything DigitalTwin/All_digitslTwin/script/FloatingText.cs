using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Transform mainCam;
    private Quaternion originalRotation;

    void Start()
    {
        mainCam = Camera.main.transform;
        originalRotation = transform.rotation; // Store the original rotation of the text
    }

    void Update()
    {
        // Calculate the direction from the text to the camera
        Vector3 directionToCamera = mainCam.position - transform.position;

        // Calculate the rotation that faces the camera
        Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);

        // Maintain the original rotation for the y-axis
        lookRotation.eulerAngles = new Vector3(originalRotation.eulerAngles.x, lookRotation.eulerAngles.y, originalRotation.eulerAngles.z);

        // Apply the rotation to the text object
        transform.rotation = lookRotation;
    }
}
