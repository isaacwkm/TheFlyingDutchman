using Needle.Console;
using UnityEngine;

public class PocketDimensionSequence : MonoBehaviour
{
    public GameObject pocketDimension;
    Cinematics cinematics;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pocketDimension.SetActive(false); // Hide pocket dimension
        cinematics = SceneCore.cinematics; // Initialize cinematics
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayPocketDimensionEffects(bool entering)
    {
        if (entering)
        {
            D.Log("Entering pocket dimension", this.gameObject, LogManager.LogCategory.PostProc);
            EnablePocketDimension(true);
            GradientEffect gradient = cinematics.GetComponent<GradientEffect>();
            gradient.PlayForward(1f);
            gradient.CoroDelayedPlayBackward(2f);
        }
        else
        {

        }
    }

    public void EnablePocketDimension(bool active)
    {
        if (active)
        {
            pocketDimension.SetActive(true);

        }
        else
        {
            pocketDimension.SetActive(false);
        }
    }
}
