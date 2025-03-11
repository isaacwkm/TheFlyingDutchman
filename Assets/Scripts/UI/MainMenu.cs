using UnityEngine;
using UnityEngine.UI;
using System;

public class MainMenu : UIStack.Context
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle highContrastToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button rebindControlsButton;
    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button backToOSButton;
    [SerializeField] private ControlsMenu controlsMenuPrefab;

    public event Action<bool> MainMenuActive;
    public event Action<float> OnVolumeChanged; // Event for volume updates

    private InputModeManager inputMan;

    private void Awake()
    {
        inputMan = InputModeManager.Instance;
    }

    private void OnEnable()
    {
        AudioManager.Instance.MainMenuSoundSettings = this;
        // TODO: certain UI input actions should navigate or close menu
        // w/o requiring use of on-screen buttons
    }

    private void OnDisable()
    {
        AudioManager.Instance.MainMenuSoundSettings = null;
    }

    private void Start()
    {
        // TODO: implement functionality:
        //      highContrastToggle
        //      volumeSlider
        fullScreenToggle.isOn = FullScreenPref.LoadPref();
        fullScreenToggle.onValueChanged.AddListener((state) => FullScreenPref.SavePref(state));
        rebindControlsButton.onClick.AddListener(() => Call(controlsMenuPrefab));
        backToGameButton.onClick.AddListener(() => Return());
        volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
        // TODO: implement backToOSButton listener
    }

    private void HandleVolumeChanged(float sliderValue)
    {
        OnVolumeChanged?.Invoke(sliderValue); // Broadcast event
    }

}