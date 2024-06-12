using UnityEngine;

public class Y_move_Brown : MonoBehaviour
{
    public float minY = 2.3f;
    public float maxY = 2.56f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay z_yellow;



    void Update()
    {
        if ((z_yellow.Printer1State == "Printing" || z_yellow.Printer1State == "Printing from SD") && (z_yellow.Printer1Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newZ = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(currentPosition.x, currentPosition.y, newZ);
        }
    }
}
