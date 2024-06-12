using UnityEngine;

public class Y_move_Yellow : MonoBehaviour
{
    public float minY = 2.3f;
    public float maxY = 2.56f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay y_yellow;

    void Update()
    {
        if ((y_yellow.Printer2State == "Printing" || y_yellow.Printer2State == "Printing from SD") && (y_yellow.Printer2Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newX = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(currentPosition.x, currentPosition.y, newX);
        }
    }
}

