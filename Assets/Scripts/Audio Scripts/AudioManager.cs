using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// Registers a SingleSound component and configures its AudioSource.
    public void RegisterSound(SingleSoundComponent sound)
    {
        if (sound.clips == null || sound.clips.Count == 0)
        {
            Debug.LogWarning($"Sound component on {sound.gameObject.name} has no clips assigned.");
            return;
        }

        // Create and configure an AudioSource
        AudioSource audioSource = sound.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        sound.source = audioSource;
    }

    /// Plays a specific sound clip from a SingleSound component by index.
    public void PlaySoundAt(SingleSoundComponent sound, int clipIndex)
    {
        if (sound == null)
        {
            Debug.LogWarning("Sound component is null.");
            return;
        }

        if (clipIndex < 0 || clipIndex >= sound.clips.Count)
        {
            Debug.LogWarning($"Clip index {clipIndex} is out of range for {sound.gameObject.name}.");
            return;
        }

        PlayClip(sound, sound.clips[clipIndex]);
    }

    /// Plays a random sound clip from a SingleSound component.
    public int PlayRandomSound(SingleSoundComponent sound)
    {
        if (sound == null)
        {
            Debug.LogWarning("Sound component is null.");
            return -1;
        }

        if (sound.clips.Count == 0)
        {
            Debug.LogWarning($"No clips available for {sound.gameObject.name}.");
            return -1;
        }

        int randomIndex = Random.Range(0, sound.clips.Count);
        PlayClip(sound, sound.clips[randomIndex]);
        return randomIndex;
    }

    /// Helper method to play a sound clip from the given SingleSound component.
    private void PlayClip(SingleSoundComponent sound, AudioClip clip)
    {
        if (clip == null || sound.source == null)
        {
            Debug.LogWarning($"Clip or AudioSource is missing for {sound.gameObject.name}.");
            return;
        }

        sound.source.clip = clip;
        sound.source.volume = sound.volume;
        sound.source.pitch = sound.pitch;
        sound.source.spatialBlend = sound.spatialBlend;
        sound.source.PlayOneShot(clip);
    }
}
