using UnityEngine;
using System;

[RequireComponent(typeof(FloatPropertyInterpolator))]
[RequireComponent(typeof(AudioSource))]
public class DynamicZoneSound : MonoBehaviour
{
    [Header("Zones must be ordered: outermost to innermost")]
    public TriggerZoneHandler[] SoundZones;

    [Range(0, 1f)]
    public float[] soundVolumes = { 0.1f, 0.4f, 1.0f }; // Per zone level

    public AudioClip envMusicClip;
    public bool isMusic = true;
    public float fadeDuration = 1.5f;

    private int currentZoneLevel = -1; // -1 = not in any zone
    private FloatPropertyInterpolator fader;
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        fader = GetComponent<FloatPropertyInterpolator>();
        fader.SetTarget(audioSource, "volume");

        if (envMusicClip != null)
        {
            audioSource.clip = envMusicClip;
            audioSource.volume = 0f;
            audioSource.loop = true;
        }

        for (int i = 0; i < SoundZones.Length; i++)
        {
            int zoneIndex = i; // capture for closure
            SoundZones[i].OnEnter += (_) => HandleZoneEnter(zoneIndex);
            SoundZones[i].OnExit += (_) => HandleZoneExit(zoneIndex);
        }
    }

    private void HandleZoneEnter(int zoneLevel)
    {
        if (zoneLevel > currentZoneLevel)
        {
            currentZoneLevel = zoneLevel;
            ApplyZoneVolume();
        }
    }

    private void HandleZoneExit(int zoneLevel)
    {
        if (zoneLevel == currentZoneLevel)
        {
            // Look for next deepest active zone
            int newLevel = -1;
            for (int i = SoundZones.Length - 1; i >= 0; i--)
            {
                if (SoundZones[i].IsPlayerInside()) // <- You'll need to expose this somehow
                {
                    newLevel = i;
                    break;
                }
            }
            currentZoneLevel = newLevel;
            ApplyZoneVolume();
        }
    }

    private void ApplyZoneVolume()
    {
        if (currentZoneLevel == -1)
        {
            fader.SetWithDuration(0f, fadeDuration);
            return;
        }

        float vol = soundVolumes[Mathf.Clamp(currentZoneLevel, 0, soundVolumes.Length - 1)];
        if (!audioSource.isPlaying)
            audioSource.Play();

        fader.SetWithDuration(vol, fadeDuration);
    }
}
