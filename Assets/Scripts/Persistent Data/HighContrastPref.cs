using Needle.Console;
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

    private VolumeProfile currentProfile;  // cache to avoid redundant profile assignments
    private bool hasChangedVolumeProfile;

    public static bool highContrast;

    void Start()
    {
        LoadPref();
        currentProfile = null; // force assignment on first Update
    }

    public static bool LoadPref()
    {
        return highContrast = PlayerPrefs.HasKey("highContrast")
            ? PlayerPrefs.GetInt("highContrast") != 0
            : false;
    }

    public static void SavePref()
    {
        PlayerPrefs.SetInt("highContrast", highContrast ? 1 : 0);
    }

    void Update()
    {
        // VolumeProfile targetProfile = GetTargetProfile();

        // // Only reassign if the profile actually changes
        // if (renderVolume.profile != targetProfile)
        // {
        //     D.Log("Reassigning volume profile in HighContrastPref.", this, "PostProc");
        //     renderVolume.profile = targetProfile;
        //     currentProfile = targetProfile;
        // }
    }

    private VolumeProfile GetTargetProfile()
    {
        if (!highContrast)
            return defaultProfile;
        else if (IsIndoors())
            return highContrastIndoorProfile;
        else
            return highContrastOutdoorProfile;
    }

    public bool IsIndoors()
    {
        LayerMask layerMask = LayerMask.GetMask("Default");

        return Physics.Raycast(
            indoorCheckStart.position,
            Vector3.up,
            indoorCheckDistance,
            layerMask
        );
    }
}
