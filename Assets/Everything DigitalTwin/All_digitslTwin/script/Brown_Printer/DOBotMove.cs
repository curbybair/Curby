using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOBotMove : MonoBehaviour
{
    public float amplitude = 45.0f; // Maximum angle of rotation from the initial position
    public float frequency = 1.0f; // Oscillation frequency
    public AudioSource onSound; // AudioSource for the "on" sound
    public AudioSource offSound; // AudioSource for the "off" sound

    private Quaternion startRotation;
    private bool isMoving = false;
    private float elapsedTime = 0f; // Track elapsed time to maintain the current state

    void Start()
    {
        // Store the initial rotation of the GameObject
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (isMoving)
        {
            // Increment elapsed time
            elapsedTime += Time.deltaTime;

            // Calculate the new rotation
            float zRotation = Mathf.Sin(elapsedTime * frequency) * amplitude;
            transform.rotation = startRotation * Quaternion.Euler(0, 0, zRotation);
        }
    }

    public void ToggleMovement()
    {
        isMoving = !isMoving; // Toggle the movement state

        // Play the appropriate sound
        if (isMoving)
        {
            onSound.Play();
        }
        else
        {
            offSound.Play();
        }
    }
}
