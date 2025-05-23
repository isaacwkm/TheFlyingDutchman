using UnityEngine;
using Needle.Console;

[RequireComponent(typeof(FloatPropertyInterpolator))]
public class MultiZoneSound : MonoBehaviour
{
    [Header("Zones ordered from inner (0) to outer (N)")]
    public TriggerZoneHandler[] zonesInnerToOuter;

    [Range(0, 1f)]
    public float[] volumesInnerToOuter = { 1f, 0.5f, 0.1f };

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
    private float soundStartGraceTime = 0f;


    private void Awake()
    {
        // Register with AudioManager (adds the AudioSource)
        AudioManager.Instance.RegisterSound(this);
    }

    private void Start()
    {
        // Validate audio source after registration
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            D.LogError("MultiZoneSound requires an AudioSource but none was registered.", gameObject, "Aud");
            enabled = false;
            return;
        }

        fader = GetComponent<FloatPropertyInterpolator>();
        fader.SetTarget(audioSource, "volume");

        ApplyVolume(isMusic ? VolumePrefs.musicVolume : 1f);
        SetupZoneCallbacks();
    }

    private void OnEnable()
    {
        fader = GetComponent<FloatPropertyInterpolator>(); // preload if needed
        VolumePrefs.OnMusicVolumeChanged += OnGlobalMusicVolumeChanged;
    }

    private void OnDisable() => VolumePrefs.OnMusicVolumeChanged -= OnGlobalMusicVolumeChanged;

    private void SetupZoneCallbacks()
    {
        for (int i = 0; i < zonesInnerToOuter.Length; i++)
        {
            int zoneIndex = i;
            zonesInnerToOuter[i].OnEnter += (_) => OnZoneEnter(zoneIndex);
            zonesInnerToOuter[i].OnExit += (_) => OnZoneExit(zoneIndex);
        }
    }

    private void OnZoneEnter(int level)
    {
        if (currentZoneLevel == -1 || level < currentZoneLevel)
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
            for (int i = 0; i < zonesInnerToOuter.Length; i++)
            {
                if (zonesInnerToOuter[i].IsPlayerInside())
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
        Debug.Log($"[Update] staying={staying}, currentStayTime={currentStayTime}, zoneLevel={currentZoneLevel}");

        if (staying)
        {
            currentStayTime += Time.deltaTime;
            if (currentStayTime >= secondsToStayInZoneBeforePlaying)
            {
                Debug.Log("Triggering PlaySound()");
                PlaySound();
                staying = false;
                currentStayTime = -1;
            }
        }

        if (!staying && audioSource != null && audioSource.volume < 0.001f && audioSource.isPlaying && Time.time > soundStartGraceTime)
        {
            Debug.Log("Volume low. Stopping sound.");
            audioSource.Stop();
        }


        if (currentZoneLevel == -1 && !AnyZoneHasPlayerInside())
        {
            staying = false;
            currentStayTime = 0;
        }

    }

    private bool AnyZoneHasPlayerInside()
    {
        foreach (var zone in zonesInnerToOuter)
        {
            if (zone.IsPlayerInside()) return true;
        }
        return false;
    }

    private void PlaySound()
    {
        D.Log("Playing zone sound", gameObject, "Aud");
        audioSource.clip = envMusicClip;
        audioSource.volume = 0;
        audioSource.loop = true;
        audioSource.Play();
        ApplyVolume(GetGlobalMultiplier());

        // Start grace period (so the system doesn't instantly stop the sound)
        soundStartGraceTime = Time.time + 0.25f;
    }


    private void OnGlobalMusicVolumeChanged(float multiplier)
    {
        if (!isMusic) return;
        ApplyVolume(multiplier);
    }

    private void ApplyVolume(float globalMultiplier)
    {
        if (fader == null)
        {
            Debug.LogWarning($"[MultiZoneSound] Tried to apply volume before fader was initialized on {gameObject.name}");
            return;
        }

        if (currentZoneLevel == -1)
        {
            fader.SetWithDuration(0f, fadeDuration);
            return;
        }

        float tierVol = volumesInnerToOuter[Mathf.Clamp(currentZoneLevel, 0, volumesInnerToOuter.Length - 1)];
        targetVolume = tierVol * ambientAndMusicMultiplier * globalMultiplier;
        fader.SetWithDuration(targetVolume, fadeDuration);
    }


    private float GetGlobalMultiplier() => isMusic ? VolumePrefs.musicVolume : 1f;

    public void ForceStop()
    {
        staying = false;
        currentStayTime = 0;
        currentZoneLevel = -1;

        if (audioSource != null)
        {
            fader.SetWithDuration(0f, fadeDuration);
            audioSource.Stop();
        }
    }
}
