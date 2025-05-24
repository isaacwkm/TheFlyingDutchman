using UnityEngine;

public class ShrimpBandAnimator : MonoBehaviour
{
    public MultiZoneSound targetZoneSound;

    [Tooltip("Animator for each shrimp in the band")]
    public Animator[] shrimpAnimators;

    [Tooltip("Name of the animation state to play for each shrimp")]
    public string[] shrimpAnimationNames;

    private bool hasPlayed = false;

    private void OnEnable()
    {
        if (targetZoneSound != null)
        {
            targetZoneSound.OnMusicStarted += PlayAllAnimations;
        }

        if (shrimpAnimators.Length != shrimpAnimationNames.Length)
        {
            Debug.LogWarning("ShrimpBandAnimator: Animator and animation name array lengths do not match!", this);
        }
    }

    private void OnDisable()
    {
        if (targetZoneSound != null)
        {
            targetZoneSound.OnMusicStarted -= PlayAllAnimations;
        }
    }

    private void PlayAllAnimations()
    {

        for (int i = 0; i < shrimpAnimators.Length; i++)
        {
            if (shrimpAnimators[i] != null && i < shrimpAnimationNames.Length)
            {
                shrimpAnimators[i].Play(shrimpAnimationNames[i], -1, 0);
            }
        }

    }

}
