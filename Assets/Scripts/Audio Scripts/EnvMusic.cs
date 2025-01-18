using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnvMusic : MonoBehaviour
{
    public AudioClip fallbackMusic; // 
    public AudioClip GrassRoomMusic; //

    // Volume multipliers for each surface type (adjust these values based on the loudness of your recordings)
    public float fallbackVolume = 0.2f; // Volume for fallback music
    public float grassRoomVolume = 0.2f;  // Grass music volume (default)
    public Transform listenerTransform;    // Reference to the camera that has the listener
    private CharacterController characterController;
    private float targetVolume = 0.5f; // Bring all music volume towards this value.
    private AudioSource audioSource;
    private float fadeDuration = 2;
    AudioClip selectedMusic;

    private void Start()
    {
        audioSource = listenerTransform.GetComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        selectedMusic = findCorrectMusicTrack(other);

        if (selectedMusic != null && !audioSource.isPlaying)
        { // It won't play if an audio is not found, or it won't play if a music is already playing.
            audioSource.clip = selectedMusic;
            audioSource.volume = 0;
            audioSource.Play();
            StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, targetVolume));
        }

    }
    private void OnTriggerExit(Collider other)
    {
        selectedMusic = findCorrectMusicTrack(other);

        if (audioSource.clip == selectedMusic)
        { // It won't play if an audio is not found, or it won't play if a music is already playing.
            audioSource.clip = selectedMusic;
            StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, 0));
        }

    }

    private AudioClip findCorrectMusicTrack(Collider other)
    {
        AudioClip selectedTrack = null;

        if (other.CompareTag("MusicGrassroom"))
        {
            selectedTrack = GrassRoomMusic;
        }

        /*else if (other.CompareTag("MusicStoneroom")){
            selectedMusic = StoneRoomMusic;
            audioSource.clip = selectedMusic; 
            selectedMusic.Play();
        }*/

        return selectedTrack;
    }


}
