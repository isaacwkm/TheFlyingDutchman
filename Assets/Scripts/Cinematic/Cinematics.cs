using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;
public class Cinematics : MonoBehaviour
{
    public Volume volume;
    public DisableHUD hudDisabler;
    public SCPEffect[] effectsLibrary;
    private InputSystem_Actions inputActions;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> onCustomToggle;
    private bool customActionPressed = false;
    private bool toggler = true;

    void OnEnable()
    {
        initializeKeybinds();
    }

    void OnDisable()
    {
        deInitializeKeybinds();
    }

    private void Start()
    {
    }

    void Update()
    {
    }

    private void LateUpdate()
    {
        if (customActionPressed)
        {
            CustomAction();
        }
    }

    public void ToggleHUD()
    {
        hudDisabler.ToggleHUD();
    }
    public void SetHUDActive(bool isActive)
    {
        hudDisabler.SetActive(isActive);
    }

    // Returns the SCPEffect object of the effect in the library, given a string name.
    public SCPEffect FindEffect(string effectName)
    {
        foreach (SCPEffect effect in effectsLibrary)
        {
            if (effectName == effect.Name)
            {
                return effect;
            }
        }

        D.LogError("Failed to find effect in the library. Are you spelling the effect name correctly?");
        return null;
    }

    private void CustomAction()
    {
        D.Log("CustomButton Pressed!", this, "Story");
        EnableBlackBars(toggler);
        Toggle();
        customActionPressed = false;
    }

    public void EnableBlackBars(bool enabled)
    {
        BlackBarsEffect effect = (BlackBarsEffect)FindEffect("BlackBars");
        if (enabled)
        {
            effect.PlayForward();
        }
        else
        {
            effect.PlayBackward();
        }
    }

    private void initializeKeybinds()
    {
        inputActions = InputModeManager.Instance.inputActions;
        onCustomToggle = ctx => customActionPressed = true;
        inputActions.Player.CustomAction3.performed += onCustomToggle;
    }

    private void deInitializeKeybinds()
    {
        inputActions.Player.CustomAction3.performed -= onCustomToggle;
    }

    private void Toggle()
    {
        if (toggler == true)
        {
            toggler = false;
        }
        else
        {
            toggler = true;
        }
    }
}
