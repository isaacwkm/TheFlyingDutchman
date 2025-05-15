using Needle.Console;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;


public class EffectFadeInOut : SCPEffect
{
    // This class is used to fade in and out a post-processing effect using Unity's SCPE (Scriptable Color Post Effects) package.
    public override string Name => "Gradient";
    public Volume volume;
    private Cinematics cinematics;
    private SCPE.BlackBars effect;

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
    public enum SCPEEffectType
    {
        Gradient,
        Glitch,
        ColorSplit
        // Add more as needed
    }


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
        volume.profile.TryGet(typeof(SCPE.Gradient), out effect);
        //Much like in the inspector, a parameter has to be overriden first if you want to modify it.
        effect.size.overrideState = true;
        //Initialize a starting value
        effect.size.value = 0f;
    }

    void Update()
    {
        if (!effect) return;
        if (!fadeActive) return;


        progress += blendInSpeed * Time.deltaTime;

        //An kind of parameter that controls the visibility of an effect (eg. intensity) is always between 0 and 1
        progress = Mathf.Clamp01(progress);
        CheckAnimationState(); // Sets fadeActive to false if the animation has finished.

        //You can use IntelliSense to browse the available parameters (CTRL+Space after the period).
        if (playingDirection == 1)
        {
            effect.size.value = intensityCurveUp.Evaluate(progress);
        }
        else
        {
            effect.size.value = intensityCurveDown.Evaluate(progress);
        }

    }

    public override void PlayForward(float speed = 0f)
    {
        if (speed == 0)
        {
            speed = blendInSpeed;
        }

        D.Log("Playing SCPE animation!", this, "PostProc");
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
        progress = 0;
        fadeActive = true;
        playingDirection = -1f;
    }

    public void DelayedPlayForward(float delay, float playSpeed)
    {
        CoroDelayedPlayForward(delay, playSpeed);
    }

    public IEnumerator CoroDelayedPlayForward(float delay, float playSpeed)
    {
        yield return new WaitForSeconds(delay);
        PlayForward(playSpeed);
    }

    public void DelayedPlayBackward(float delay, float playSpeed)
    {
        CoroDelayedPlayBackward(delay, playSpeed);
    }
    public IEnumerator CoroDelayedPlayBackward(float delay, float playSpeed)
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
