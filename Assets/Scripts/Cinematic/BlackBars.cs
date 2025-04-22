using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class BlackBarsEffect : MonoBehaviour, SCPEffect
{
    public Volume volume;
    private SCPE.BlackBars blackBarsEffect;

    public AnimationCurve intensityCurveUp = new AnimationCurve(new Keyframe[]
    {
        //Bell curve
        new Keyframe(0f, 0f), new Keyframe(1f, 1f)
    });

    public AnimationCurve intensityCurveDown = new AnimationCurve(new Keyframe[]
    {
        //Bell curve
        new Keyframe(0f, 1f), new Keyframe(1f, 0f)
    });

    public float blendInSpeed = 5f;

    [Range(0f, 1f)]
    private float progress;
    private bool customToggleOn = false;

    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    private void Reset()
    {
        volume = GetComponent<Volume>();
    }

    private void Start()
    {
        // Get the black bars effect
        volume.profile.TryGet(typeof(SCPE.BlackBars), out blackBarsEffect);
        //Much like in the inspector, a parameter has to be overriden first if you want to modify it.
        blackBarsEffect.size.overrideState = true;
        //Initialize a starting value
        blackBarsEffect.size.value = 0f;
    }

    void Update()
    {
        if (!blackBarsEffect) return;

        progress += blendInSpeed * Time.deltaTime;

        //An kind of parameter that controls the visibility of an effect (eg. intensity) is always between 0 and 1
        progress = Mathf.Clamp01(progress);

        //You can use IntelliSense to browse the available parameters (CTRL+Space after the period).        
        blackBarsEffect.size.value = intensityCurveUp.Evaluate(progress);
    }

    public void PlayForward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation!", this, "PostProc");
        progress = 0;
        // Set flag
    }

    public void PlayBackward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation!", this, "PostProc");
        progress = 0;
        // Set flag
    }
}
