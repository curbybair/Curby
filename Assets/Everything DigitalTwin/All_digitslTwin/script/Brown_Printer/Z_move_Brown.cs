using UnityEngine;

public class Z_move_Brown : MonoBehaviour
{
    public float minY = -0.13f;
    public float maxY = .02f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay z_brown;



    void Update()
    {
        if ((z_brown.Printer1State == "Printing" || z_brown.Printer1State == "Printing from SD") && (z_brown.Printer1Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newZ = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(currentPosition.x, newZ, currentPosition.z);
        }
    }
}
