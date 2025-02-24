using UnityEngine;

public class DashboardController : MonoBehaviour
{
    private ur_data_processing urDataProcessor;

    void Start()
    {
        // Find the ur_data_processing instance in the scene
        urDataProcessor = FindFirstObjectByType<ur_data_processing>();
    }

    // Method to be called by UI Button
    public void RunRobot1()
    {
        if (urDataProcessor != null)
        {
            urDataProcessor.URDashboardRobot1.OnRunRobot1();
        }
    }

    // Method to be called by UI Button
    public void RunRobot2()
    {
        if (urDataProcessor != null)
        {
            urDataProcessor.URDashboardRobot2.OnRunRobot2();
        }
    }
}