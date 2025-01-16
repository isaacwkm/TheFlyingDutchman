using System;
using UnityEngine;

public class InputEventDispatcher : MonoBehaviour
{
    public static event Action<Vector2> OnMovementInput;
    public static event Action<bool> OnJumpInput;
    public static event Action<bool> OnCrouchInput;

    void Update()
    {
        // Movement
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        OnMovementInput?.Invoke(movement);

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            OnJumpInput?.Invoke(true);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            OnJumpInput?.Invoke(false);
        }

        // Crouch
        //bool isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isCrouching = false;
        OnCrouchInput?.Invoke(isCrouching);
    }
}
