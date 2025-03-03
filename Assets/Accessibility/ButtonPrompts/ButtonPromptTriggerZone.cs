using UnityEngine;

[RequireComponent(typeof(InputPromptReplacer), typeof(TriggerZoneHandler))]
public class ButtonPromptTriggerZone : MonoBehaviour
{
    private InputPromptReplacer inputPromptReplacer;
    private TriggerZoneHandler triggerZoneHandler;

    void Awake()
    {
        inputPromptReplacer = gameObject.GetComponent<InputPromptReplacer>();
        triggerZoneHandler = gameObject.GetComponent<TriggerZoneHandler>();
    }
    void OnEnable()
    {
    }

    void OnDisable()
    {
        
    }
}
