using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class FadeAudioSource {
    private static float currentVolume = 0; // Private variable to track volume
    public static float GetCurrentVolume()
    {
        return currentVolume;
    }
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume, float startVolume = -1)
    {
        float currentTime = 0;

        if (startVolume == -1){
            startVolume = audioSource.volume;
        }

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            currentVolume = audioSource.volume;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        if (audioSource.volume == 0){
            audioSource.Stop();
        }
        /* ^ If we do this, music won't start when muted in settings, and therefore will remain muted even if the setting changes.
         * We can find a more elegant solution than just always playing the music at zero volume
         * when we're under less time crunch */
        yield break;
    }
}
