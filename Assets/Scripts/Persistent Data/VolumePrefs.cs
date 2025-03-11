using System;
using UnityEngine;

public class VolumePrefs : MonoBehaviour
{
    void Start()
    {
        LoadPrefs();
    }

    public static event Action<float> OnMusicVolumeChanged;

    public static float masterVolume
    {
        get
        {
            return AudioListener.volume;
        }
        set
        {
            AudioListener.volume = value;
        }
    }

    private static float _musicVolume = 1.0f;

    public static float musicVolume
    {
        get
        {
            return _musicVolume;
        }
        set
        {
            _musicVolume = value;
            OnMusicVolumeChanged?.Invoke(value);
        }
    }

    public static void LoadPrefs()
    {
        masterVolume = (
            PlayerPrefs.HasKey("masterVolume") ?
            PlayerPrefs.GetFloat("masterVolume") :
            1.0f
        );
        musicVolume = (
            PlayerPrefs.HasKey("musicVolume") ?
            PlayerPrefs.GetFloat("musicVolume") :
            1.0f
        );
    }

    public static void SavePrefs()
    {
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
    }
}
