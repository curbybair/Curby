using UnityEngine;

public class LCD_Brown : MonoBehaviour
{
    public TextMesh textMesh1;

    public OctoPrint_Brown Temp1;

    private void Update()
    {
        if (Temp1 != null)
        {
            float bedTemp = Temp1.BedActualTemperature;
            float toolTemp = Temp1.ToolActualTemperature;


            textMesh1.text = "Bed: " + bedTemp.ToString("F2") + "\n" + "Head: " + toolTemp.ToString("F2");
        }
        else
        {
            textMesh1.text = "Bed: N/A\nHead: N/A";
        }
    }
}