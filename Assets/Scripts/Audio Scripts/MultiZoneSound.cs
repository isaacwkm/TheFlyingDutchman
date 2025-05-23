using UnityEngine;
using Needle.Console;

[RequireComponent(typeof(FloatPropertyInterpolator))]
public class MultiZoneSound : MonoBehaviour
{
    [Header("Volume tiers ordered outer → inner")]
    public TriggerZoneHandler[] soundZones;
    [Range(0, 1f)] public float[] volumeTiers = { 0.1f, 0.5f, 1f };

    public bool isMusic;
    public AudioClip envMusicClip;
    public float fadeDuration = 2f;
    public float secondsToStayInZoneBeforePlaying = 1f;

    [HideInInspector] public AudioSource audioSource;

    private FloatPropertyInterpolator fader;
    private float ambientAndMusicMultiplier = 0.8f;
    private float targetVolume;
    private int currentZoneLevel = -1;
    private bool staying = false;
    private float currentStayTime = 0;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        fader = GetComponent<FloatPropertyInterpolator>();
        fader.SetTarget(audioSource, "volume");

        AudioManager.Instance.RegisterSound(this);
    }

    private void Start()
    {
        ApplyVolume(isMusic ? VolumePrefs.musicVolume : 1f);
        SetupZoneCallbacks();
    }

    private void OnEnable() => VolumePrefs.OnMusicVolumeChanged += OnGlobalMusicVolumeChanged;
    private void OnDisable() => VolumePrefs.OnMusicVolumeChanged -= OnGlobalMusicVolumeChanged;

    private void SetupZoneCallbacks()
    {
        for (int i = 0; i < soundZones.Length; i++)
        {
            int zoneIndex = i;
            soundZones[i].OnEnter += (_) => OnZoneEnter(zoneIndex);
            soundZones[i].OnExit += (_) => OnZoneExit(zoneIndex);
        }
    }

    private void OnZoneEnter(int level)
    {
        if (level > currentZoneLevel)
        {
            currentZoneLevel = level;
            if (!audioSource.isPlaying && envMusicClip != null)
            {
                staying = true;
                currentStayTime = 0;
            }
            else
            {
                ApplyVolume(GetGlobalMultiplier());
            }
        }
    }

    private void OnZoneExit(int level)
    {
        if (level == currentZoneLevel)
        {
            int fallbackLevel = -1;
            for (int i = soundZones.Length - 1; i >= 0; i--)
            {
                if (soundZones[i].IsPlayerInside()) // Must track this in TriggerZoneHandler
                {
                    fallbackLevel = i;
                    break;
                }
            }

            currentZoneLevel = fallbackLevel;
            ApplyVolume(GetGlobalMultiplier());
        }
    }

    private void Update()
    {
        if (!staying) return;

        currentStayTime += Time.deltaTime;
        if (currentStayTime >= secondsToStayInZoneBeforePlaying)
        {
            PlaySound();
            staying = false;
            currentStayTime = -1;
        }

        if (!staying && audioSource.volume < 0.001f)
        {
            audioSource.Stop();
        }
    }

    private void PlaySound()
    {
        D.Log("Playing zone sound", gameObject, "Aud");
        audioSource.clip = envMusicClip;
        audioSource.volume = 0;
        audioSource.Play();
        ApplyVolume(GetGlobalMultiplier());
    }

    private void OnGlobalMusicVolumeChanged(float multiplier)
    {
        if (!isMusic) return;
        ApplyVolume(multiplier);
    }

    private void ApplyVolume(float globalMultiplier)
    {
        if (currentZoneLevel == -1)
        {
            fader.SetWithDuration(0f, fadeDuration);
            return;
        }

        float tierVol = volumeTiers[Mathf.Clamp(currentZoneLevel, 0, volumeTiers.Length - 1)];
        targetVolume = tierVol * ambientAndMusicMultiplier * globalMultiplier;
        fader.SetWithDuration(targetVolume, fadeDuration);
    }

    private float GetGlobalMultiplier()
    {
        return isMusic ? VolumePrefs.musicVolume : 1f;
    }

    public void ForceStop()
    {
        staying = false;
        currentStayTime = 0;
        currentZoneLevel = -1;
        fader.SetWithDuration(0f, fadeDuration);
        audioSource.Stop();
    }
}
