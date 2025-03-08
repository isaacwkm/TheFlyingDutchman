using Needle.Console;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ZoneSound : MonoBehaviour
{
    public GameObject globalVolumeSetting;
    public bool isMusic;
    public AudioClip envMusicClip;

    // Volume multipliers for each surface type (adjust these values based on the loudness of your recordings)
    public Transform playerCameraHere;    // Reference to the object (camera currently) that has the listener
    [Range(0, 1f)]
    public float soundVolume = 0.1f; // Volume for this track.
    public float fadeDuration = 2;
    public float secondsToStayInZoneBeforePlaying = 1;
    [HideInInspector]
    public AudioSource audioSource;
    private float ambientAndMusicMultiplier = 0.8f; // Private multiplier (decided by developer) to all music volume towards this value.
    private float targetVolume; // Final target volume after formulas and multipliers
    private bool staying = false;
    private float currentStayTime = 0;

    private void Awake()
    {
        // Register this sound with the AudioManager
        AudioManager.Instance.RegisterSound(this);
    }
    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();

        float userMusicVolumeMultiplier = 1.0f;

        if (isMusic && globalVolumeSetting != null){
            // GET GLOBAL VOLUME FLOAT VALUE HERE, AND SET IT TO MUSIC VOLUME MULTIPLIER
        }
        targetVolume = soundVolume * ambientAndMusicMultiplier * userMusicVolumeMultiplier;

    }

    private void Update()
    {
        checkStaying();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; // if it's not a player entering it, ignore

        D.Log("Entered zonesound!", gameObject, "Aud");

        if (envMusicClip != null && !audioSource.isPlaying)
        { // It won't play if an audio is not found, or it won't play if a music is already playing.
          //startStayCheck();

            staying = true;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        staying = false;
        currentStayTime = 0;
        StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, 0));
    }

    private void playSound()
    {
        D.Log("Starting zonesound!", gameObject, "Aud");
        audioSource.clip = envMusicClip;
        audioSource.volume = 0;
        audioSource.Play();
        StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, targetVolume));
    }
    private float getCurrentPlayingVolume()
    {
        return FadeAudioSource.GetCurrentVolume();
    }

    private void checkStaying()
    {
        if (staying == false || currentStayTime == -1) return;

        currentStayTime += Time.deltaTime;
        D.Log($"Current Stay time: {currentStayTime}", gameObject, "Aud");
        if (currentStayTime >= secondsToStayInZoneBeforePlaying)
        {
            playSound();
            currentStayTime = -1;
        }
    }


}
