using UnityEngine;
using UnityEngine.Rendering;

public class HighContrastPref : MonoBehaviour
{
    [SerializeField] private Transform indoorCheckStart;
    [SerializeField] private float indoorCheckDistance = 30.0f;
    [SerializeField] private Volume renderVolume;
    [SerializeField] private VolumeProfile defaultProfile;
    [SerializeField] private VolumeProfile highContrastIndoorProfile;
    [SerializeField] private VolumeProfile highContrastOutdoorProfile;
    private bool hasChangedVolumeProfile;

    public static bool highContrast;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadPref();
    }

    public static bool LoadPref()
    {
        return highContrast =
            PlayerPrefs.HasKey("highContrast") ?
            (PlayerPrefs.GetInt("highContrast") != 0) :
            false;
    }

    public static void SavePref()
    {
        PlayerPrefs.SetInt("highContrast", highContrast ? 1 : 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!highContrast)
        {
            renderVolume.profile = defaultProfile;
        }
        else if (IsIndoors())
        {
            renderVolume.profile = highContrastIndoorProfile;
        }
        else
        {
            renderVolume.profile = highContrastOutdoorProfile;
        }
    }

    public bool IsIndoors()
    {
        LayerMask layerMask = LayerMask.GetMask("Default");
        return !!Physics.Raycast(
            indoorCheckStart.position,
            Vector3.up,
            indoorCheckDistance,
            layerMask
        );
    }
}
