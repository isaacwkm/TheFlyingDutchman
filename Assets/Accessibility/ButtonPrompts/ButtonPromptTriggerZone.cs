using Needle.Console;
using UnityEngine;

[RequireComponent(typeof(InputPromptReplacer), typeof(TriggerZoneHandler))]
public class ButtonPromptTriggerZone : MonoBehaviour
{
    public enum WhereToShowTooltip
    {
        TopLeftTooltips = 1,
        SecondaryTooltip = 2
    }
    public WhereToShowTooltip whereToShowTooltip = WhereToShowTooltip.TopLeftTooltips;
    public TooltipManager tooltipManager;
    private TopLeftTooltipManager topLeftTooltipManager;
    private InputPromptReplacer inputPromptReplacer;
    private TriggerZoneHandler triggerZoneHandler;
    private GameObject topLeftMessageObjReference;


    void Awake()
    {
        topLeftTooltipManager = tooltipManager.gameObject.GetComponent<TopLeftTooltipManager>();
        inputPromptReplacer = gameObject.GetComponent<InputPromptReplacer>();
        triggerZoneHandler = gameObject.GetComponent<TriggerZoneHandler>();
    }
    void OnEnable()
    {
        triggerZoneHandler.OnEnter += ShowTooltip;
        triggerZoneHandler.OnExit += HideTooltip;
    }

    void OnDisable()
    {
        triggerZoneHandler.OnEnter -= ShowTooltip;
        triggerZoneHandler.OnExit -= HideTooltip;
    }

    private void ShowTooltip()
    {
        switch (whereToShowTooltip)
        {
            case WhereToShowTooltip.SecondaryTooltip: 
                ShowButtonPromptAtSecondTooltip();
                return;
            case WhereToShowTooltip.TopLeftTooltips: 
                ShowButtonPromptAtTopLeftTooltip();
                return;
            default:
                return;
        }
    }

    private void HideTooltip()
    {
        switch (whereToShowTooltip)
        {
            case WhereToShowTooltip.SecondaryTooltip: 
                HideButtonPromptAtSecondTooltip();
                return;
            case WhereToShowTooltip.TopLeftTooltips: 
                HideButtonPromptAtTopLeftTooltip();
                return;
            default:
                return;
        }
    }

    private void ShowButtonPromptAtSecondTooltip()
    {
        D.Log("Entered ButtonPromptTriggerZone!", gameObject, "Able");
        inputPromptReplacer.SetConvertedText_SlowPerformance(); // Use of SlowPerformance setter is OK because ShowToolTip() is only called on an enter trigger rather than in a loop.
        tooltipManager.SetSecondTooltipActive(true);
    }

    private void HideButtonPromptAtSecondTooltip()
    {
        D.Log("Exited ButtonPromptTriggerZone!", gameObject, "Able");
        tooltipManager.SetSecondTooltipActive(false);
    }

    private void ShowButtonPromptAtTopLeftTooltip()
    {
        topLeftMessageObjReference = topLeftTooltipManager.AddMessageWithConversion(inputPromptReplacer); // Adds Message to top left tooltip using QuickAdd
    }

    private void HideButtonPromptAtTopLeftTooltip()
    {
        topLeftTooltipManager.RemoveMessage(topLeftMessageObjReference); // Removes the message that was shown in the top left by using the reference saved when first shown.
    }
}
