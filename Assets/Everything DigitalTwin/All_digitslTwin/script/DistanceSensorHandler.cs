using UnityEngine;

public class DistanceSensorHandler : MonoBehaviour
{
    public GameObject extruder;
    public GameObject bed;
    public GameObject xLeadingRod;
    public float smoothSpeed = 0.125f;
    public Vector3 bedOffset = Vector3.zero;

    private Vector3 initialExtruderPosition;
    private Vector3 initialBedPosition;
    private Vector3 initialXLeadingRodPosition;

    private Vector3 targetExtruderPosition;
    private Vector3 targetBedPosition;
    private Vector3 targetXLeadingRodPosition;

    private ZMQSubscriber zmqSubscriber;

    void Start()
    {
        zmqSubscriber = gameObject.AddComponent<ZMQSubscriber>();

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

        zmqSubscriber.Initialize("tcp://192.168.1.111:5555", "Port0_Grey_Position", ProcessMessage);
    }

    void Update()
    {
        if (extruder != null)
            extruder.transform.position = Vector3.Lerp(extruder.transform.position, targetExtruderPosition, smoothSpeed * Time.deltaTime);

        if (bed != null)
            bed.transform.position = Vector3.Lerp(bed.transform.position, targetBedPosition, smoothSpeed * Time.deltaTime);

        if (xLeadingRod != null)
            xLeadingRod.transform.position = Vector3.Lerp(xLeadingRod.transform.position, targetXLeadingRodPosition, smoothSpeed * Time.deltaTime);
    }

    void ProcessMessage(string message)
    {
        string[] parts = message.Split('%');
        if (parts.Length > 1)
        {
            string inputString = parts[1];
            string cleanString = inputString.Replace("'", "").Replace(" ", "").Replace("[", "").Replace("]", "");
            string[] stringArray = cleanString.Split(',');

            if (stringArray.Length >= 3)
            {
                if (float.TryParse(stringArray[0], out float xPositionCm) &&
                    float.TryParse(stringArray[1], out float zPositionCm) &&
                    float.TryParse(stringArray[2], out float yPositionCm))
                {
                    float xPositionMeters = xPositionCm / 100.0f;
                    float zPositionMeters = zPositionCm / 100.0f;
                    float yPositionMeters = yPositionCm / 100.0f;

                    Vector3 newExtruderPosition = initialExtruderPosition + new Vector3(-xPositionMeters, -yPositionMeters, 0);
                    Vector3 newBedPosition = initialBedPosition + new Vector3(0, 0, zPositionMeters);
                    Vector3 newXLeadingRodPosition = initialXLeadingRodPosition + new Vector3(0, -yPositionMeters, 0);

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (extruder != null)
                            targetExtruderPosition = newExtruderPosition;

                        if (bed != null)
                            targetBedPosition = newBedPosition;

                        if (xLeadingRod != null)
                            targetXLeadingRodPosition = newXLeadingRodPosition;
                    });
                }
            }
        }
    }
}
