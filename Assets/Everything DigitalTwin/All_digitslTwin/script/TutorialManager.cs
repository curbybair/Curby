
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance; // Singleton for global access
    public bool isTutorialActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public void ActivateTutorial()
    {
        isTutorialActive = true;
        Debug.Log("Tutorial Activated!");
    }

    public void DeactivateTutorial()
    {
        isTutorialActive = false;
        Debug.Log("Tutorial Deactivated!");
    }
}