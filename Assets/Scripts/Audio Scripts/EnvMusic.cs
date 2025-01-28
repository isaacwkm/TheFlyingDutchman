using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ZoneSound : MonoBehaviour
{
    public AudioClip envMusicClip;

    // Volume multipliers for each surface type (adjust these values based on the loudness of your recordings)
    public float envMusicVolume = 0.2f; // Volume
    public Transform listenerTransform;    // Reference to the object (camera currently) that has the listener
    [Range(0, 1f)]
    public float musicVolume = 0.1f; // Volume for this track.
    private CharacterController characterController; // Private reference to player character controller
    private float musicVolumeMultiplier = 0.8f; // Bring all music volume towards this value.
    private float targetVolume; // Final target volume after formulas and multipliers
    public AudioSource audioSource;
    private float fadeDuration = 2;

    private void Awake()
    {
        // Register this sound with the AudioManager
        AudioManager.Instance.RegisterSound(this);
    }
    private void Start()
    {
        audioSource = listenerTransform.GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        targetVolume = musicVolume * musicVolumeMultiplier;

    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (envMusicClip != null && !audioSource.isPlaying)
        { // It won't play if an audio is not found, or it won't play if a music is already playing.
            audioSource.clip = envMusicClip;
            audioSource.volume = 0;
            audioSource.Play();
            StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, targetVolume));
        }

    }
    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, 0));

    }


}
