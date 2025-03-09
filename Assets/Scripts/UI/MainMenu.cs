using UnityEngine;
using UnityEngine.UI;

public class MainMenu : UIStack.Context
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle highContrastToggle;
    [SerializeField] private Slider volumeSlider;
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
        //      volumeSlider
        fullScreenToggle.isOn = FullScreenPref.LoadPref();
        fullScreenToggle.onValueChanged.AddListener((state) => FullScreenPref.SavePref(state));
        rebindControlsButton.onClick.AddListener(() => Call(controlsMenuPrefab));
        backToGameButton.onClick.AddListener(() => Return());
        // TODO: implement backToOSButton listener
    }
}
