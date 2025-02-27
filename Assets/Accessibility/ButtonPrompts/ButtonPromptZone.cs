using Needle.Console;
using UnityEngine;

public class ButtonPromptZone : MonoBehaviour
{
    public string buttonPrompt;
    public string replaceBUTTONPROMPTwithKey;
    private TooltipManager tooltipManager = null; 

    private void OnEnable()
    {
        tooltipManager = SceneCore.playerCharacter.gameObject.GetComponent<TooltipManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; // if it's not a player entering it, ignore

        SetTextWithIconsOnTextBox setter = tooltipManager.secondTooltipComponent.gameObject.GetComponent<SetTextWithIconsOnTextBox>();
        setter.message = buttonPrompt;
        setter.SetText(replaceBUTTONPROMPTwithKey);
        tooltipManager.ShowSecondTooltip();
    }
    private void OnTriggerExit(Collider other)
    {
        tooltipManager.HideSecondTooltip();
    }
}
