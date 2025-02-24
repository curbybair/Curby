using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTaskBlock : MonoBehaviour
{
    public GameObject TaskBlockYellow;  // Block to activate
    public GameObject TaskBlockGrey;    // Block to activate

    void Start()
    {
        // Enable both Task Blocks
        TaskBlockGrey.SetActive(true);
        TaskBlockYellow.SetActive(true);
        
        Debug.Log("Both Task Blocks enabled.");
    }
}
