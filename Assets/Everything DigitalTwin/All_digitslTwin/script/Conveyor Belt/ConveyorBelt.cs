using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public float conveyorSpeed = 1.0f; // Speed of the conveyor belt

    public enum Direction
    {
        Left,
        Right
    }

    public Direction conveyorDirection = Direction.Right;

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Determine the movement direction
            Vector3 moveDirection = Vector3.right; // Default to right

            if (conveyorDirection == Direction.Left)
            {
                moveDirection = Vector3.left;
            }

            // Move the object on the conveyor belt
            Vector3 move = moveDirection * conveyorSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the direction of the conveyor belt in the Scene view for visualization
        Gizmos.color = Color.red;
        Vector3 direction = Vector3.right; // Default to right

        if (conveyorDirection == Direction.Left)
        {
            direction = Vector3.left;
        }

        Gizmos.DrawLine(transform.position, transform.position + direction * 2);
    }
}