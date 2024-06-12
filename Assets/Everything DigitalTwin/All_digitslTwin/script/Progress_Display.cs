using UnityEngine;

public class Progress_Display : MonoBehaviour
{
    public TextMesh textMesh1;
    public TextMesh textMesh2;
    public TextMesh textMesh3;
    public TextMesh textMesh4;

    public OctoPrintJobDisplay Progress_display;
    

    private void Update()
    {

        if (Progress_display != null)
        {
            textMesh1.text = "Printer 1\nStatus: " + Progress_display.Printer1State;
            textMesh2.text = "Printer 2 \nStatus: " + Progress_display.Printer2State;

            if ((Progress_display.Printer1State == "Printing")|| (Progress_display.Printer1State == "Printing from SD"))
            {
                textMesh3.text = "Time Elapsed: " + Progress_display.Printer1Progress + " s";
                
                

            }
            if ((Progress_display.Printer2State == "Printing") || (Progress_display.Printer2State == "Printing from SD"))
            {
                textMesh4.text = "Time Elapsed " + Progress_display.Printer2Progress + " s";
                
            }
        }
        else
        {
            textMesh1.text = "Printer 1 Status: " + Progress_display.Printer1State;
            textMesh2.text = "Printer 2 Status: " + Progress_display.Printer2State;
        }
        
    }
}
