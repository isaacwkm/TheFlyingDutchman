using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [HideInInspector] public MainMenu MainMenuSoundSettings;

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
    public void RegisterSound(ActionSound sound)
    {
        if (sound.clips == null || sound.clips.Count == 0)
        {
            Debug.LogWarning($"Sound component on {sound.gameObject.name} has no clips assigned.");
            return;
        }

        // Create and configure an AudioSource
        AudioSource audioSource = sound.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        sound.audioSource = audioSource;
    }

    /// Registers a SingleSound component and configures its AudioSource.
    public void RegisterSound(ZoneSound sound)
    {
        if (sound.envMusicClip == null)
        {
            Debug.LogWarning($"Sound component on {sound.gameObject.name} has no clip assigned.");
            return;
        }

        // Create and configure an AudioSource
        AudioSource audioSource = sound.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        sound.audioSource = audioSource;
    }

    /// Plays a specific sound clip from a SingleSound component by index.
    public void PlaySingleSoundAt(ActionSound sound, int clipIndex)
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
    public int PlayRandomSingleSound(ActionSound sound)
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
    private void PlayClip(ActionSound sound, AudioClip clip)
    {
        if (clip == null || sound.audioSource == null)
        {
            Debug.LogWarning($"Clip or AudioSource is missing for {sound.gameObject.name}.");
            return;
        }

        sound.audioSource.clip = clip;
        sound.audioSource.volume = sound.volume;
        sound.audioSource.pitch = sound.pitch;
        sound.audioSource.spatialBlend = sound.spatialBlend;
        sound.audioSource.PlayOneShot(clip);
    }
}
