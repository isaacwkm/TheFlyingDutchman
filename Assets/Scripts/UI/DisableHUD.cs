using UnityEngine;

public class DisableHUD : MonoBehaviour
{
    public CanvasGroup[] HUDElementsToToggles;
    private bool hudToggle = true;

    public void toggleHUD(bool isActive)
    {
        hudToggle = isActive;
        float alphaOwl = 0; // Alpha value for the HUD element. Setting the CanvasGroup component of a HUD element enables an alpha value to be modified, which can be set to 0 to hide it (without disabling it).

        if (hudToggle == true)
        {
            alphaOwl = 1;
        }
        else
        {
            alphaOwl = 0;
        }

        foreach (CanvasGroup HUDElement in HUDElementsToToggles){
            HUDElement.alpha = alphaOwl;
        }
    }
}
