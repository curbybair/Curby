using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PrinterType{ Grey, Yellow }

public class GCodeReader
{
    string[] Lines;
    int Index;

    Vector2 greyCursor, yellowCursor;

    float minSphereSize = 0.02f;
    float maxSphereSize = 0.03f;

    float printheadLength = 0.2f;
    GameObject greyPrintHead, yellowPrintHead;

    public GCodeReader (string[] lines, GameObject greyHead, GameObject yellowHead)
    {
        Lines = lines;
        Index = 0;

        greyPrintHead = greyHead;
        yellowPrintHead = yellowHead;

        greyCursor = Vector2.zero;
        yellowCursor = Vector2.zero;
    }

    public IEnumerator UpdatePrintModel(string[] newLines, PrinterType printer)
    {
        Lines = newLines;
        Index = 0;

        while (HasNextCommand())
        {
            yield return RunOneCommand(printer);
        }
    }
    
    Vector3 GCodeToWorld(Vector2 gcode)
    {
        return new Vector3(gcode.x, 0, gcode.y);
    }

    void PrintheadChase(Vector2 cursor, GameObject printHead)
    {
        Vector3 worldPosition = GCodeToWorld(cursor);
        printHead.transform.position = worldPosition + Vector3.up * printheadLength / 2;
    }

    void DepositMaterial(Vector2 cursor, PrinterType printer)
    {
        GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blob.transform.position = GCodeToWorld(cursor);
        blob.transform.localScale = new Vector3(
            Random.Range(minSphereSize, maxSphereSize),
            Random.Range(minSphereSize, maxSphereSize),
            Random.Range(minSphereSize, maxSphereSize));
        
        if (printer == PrinterType.Grey)
            blob.GetComponent<Renderer>().material.color = Color.grey;
        else
            blob.GetComponent<Renderer>().material.color = Color.yellow;
        
    }

    IEnumerator DragPrinthead(Vector2 targetPosition, PrinterType printer)
    {
        Vector2 currentCursor = printer == PrinterType.Grey ? greyCursor : yellowCursor;
        GameObject printHead = printer == PrinterType.Grey ? greyPrintHead : yellowPrintHead;

        float distance = Vector2.Distance(targetPosition, currentCursor);
        int count = 1 + (int)(distance / minSphereSize);

        for (int i = 0; i <= count; i++)
        {
            float fraction = (float)i / count;
            Vector2 position = Vector2.Lerp(currentCursor, targetPosition, fraction);

            // Update the cursor
            if (printer == PrinterType.Grey)
                greyCursor = position;
            else
                yellowCursor = position;

            DepositMaterial(position, printer);
            PrintheadChase(position, printHead);

            yield return null;
        }

     // Update cursor to the final position
        if (printer == PrinterType.Grey)
            greyCursor = targetPosition;
        else
            yellowCursor = targetPosition;
    }

    float Parse(string[] parts, string s)
    {
        char c = s[0];

        foreach (var part in parts)
        {
            if (part[0] == c)
            {
                return float.Parse(part.Substring(1));
            }
        }
        Debug.LogError("Cannot parse for '" + c + "' in these lines");
        for (int i = 0; i < parts.Length; i++)
        {
            Debug.LogError("Part " + i + ": " + parts[i]);
        }
        throw new System.FormatException("Cannot parse for '" + c + "' in these lines");
    }

    public IEnumerator RunOneCommand(PrinterType printer)
    {
        int operationsPerYield = 10;
        int operationCounter = 0;

        System.Func<bool> ShouldYield = () => {
            operationCounter++;
            return (operationCounter % operationsPerYield) == 0;
        };

        if (Index >= Lines.Length)
            yield break;

        string line = Lines[Index];
        string[] parts = line.Split(' ');
        string cmd = parts[0];

        Vector2 position = Vector2.zero;
        Vector2 center = Vector2.zero;

        System.Func<Vector2> ReadXY = () => {
            float x = Parse(parts, "X");
            float y = Parse(parts, "Y");
            return new Vector2(x, y);
        };

        System.Func<Vector2> ReadIJ = () => {
            float i = Parse(parts, "I");
            float j = Parse(parts, "J");
            return new Vector2(i, j);
        };

        IEnumerator commandEnumerator = null;

        switch (cmd)
        {
            case "%":
                break;
            case ";":
                break;
            case "G21":
            case "G17":
            case "G90":
                // Silent fail for now
                break;
            case "M03":
                break;
            case "G28":
                break;
            case "G00": // Fast reposition
                position = ReadXY();
                commandEnumerator = DragPrinthead(position, printer);
                break;
            case "G01": // Linear transit
                position = ReadXY();
                commandEnumerator = DragPrinthead(position, printer);
                break;
            case "G02": // Clockwise arc
            case "G03": // Counter-clockwise arc
                position = ReadXY();
                center = ReadIJ() + (printer == PrinterType.Grey ? greyCursor : yellowCursor);
                commandEnumerator = RotateAround(position, center, printer);
                break;
            default:
                Debug.LogWarning($"Unknown G-code command: {cmd}. Skipping.");
                break;
        }
        if (commandEnumerator != null)
        {
            while (commandEnumerator.MoveNext())
            {
                if (ShouldYield())
                    yield return null;
            }
        }

        Index++;
        yield return null;
    }

    void SnapPrinthead(Vector2 position, PrinterType printer)
    {
        if (printer == PrinterType.Grey)
            greyCursor = position;
        else
            yellowCursor = position;

        GameObject printHead = printer == PrinterType.Grey ? greyPrintHead : yellowPrintHead;
        PrintheadChase(position, printHead);
    }

    IEnumerator RotateAround(Vector2 targetPosition, Vector2 center, PrinterType printer)
    {
        Vector2 currentCursor = printer == PrinterType.Grey ? greyCursor : yellowCursor;
        GameObject printHead = printer == PrinterType.Grey ? greyPrintHead : yellowPrintHead;

        // The two arms
        Vector2 dA = currentCursor - center;
        Vector2 dB = targetPosition - center;

        float radius = dA.magnitude;
        radius = dB.magnitude;

        float dotProduct = Vector2.Dot(dA, dB);
        float cos = dotProduct / (radius * radius);

        if (cos < -1 || cos > +1)
        {
            Debug.LogWarning("acos domain exceedance.");
            yield break;
        }

        float theta = Mathf.Acos(cos);
        float arcLength = radius * theta;

        int count = 1 + (int)(arcLength / minSphereSize);

        for (int i = 0; i <= count; i++)
        {
            float fraction = (float)i / count;
            Vector2 position = center + (Vector2)Vector3.Slerp(dA, dB, fraction);

            if (printer == PrinterType.Grey)
                greyCursor = position;
            else
                yellowCursor = position;

            DepositMaterial(position, printer);
            PrintheadChase(position, printHead);

            yield return null;
        }

        if (printer == PrinterType.Grey)
            greyCursor = targetPosition;
        else
            yellowCursor = targetPosition;
    }

    // Method to check if there are more commands to process
    public bool HasNextCommand()
    {
        return Index < Lines.Length;
    }
}
