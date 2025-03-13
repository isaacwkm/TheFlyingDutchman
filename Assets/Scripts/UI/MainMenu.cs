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
        GetEventSystem()?.SetSelectedGameObject(rebindControlsButton.gameObject);
    }

    void Start()
    {
        // TODO: implement functionality:
        //      highContrastToggle
        GetEventSystem().SetSelectedGameObject(fullScreenToggle.gameObject);
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
