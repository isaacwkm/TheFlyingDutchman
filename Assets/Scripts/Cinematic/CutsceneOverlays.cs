using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class CutsceneOverlays : MonoBehaviour
{
    public Volume volume;
    private SCPE.RadialBlur radialBlurEffect;

    public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
    {
        //Bell curve
        new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)
    });

    public float blendInSpeed = 20f;

    [Range(0f, 1f)]
    private float progress;
    private InputSystem_Actions inputActions;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> onCustomToggle;
    private bool customToggleOn = false;

    void OnEnable()
    {
        inputActions = InputModeManager.Instance.inputActions;
        onCustomToggle = ctx => customToggleOn = true;
        inputActions.Player.CustomAction3.performed += onCustomToggle;
    }

    void OnDisable()
    {
        inputActions.Player.CustomAction3.performed -= onCustomToggle;
    }

    private void Reset()
    {
        volume = GetComponent<Volume>();
    }

    private void Start()
    {
        //Note: Accessing "profile" creates a new temporary copy of the profile, it can be modified freely. You can tell by the fact that the name of the profile turns blank in the inspector!
        //This behaviour is identical to accessing "material" on a Renderer component (as opposed to "sharedMaterial")
        //When using "sharedProfile" you'll be modifying the profile asset saved to disk. Changes made to parameters will be persistent, which isn't the intention here.
        volume.profile.TryGet(typeof(SCPE.RadialBlur), out radialBlurEffect);

        //Another possible method is to create a temporary Volume object at runtime, with a runtime created profile. Then adding effects to it.

        //Much like in the inspector, a parameter has to be overriden first if you want to modify it. If it isn't, the value from another volume/profile may be controlling it.
        radialBlurEffect.amount.overrideState = true;

        //Initialize a starting value
        radialBlurEffect.amount.value = 0f;
    }

    void Update()
    {
        if (!radialBlurEffect) return;

        progress += blendInSpeed * Time.deltaTime;

        //An kind of parameter that controls the visibility of an effect (eg. intensity) is always between 0 and 1
        progress = Mathf.Clamp01(progress);

        //Depending on the effect, the "main" parameter may be called different. Most of the time the parameter is called "intensity".
        //You can use IntelliSense to browse the available parameters (CTRL+Space after the period).
        //The .value path points to the actual value of the parameter (it may be a float, color, int, etc...)
        
        radialBlurEffect.amount.value = intensityCurve.Evaluate(progress);

        Debug.Log($"[RadialBlur] progress: {progress:F2} | amount: {radialBlurEffect.amount.value:F2}");
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


        progress = 0;
        customToggleOn = false;
    }
}
