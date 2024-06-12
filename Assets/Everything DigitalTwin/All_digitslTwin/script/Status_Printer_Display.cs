using UnityEngine;

public class Status_Printer_Display : MonoBehaviour
{
    public TextMesh textMesh1;
    public TextMesh textMesh2;
    public OctoPrintJobDisplay octoPrintJobDisplay;

    private void Update()
    {
        if (octoPrintJobDisplay != null)
        {
            textMesh1.text = "Status:\n" + octoPrintJobDisplay.Printer1State;
            textMesh2.text = "Status:\n" + octoPrintJobDisplay.Printer2State;
        }
        else
        {
            textMesh1.text = "Status: N/A";
            textMesh2.text = "Status: N/A";
        }
    }
}
