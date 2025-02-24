using UnityEngine;
using UnityEngine.InputSystem;


public class ActivateTeleportationRay : MonoBehaviour
{
    public GameObject leftTeleportRay;
    public GameObject rightTeleportRay;

    public InputActionProperty leftStick;
    public InputActionProperty rightStick;

    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    private bool leftStickWasActive = false;
    private bool rightStickWasActive = false;

    // Add references to the XR Ray Interactors
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor leftRayInteractor;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRayInteractor;

    void Update()
    {
        Vector2 leftStickValue = leftStick.action.ReadValue<Vector2>();
        Vector2 rightStickValue = rightStick.action.ReadValue<Vector2>();

        // Check if left stick is moved
        if (leftStickValue.magnitude > 0.1f)
        {
            leftTeleportRay.SetActive(true);
            leftStickWasActive = true;
        }
        // Trigger teleport on left stick release
        else if (leftStickWasActive)
        {
            leftStickWasActive = false;
            leftTeleportRay.SetActive(false);

            // Trigger teleport through XR Ray Interactor when stick is released
            TriggerTeleport(leftRayInteractor);
        }

        // Check if right stick is moved
        if (rightStickValue.magnitude > 0.1f)
        {
            rightTeleportRay.SetActive(true);
            rightStickWasActive = true;
        }
        // Trigger teleport on right stick release
        else if (rightStickWasActive)
        {
            rightStickWasActive = false;
            rightTeleportRay.SetActive(false);

            // Trigger teleport through XR Ray Interactor when stick is released
            TriggerTeleport(rightRayInteractor);
        }
    }

    private void TriggerTeleport(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor)
    {
        if (teleportationProvider != null && rayInteractor != null && rayInteractor.enabled)
        {
            // Trigger the interaction as if a teleportation button was pressed
            rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
            if (hit.collider != null)
            {
                UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
                {
                    destinationPosition = hit.point,
                    matchOrientation = UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.None
                };

                // Queue the teleport request
                teleportationProvider.QueueTeleportRequest(request);
            }
        }
        else
        {
            Debug.LogWarning("Teleportation provider or ray interactor is not available.");
        }
    }
}



