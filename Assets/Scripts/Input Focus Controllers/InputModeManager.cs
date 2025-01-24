using UnityEngine;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    public InputSystem_Actions inputActions;

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
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void SwitchToPlayerControls()
    {
        inputActions.Flying.Disable();  // Disable Flying action map
        inputActions.Player.Enable();   // Enable Player action map
    }

    public void SwitchToShipControls()
    {
        inputActions.Player.Disable();  // Disable Player action map
        inputActions.Flying.Enable();   // Enable Flying action map
    }
}
