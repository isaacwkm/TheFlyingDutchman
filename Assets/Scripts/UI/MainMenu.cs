using UnityEngine;
using UnityEngine.UI;
using System;

public class MainMenu : UIStack.Context
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle highContrastToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button rebindControlsButton;
    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button backToOSButton;
    [SerializeField] private ControlsMenu controlsMenuPrefab;

    private InputModeManager inputMan;

    private void Awake()
    {
        inputMan = InputModeManager.Instance;
    }

    private void OnEnable()
    {
        // TODO: certain UI input actions should navigate or close menu
        // w/o requiring use of on-screen buttons
    }

    private void Start()
    {
        // TODO: implement functionality:
        //      highContrastToggle
        fullScreenToggle.isOn = FullScreenPref.LoadPref();
        fullScreenToggle.onValueChanged.AddListener((state) => FullScreenPref.SavePref(state));
        VolumePrefs.LoadPrefs();
        masterVolumeSlider.value = VolumePrefs.masterVolume;
        masterVolumeSlider.onValueChanged.AddListener((value) => {
            VolumePrefs.masterVolume = value;
            VolumePrefs.SavePrefs();
        });
        musicVolumeSlider.value = VolumePrefs.musicVolume;
        musicVolumeSlider.onValueChanged.AddListener((value) => {
            VolumePrefs.musicVolume = value;
            VolumePrefs.SavePrefs();
        });
        rebindControlsButton.onClick.AddListener(() => Call(controlsMenuPrefab));
        backToGameButton.onClick.AddListener(() => Return());
        // TODO: implement backToOSButton listener
    }

}
