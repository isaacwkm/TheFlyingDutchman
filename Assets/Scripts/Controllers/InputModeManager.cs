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
        SwitchToPlayerControls();
    }

    private void OnDisable()
    {
        DisableAllControls();
    }

    public void DisableAllControls()
    {
        inputActions.Disable();
        inputMode = InputMode.None;
    }

    public void SwitchToPlayerControls()
    {
        inputActions.Disable();
        inputActions.Player.Enable();   // Enable Player action map
        inputMode = InputMode.Player;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SwitchToShipControls()
    {
        inputActions.Disable();
        inputActions.Flying.Enable();   // Enable Flying action map
        inputMode = InputMode.Flying;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SwitchToUIControls()
    {
        inputActions.Disable();
        inputActions.UI.Enable();
        inputMode = InputMode.UI;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
