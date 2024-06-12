using UnityEngine;

public class X_move_Yellow : MonoBehaviour
{
    public float minY = 2.3f;
    public float maxY = 2.56f;
    public float moveSpeed = .25f;
    public OctoPrintJobDisplay x_yellow;
 
    void Update()
    {
        if ((x_yellow.Printer2State == "Printing" || x_yellow.Printer2State == "Printing from SD") && (x_yellow.Printer2Progress != null))
        {
            Vector3 currentPosition = transform.position;
            float newX = Mathf.PingPong(Time.time * moveSpeed, maxY - minY) + minY;
            transform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
        }
       
    }
}



