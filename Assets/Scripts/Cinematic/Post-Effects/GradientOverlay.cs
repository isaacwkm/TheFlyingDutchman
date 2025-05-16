using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;


public class GradientEffect : SCPEffect
{
    public override string Name => "Gradient";
    public Volume volume;
    private Cinematics cinematics;
    private SCPE.Gradient gradientEffect;

    public AnimationCurve intensityCurveUp = new AnimationCurve(new Keyframe[]
    {
        new Keyframe(0f, 0f), new Keyframe(1f, 1f)
    });

    public AnimationCurve intensityCurveDown = new AnimationCurve(new Keyframe[]
  {
        new Keyframe(0f, 1f), new Keyframe(1f, 0f)
  });

    public float blendInSpeed = 5f;
    public float blendOutSpeed = 5f;

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

    private void Start()
    {
        // Initialize cinematics
        cinematics = SceneCore.cinematics;

        // Get the black bars effect
        volume.profile.TryGet(typeof(SCPE.Gradient), out gradientEffect);
        //Much like in the inspector, a parameter has to be overriden first if you want to modify it.
        gradientEffect.intensity.overrideState = true;
        //Initialize a starting value
        gradientEffect.intensity.value = 0f;
    }

    void Update()
    {
        if (!gradientEffect) return;
        if (!fadeActive) return;


        progress += blendInSpeed * Time.deltaTime;

        //An kind of parameter that controls the visibility of an effect (eg. intensity) is always between 0 and 1
        progress = Mathf.Clamp01(progress);
        CheckAnimationState(); // Sets fadeActive to false if the animation has finished.

        //You can use IntelliSense to browse the available parameters (CTRL+Space after the period).
        if (playingDirection == 1)
        {
            gradientEffect.intensity.value = intensityCurveUp.Evaluate(progress);
        }
        else
        {
            gradientEffect.intensity.value = intensityCurveDown.Evaluate(progress);
        }

    }

    public override void PlayForward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation! GradientOverlay PlayForward", this, "PostProc");
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

        D.Log("Playing SCPE animation! GradientOverlay PlayBackward", this, "PostProc");
        progress = 0;
        fadeActive = true;
        playingDirection = -1f;
    }

    public void DelayedPlayForward(float delay, float playSpeed = 0f)
    {
        CoroDelayedPlayForward(delay, playSpeed);
    }

    public IEnumerator CoroDelayedPlayForward(float delay, float playSpeed = 0f)
    {
        yield return new WaitForSeconds(delay);
        PlayForward(playSpeed);
    }

    public void DelayedPlayBackward(float delay, float playSpeed = 0f)
    {
        CoroDelayedPlayBackward(delay, playSpeed);
    }
    public IEnumerator CoroDelayedPlayBackward(float delay, float playSpeed = 0f)
    {
        yield return new WaitForSeconds(delay);
        PlayBackward(playSpeed);
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
