using Needle.Console;
using UnityEditor.SearchService;
using UnityEngine;

public class ToggleMap : MonoBehaviour
{
    public Camera[] cameras;
    private int currentCameraIndex = 0;

    public void Toggle()
    {
        if (currentCameraIndex == 1) // Map is open
        {
            SwitchCamera(0);
            SceneCore.cinematics.SetHUDActive(false);
            InputModeManager.Instance.EnablePlayerControls(false);
        }
        else // Map is not open
        {
            SwitchCamera(1);
            SceneCore.cinematics.SetHUDActive(true);
            InputModeManager.Instance.EnablePlayerControls(true);
        }
    }
    public void SwitchCamera(int camNumber)
    {
        if (camNumber < 0 || camNumber >= cameras.Length)
        {
            D.LogError("check context", this, "Any");
            return;
        }
        
        // Disable the current camera
        cameras[currentCameraIndex].gameObject.SetActive(false);

        currentCameraIndex = camNumber;

        // Enable the new active camera
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }
}