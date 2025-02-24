using UnityEngine;
using TMPro;

public class LCD_Yellow : MonoBehaviour
{
   
    public TextMesh textMesh2;
    public TMP_Text tmpTextMesh2;

    public OctoPrint_Yellow Temp2;
    

    private void Update()
    {
      
        if (Temp2 != null)
        {
            float bedTemp = Temp2.BedActualTemperature;
            float toolTemp = Temp2.ToolActualTemperature;


            if (textMesh2 != null)
            {
                textMesh2.text = "Bed: " + bedTemp.ToString("F2") + "\n" + "Head: " + toolTemp.ToString("F2");
            }

            // Update TextMeshPro if assigned
            if (tmpTextMesh2 != null)
            {
                tmpTextMesh2.text = "Bed: " + bedTemp.ToString("F2") + "\n" + "Head: " + toolTemp.ToString("F2");
            }
        }
        else
        {
            if (textMesh2 != null)
            {
                textMesh2.text = "Bed: N/A\nHead: N/A";
            }

            if (tmpTextMesh2 != null)
            {
                tmpTextMesh2.text = "Bed: N/A\nHead: N/A";
            }
        }
    }
}



