using UnityEngine;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    public enum ControlDeviceType
    {
        Keyboard = 0,
        Gamepad = 1
    }

public enum InputMode {
        None,
        Player,
        Flying,
        UI
    }
    public InputSystem_Actions inputActions;
    public InputActionMap currentActionMap;
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

    private PlayerInput playerInput;

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
        currentActionMap = null;
    }

    public void SwitchToPlayerControls()
    {
        inputActions.Disable();
        inputActions.Player.Enable();   // Enable Player action map
        inputMode = InputMode.Player;
        currentActionMap = inputActions.Player;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SwitchToShipControls()
    {
        inputActions.Disable();
        inputActions.Flying.Enable();   // Enable Flying action map
        inputMode = InputMode.Flying;
        currentActionMap = inputActions.Flying;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SwitchToUIControls()
    {
        inputActions.Disable();
        inputActions.UI.Enable();
        inputMode = InputMode.UI;
        currentActionMap = inputActions.UI;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Getters Setters

    public PlayerInput GetPlayerInput(){
        return playerInput;
    }

    public InputBinding GetBinding(string actionName, ControlDeviceType deviceType){

        InputActionMap actionMap = GetCurrentActionMap();
        InputAction action = actionMap.FindAction(actionName);

        InputBinding deviceBinding = action.bindings[(int)deviceType];
        return deviceBinding;
    }

    public InputActionMap GetCurrentActionMap()
    {
        return currentActionMap;
    }

    public int GetCurrentDeviceType(){
        return (int)ControlDeviceType.Gamepad;
    }

    
}
