using UnityEngine;

public class Z_move_Yellow : MonoBehaviour
{
    public float minY = 2.3f;
    public float maxY = 2.56f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay z_yellow;
    


    void Update()
    {
        if ((z_yellow.Printer2State == "Printing" || z_yellow.Printer2State == "Printing from SD") && (z_yellow.Printer2Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newZ = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(currentPosition.x, newZ, currentPosition.z);
        }
    }
}

