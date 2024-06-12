using UnityEngine;

public class X_move_Brown : MonoBehaviour
{
    public float minY = 2.3f;
    public float maxY = 2.56f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay y_yellow;

    void Update()
    {
        if ((y_yellow.Printer1State == "Printing" || y_yellow.Printer1State == "Printing from SD") && (y_yellow.Printer1Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newX = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
        }
    }
}

