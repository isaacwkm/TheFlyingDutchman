using UnityEngine;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
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
        inputActions.Flying.Disable();  // Disable Ship action map
        inputActions.Player.Enable(); // Enable Player action map
    }

    public void SwitchToShipControls()
    {
        inputActions.Player.Disable(); // Disable Player action map
        inputActions.Flying.Enable();    // Enable Ship action map
    }
}