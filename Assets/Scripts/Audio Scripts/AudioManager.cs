using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private List<Sound> sounds = new List<Sound>();

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

    public void RegisterSound(Sound sound)
    {
        if (sound.clip == null)
        {
            Debug.LogWarning($"Sound component on {sound.gameObject.name} has no clip assigned.");
            return;
        }

        // Create and configure an AudioSource
        AudioSource audioSource = sound.gameObject.AddComponent<AudioSource>();
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.pitch = sound.pitch;
        audioSource.spatialBlend = sound.spatialBlend;
        audioSource.playOnAwake = false;

        sound.source = audioSource;

        sounds.Add(sound);
    }

    public void PlaySound(Sound sound)
    {
        if (sound != null && sound.source != null)
        {
            sound.source.Play();
        }
    }

    public void PlaySound(GameObject soundObject)
    {
        Sound sound = soundObject.GetComponent<Sound>();
        if (sound != null)
        {
            PlaySound(sound);
        }
        else
        {
            Debug.LogWarning($"No Sound component found on {soundObject.name}.");
        }
    }
}
