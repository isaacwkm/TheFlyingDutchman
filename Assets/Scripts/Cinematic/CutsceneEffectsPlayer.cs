using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class CutsceneEffectsPlayer : MonoBehaviour
{
    public Volume volume;
    private SCPE.BlackBars blackBarsEffect;

    public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
    {
        //Bell curve
        new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)
    });

    //public SCPEffect

    [Range(0f, 1f)]
    private float progress;
    private InputSystem_Actions inputActions;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> onCustomToggle;
    private bool customToggleOn = false;

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
        if (customToggleOn)
        {
            CustomToggle();
        }
    }

    private void CustomToggle()
    {
        D.Log("CustomButton Pressed!", this, "Story");
        // call method here
    }

    private void initializeKeybinds()
    {
        inputActions = InputModeManager.Instance.inputActions;
        onCustomToggle = ctx => customToggleOn = true;
        inputActions.Player.CustomAction3.performed += onCustomToggle;
    }

    private void deInitializeKeybinds()
    {
        inputActions.Player.CustomAction3.performed -= onCustomToggle;
    }
}
