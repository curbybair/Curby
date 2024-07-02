using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanMove : MonoBehaviour
{
    public float rotationSpeed = 100f; // Speed of the fan rotation

    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
        {
            // Rotate the fan around its Z-axis
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    public void ToggleMovement()
    {
        isMoving = !isMoving; // Toggle the movement state
    }
}
