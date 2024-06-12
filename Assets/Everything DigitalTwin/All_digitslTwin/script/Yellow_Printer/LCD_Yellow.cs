using UnityEngine;

public class LCD_Yellow : MonoBehaviour
{
   
    public TextMesh textMesh2;

    public OctoPrint_Yellow Temp2;
    

    private void Update()
    {
      
        if (Temp2 != null)
        {
            float bedTemp = Temp2.BedActualTemperature;
            float toolTemp = Temp2.ToolActualTemperature;


            textMesh2.text = "Bed: " + bedTemp.ToString("F2") + "\n" + "Head: " + toolTemp.ToString("F2");
        }
        else
        {
            textMesh2.text = "Bed: N/A\nHead: N/A";
        }
    }
}



