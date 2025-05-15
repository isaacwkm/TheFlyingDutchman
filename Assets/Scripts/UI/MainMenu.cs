using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class MainMenu : UIStack.Context
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle highContrastToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button rebindControlsButton;
    [SerializeField] private Button metricsButton;
    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button backToOSButton;
    [SerializeField] private ControlsMenu controlsMenuPrefab;
    [SerializeField] private UIStack.Context metricsMenuPrefab;
    [SerializeField] private UIStack.Context confirmQuitPrefab;

    private InputModeManager inputMan;

    override protected void Awake()
    {
        base.Awake();
        inputMan = InputModeManager.Instance;
    }

    override protected void OnEnable()
    {
        base.OnEnable();
        // If we're enabling and start doesn't run afterward,
        // that means we're enabling from being disabled,
        // which means we're coming back from a submenu.
        // The only submenu is the controls rebind submenu.
        // If start does run afterward, it will override this selection operation
        EventSystem.current?.SetSelectedGameObject(rebindControlsButton.gameObject);
    }

    void Start()
    {
        // TODO: implement functionality:
        //      highContrastToggle
        EventSystem.current?.SetSelectedGameObject(fullScreenToggle.gameObject);
        fullScreenToggle.isOn = FullScreenPref.LoadPref();
        fullScreenToggle.onValueChanged.AddListener((state) => FullScreenPref.SavePref(state));
        highContrastToggle.isOn = HighContrastPref.LoadPref();
        highContrastToggle.onValueChanged.AddListener((state) => {
            HighContrastPref.highContrast = state;
            HighContrastPref.SavePref();
        });
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
        metricsButton.onClick.AddListener(() => Call(metricsMenuPrefab));
        backToGameButton.onClick.AddListener(() => Return());
        backToOSButton.onClick.AddListener(() => Call(confirmQuitPrefab));
    }

}
