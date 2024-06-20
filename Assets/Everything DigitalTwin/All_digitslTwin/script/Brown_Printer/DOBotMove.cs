using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOBotMove : MonoBehaviour
{
    public float amplitude = 45.0f; // Maximum angle of rotation from the initial position
    public float frequency = 1.0f; // Oscillation frequency

    private Quaternion startRotation;

    void Start()
    {
        // Store the initial rotation of the GameObject
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Calculate the new rotation
        float zRotation = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.rotation = startRotation * Quaternion.Euler(0, 0, zRotation);

    }
}