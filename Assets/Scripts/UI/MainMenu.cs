using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
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

    private void Close()
    {
        // TODO: Add a stack-based UI system to SceneCore
        // so we can remember if we got to the menu from player mode or flying mode
        // even if we're returning from a submenu.
        // A stack-based UI system is just generally a good idea,
        // for many reasons besides this.
        // For now, always assume we got here from player controls,
        // for lack of a better alternative.
        inputMan.SwitchToPlayerControls();
        Destroy(this.gameObject);
    }

    private void Start()
    {
        transform.SetParent(SceneCore.canvas.transform, false);
        transform.SetLocalPositionAndRotation(3*Vector3.back, Quaternion.identity);
        inputMan.SwitchToUIControls();
        // TODO: implement functionality for everything except buttons:
        //      fullScreenToggle
        //      highContrastToggle
        //      volumeSlider
        rebindControlsButton.onClick.AddListener(() => {
            Instantiate(controlsMenuPrefab);
            Destroy(this.gameObject);
        });
        backToGameButton.onClick.AddListener(() => Close());
        // TODO: implement backToOSButton listener
    }
}
