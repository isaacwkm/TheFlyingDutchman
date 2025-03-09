using System;
using Needle.Console;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    public enum ControlDeviceType // Arrange in order they are bound in the new Unity Inputsystem InputAction asset.
    {
        Gamepad = 0,
        Keyboard = 1
    }

    public enum InputMode
    {
        None,
        Player,
        Flying,
        UI
    }
    public event Action OnInputModeSwitch;
    public InputSystem_Actions inputActions;
    private InputActionMap currentActionMap;
    public InputMode inputMode { get; protected set; }

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
        OnInputModeSwitch?.Invoke();
    }

    public void SwitchToShipControls()
    {
        D.Log($"Switched to Ship Controls.", gameObject, "Able");
        inputActions.Disable();
        inputActions.Flying.Enable();   // Enable Flying action map
        inputMode = InputMode.Flying;
        currentActionMap = inputActions.Flying;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        OnInputModeSwitch?.Invoke();
    }

    public void SwitchToUIControls()
    {
        inputActions.Disable();
        inputActions.UI.Enable();
        inputMode = InputMode.UI;
        currentActionMap = inputActions.UI;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        OnInputModeSwitch?.Invoke();
    }

    public void SwitchControls(InputMode mode)
    {
        switch (mode)
        {
            case InputMode.None: DisableAllControls(); break;
            case InputMode.Player: SwitchToPlayerControls(); break;
            case InputMode.Flying: SwitchToShipControls(); break;
            case InputMode.UI: SwitchToUIControls(); break;
        }
    }

    // Getters Setters

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }

    public InputBinding GetBinding(string actionName, ControlDeviceType deviceType)
    {

        InputActionMap actionMap = GetCurrentActionMap();
        InputAction action = actionMap.FindAction(actionName);
        D.Log($"GetBinding() - Action: {action.name}, ActionMap: {actionMap.name}", gameObject, "Able");

        InputBinding deviceBinding = action.bindings[(int)deviceType];
        return deviceBinding;
    }

    public InputActionMap GetCurrentActionMap()
    {
        return currentActionMap;
    }

    public ControlDeviceType GetCurrentDeviceType()
    {
        return ControlDeviceType.Keyboard;
    }

    public InputModeManager.InputMode gameplayControlMode
    {
        get
        {
            if (SceneCore.uiStack.context)
            {
                return SceneCore.uiStack.inputModeBeforeUI;
            }
            else
            {
                return inputMode;
            }
        }
    }
}
