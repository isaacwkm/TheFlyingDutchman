using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class BlackBarsEffect : SCPEffect
{
    public override string Name => "BlackBars";
    public Volume volume;
    public DisableHUD hudDisabler;
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
    private float cachedProgress = -1;
    private bool fadeActive = false;
    private float playingDirection = 1f;

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
        if (!fadeActive) return;


        progress += blendInSpeed * Time.deltaTime;

        //An kind of parameter that controls the visibility of an effect (eg. intensity) is always between 0 and 1
        progress = Mathf.Clamp01(progress);
        CheckAnimationState(); // Sets fadeActive to false if the animation has finished.

        //You can use IntelliSense to browse the available parameters (CTRL+Space after the period).
        if (playingDirection == 1)
        {
            blackBarsEffect.size.value = intensityCurveUp.Evaluate(progress);
        }
        else
        {
            blackBarsEffect.size.value = intensityCurveDown.Evaluate(progress);
        }
        
    }

    public override void PlayForward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation!", this, "PostProc");
        hudDisabler.SetActive(false);
        progress = 0;
        fadeActive = true;
        playingDirection = 1f;
    }

    public override void PlayBackward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation!", this, "PostProc");
        hudDisabler.SetActive(true);
        progress = 0;
        fadeActive = true;
        playingDirection = -1f;
    }

    private void CheckAnimationState()
    {
        if (progress == cachedProgress)
        {
            fadeActive = false;
            cachedProgress -= 1;
            return;
        }
        cachedProgress = progress;
    }
}
