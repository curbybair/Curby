using System.Collections.Generic;
using UnityEngine;

public class LineChart : MonoBehaviour
{
    public GameObject linePrefab; // A prefab to represent data points or lines
    private List<GameObject> lines = new List<GameObject>();

    public void UpdateChart(List<List<float>> dataSets)
    {
        ClearChart();
        Debug.Log("Starting to update chart...");

        // For simplicity, assuming you want to represent each dataset as a line
        for (int i = 0; i < dataSets.Count; i++)
        {
            List<float> dataSet = dataSets[i];
            for (int j = 0; j < dataSet.Count - 1; j++)
            {
                // Prevent crash from too many lines
                if (j > 100) // Limiting to 100 data points for now (adjust based on needs)
                {
                    Debug.LogError("Too many data points. Limiting to 100 lines.");
                    break;
                }

                // Create a line between each data point
                GameObject newLine = Instantiate(linePrefab, transform); // Set parent to LineChart
                newLine.transform.localPosition = new Vector3(j, dataSet[j], 0); // Position in local space
                newLine.transform.localScale = new Vector3(1, Mathf.Abs(dataSet[j + 1] - dataSet[j]), 1); // Set height based on data difference

                // Position line between two points
                newLine.transform.position = new Vector3(j, (dataSet[j] + dataSet[j + 1]) / 2, 0); // Midpoint for line

                

                lines.Add(newLine);
            }
        }
    }

    public void ClearChart()
    {
        // Clear all the old lines
        foreach (var line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
    }
}
