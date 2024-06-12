using UnityEngine;

public class PowerButton_Brown : MonoBehaviour
{
    public OctoPrint_Brown tool;
    public OctoPrint_Brown bed;

    public GameObject objectToToggle;
    public bool isOnByDefault = false;

    public Material offMaterial;
    public Material onMaterial;

    private bool isOn;
    private Renderer buttonRenderer;

    private void Start()
    {
        isOn = isOnByDefault;
        SetObjectState();
        buttonRenderer = GetComponent<Renderer>();
        UpdateButtonMaterial();
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {

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
            if (isOn)
            {
                buttonRenderer.material = onMaterial;
            }
            else
            {
                buttonRenderer.material = offMaterial;
            }
        }
    }
}


