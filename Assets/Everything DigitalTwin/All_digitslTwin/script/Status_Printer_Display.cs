using UnityEngine;

public class Status_Printer_Display : MonoBehaviour
{
    public TextMesh Status_text_for_Brown_Printer;
    public TextMesh Status_text_for_Yellow_Printer;
    public OctoPrintJobDisplay octoPrintJobDisplay;

    private void Update()
    {
        if (octoPrintJobDisplay != null)
        {
            Status_text_for_Brown_Printer.text = "Status:\n" + octoPrintJobDisplay.Printer1State;
            Status_text_for_Yellow_Printer.text = "Status:\n" + octoPrintJobDisplay.Printer2State;
        }
        else
        {
            Status_text_for_Brown_Printer.text = "Status: N/A";
            Status_text_for_Yellow_Printer.text = "Status: N/A";
        }
    }
}
