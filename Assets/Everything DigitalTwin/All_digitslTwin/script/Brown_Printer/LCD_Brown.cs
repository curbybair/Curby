using UnityEngine;

public class LCD_Brown : MonoBehaviour
{
    public TextMesh Tempreture_text;

    public OctoPrint_Brown LCD_Brown_Object;

    private void Update()
    {
        if (LCD_Brown_Object != null)
        {
            float bedTemp = LCD_Brown_Object.BedActualTemperature;
            float toolTemp = LCD_Brown_Object.ToolActualTemperature;


            Tempreture_text.text = "Bed: " + bedTemp.ToString("F2") + "\n" + "Head: " + toolTemp.ToString("F2");
        }
        else
        {
            Tempreture_text.text = "Bed: N/A\nHead: N/A";
        }
    }
}