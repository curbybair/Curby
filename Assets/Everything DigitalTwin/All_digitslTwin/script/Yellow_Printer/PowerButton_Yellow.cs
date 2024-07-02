using UnityEngine;

public class PowerButton_Yellow : MonoBehaviour
{
    public OctoPrint_Yellow tool;
    public OctoPrint_Yellow bed;

    public GameObject objectToToggle;
    public bool isOnByDefault = false;

    public Material offMaterial;
    public Material onMaterial;

    private bool isOn;
    private Renderer buttonRenderer;

    private GameObject presser;
    private AudioSource sound;
    private bool isPressed;

    private void Start()
    {
        isOn = isOnByDefault;
        SetObjectState();
        buttonRenderer = GetComponent<Renderer>();
        UpdateButtonMaterial();
        sound = GetComponent<AudioSource>();
        isPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            // Change button position to indicate it's being pressed
            transform.localPosition = new Vector3(0, 0.003f, 0);
            presser = other.gameObject;
            sound.Play();
            isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == presser)
        {
            // Change button position back to the original position
            transform.localPosition = new Vector3(0, 0.015f, 0);
            isPressed = false;

            // Toggle the state of the button
            if (isOn)
            {
                TurnOff();
                bed.IsTemperatureEndpointActive = false;
                tool.IsTemperatureEndpointActive = false;
            }
            else
            {
                TurnOn();
                bed.IsTemperatureEndpointActive = true;
                tool.IsTemperatureEndpointActive = true;
            }
        }
    }

    public void TurnOn()
    {
        isOn = true;
        SetObjectState();
        UpdateButtonMaterial();
    }

    public void TurnOff()
    {
        isOn = false;
        SetObjectState();
        UpdateButtonMaterial();
    }

    private void SetObjectState()
    {
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(isOn);
        }
    }

    private void UpdateButtonMaterial()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material = isOn ? onMaterial : offMaterial;
        }
    }
}