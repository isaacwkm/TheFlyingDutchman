using UnityEngine;

public class SecondaryButtonPrompt : MonoBehaviour
{
    TriggerZoneHandler buttonPromptZone;


    void OnEnable()
    {
        buttonPromptZone.OnEnter += foo;
        buttonPromptZone.OnExit += foo;
    }
    void Oisable()
    {
        buttonPromptZone.OnEnter -= foo;
        buttonPromptZone.OnExit -= foo;
    }

    void foo(){

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
