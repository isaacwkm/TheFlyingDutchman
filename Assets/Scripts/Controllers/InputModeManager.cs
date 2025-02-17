using UnityEngine;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    public InputSystem_Actions inputActions;

    public enum InputMode {
        None,
        Player,
        Flying,
        UI
    }
    public InputMode inputMode {get; protected set;}

    private static InputModeManager _instance;
    public static InputModeManager Instance // Singleton Pattern
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<InputModeManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure only one instance of InputModeManager exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Disable();
        SwitchToPlayerControls();
        inputMode = InputMode.Player;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputMode = InputMode.None;
    }

    public void SwitchToPlayerControls()
    {
        inputActions.Player.Enable();   // Enable Player action map
        inputActions.Flying.Disable();  // Disable Flying action map
        inputActions.UI.Disable();
        inputMode = InputMode.Player;
    }

    public void SwitchToShipControls()
    {
        inputActions.Player.Disable();  // Disable Player action map
        inputActions.Flying.Enable();   // Enable Flying action map
        inputActions.UI.Disable();
        inputMode = InputMode.Flying;
    }

    public void SwitchToUIControls()
    {
        inputActions.Player.Disable();
        inputActions.Flying.Disable();
        inputActions.UI.Enable();
        inputMode = InputMode.UI;
    }
}
