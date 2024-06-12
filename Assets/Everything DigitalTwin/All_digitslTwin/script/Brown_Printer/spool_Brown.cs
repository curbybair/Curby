using UnityEngine;

public class spool_Brown : MonoBehaviour

{

    [SerializeField] Vector3 _rotation = new Vector3();
    [SerializeField] float speed;
    public OctoPrintJobDisplay spool_rotate;

    void Update()
    {
        if ((spool_rotate.Printer1State == "Printing" || spool_rotate.Printer1State == "Printing from SD") && (spool_rotate.Printer1Progress != null))
        {

            transform.Rotate(speed * _rotation * Time.deltaTime);
        }
        
    }
}

