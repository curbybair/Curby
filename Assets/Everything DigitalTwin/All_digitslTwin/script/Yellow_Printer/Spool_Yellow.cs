using UnityEngine;

public class Spool_Yellow : MonoBehaviour

{

    [SerializeField] Vector3 _rotation = new Vector3 ();
    [SerializeField] float speed;
    public OctoPrintJobDisplay spool_rotate;

    void Update()
    {
        if ((spool_rotate.Printer2State == "Printing" || spool_rotate.Printer2State == "Printing from SD") && (spool_rotate.Printer2Progress != null))
        {

            transform.Rotate(speed * _rotation * Time.deltaTime);
        }

    }
}

