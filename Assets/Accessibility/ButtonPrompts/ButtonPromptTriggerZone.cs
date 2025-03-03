using Needle.Console;
using UnityEngine;

[RequireComponent(typeof(InputPromptReplacer), typeof(TriggerZoneHandler))]
public class ButtonPromptTriggerZone : MonoBehaviour
{
    private InputPromptReplacer inputPromptReplacer;
    private TriggerZoneHandler triggerZoneHandler;
    public TooltipManager tooltipManager;

    void Awake()
    {
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
        D.Log("Entered ButtonPromptTriggerZone!", gameObject, "Able");
        inputPromptReplacer.SetConvertedText_SlowPerformance(); // Use of SlowPerformance setter is OK because ShowToolTip() is only called on an enter trigger rather than in a loop.
        tooltipManager.SetSecondTooltipActive(true);
    }

    private void HideTooltip()
    {
        D.Log("Exited ButtonPromptTriggerZone!", gameObject, "Able");
        tooltipManager.SetSecondTooltipActive(false);
    }
}
