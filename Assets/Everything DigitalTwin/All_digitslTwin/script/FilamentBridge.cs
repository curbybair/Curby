using UnityEngine;

public class FilamentBridge : MonoBehaviour
{
    public Transform printerHead;  // Assign the printer head's Transform
    public Transform spool;        // Assign the spool's Transform
    private LineRenderer lineRenderer;

    void Start()
    {
        // Get the LineRenderer component on the current GameObject
        lineRenderer = GetComponent<LineRenderer>();

        // Set the number of positions (2 points for the line)
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        // Update the positions of the line renderer based on the printer head and spool positions
        if (printerHead != null && spool != null)
        {
            lineRenderer.SetPosition(0, printerHead.position); // Start at printer head
            lineRenderer.SetPosition(1, spool.position);       // End at spool
        }
    }
}
