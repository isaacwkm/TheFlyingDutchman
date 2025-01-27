using UnityEngine;
using System.Collections.Generic;

public class SingleSoundComponent : MonoBehaviour
{
    [Tooltip("List of audio clips for this sound.")]
    public List<AudioClip> clips;  // Array of clips for the sound

    [Range(0f, 1f)]
    public float volume = 0.05f;  // Volume - How loud the sound will play
    [Range(0.1f, 3f)]
    public float pitch = 1;  // Pitch - Changes the pitch of the sound
    [Range(0f, 1f)]
    public float spatialBlend = 1;  // Spatial Blend - 0 for 2D, 1 for 3D

    [HideInInspector]
    public AudioSource source;  // AudioSource used to play the sound

    private void Awake()
    {
        // Register this sound with the AudioManager
        AudioManager.Instance.RegisterSound(this);
    }

    /// Plays a specific sound clip from the list by index.
    public void PlaySoundAtIndex(int clipIndex)
    {
        AudioManager.Instance.PlaySoundAt(this, clipIndex);
    }


    /// Plays a random sound clip from the list.
    public int PlaySoundRandom()
    {
        return AudioManager.Instance.PlayRandomSound(this);
    }
}
