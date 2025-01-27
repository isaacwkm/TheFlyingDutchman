using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class Sound : MonoBehaviour
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.1f; // Volume - How loud the sound will play
    [Range(0.1f, 3f)]
    public float pitch = 1; // Pitch - Changes the pitch of the sound
    [Range(0f, 1f)]
    public float spatialBlend = 1; // Spatial Blend - 0 for 2D, 1 for 3D

    [HideInInspector]
    public AudioSource source;

    private void Awake()
    {
        AudioManager.Instance.RegisterSound(this);
    }
}