using System;
using UnityEngine;

public class InputEventDispatcher : MonoBehaviour
{
    public static event Action<Vector2> OnMovementInput;
    public static event Action<bool> OnJumpInput;
    public static event Action<bool> OnCrouchInput;
    public static event Action<bool> OnInteractInput;
    private static GameObject inputFocusOwner = null;

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

        // Interact
        if (Input.GetButtonDown("Interact")) {
            OnInteractInput?.Invoke(true);
        } else if (Input.GetButtonUp("Interact")) {
            OnInteractInput?.Invoke(false);
        }
    }

    public static bool acquireInputFocus(GameObject whom) {
        if (holdsInputFocus(whom)) {
            return true;
        } else if (inputFocusOwner == null) {
            inputFocusOwner = whom;
            return true;
        } else {
            return false;
        }
    }

    public static bool acquireInputFocus(Component whom) {
        return acquireInputFocus(whom.gameObject);
    }

    public static bool holdsInputFocus(GameObject whom) {
        return inputFocusOwner == whom || (
            inputFocusOwner == null &&
            whom.GetComponent<PlayerCharacterMovement>() != null
        );
    }

    public static bool holdsInputFocus(Component whom) {
        return holdsInputFocus(whom.gameObject);
    }

    public static void relinquishInputFocus(GameObject whom) {
        if (inputFocusOwner == whom) {
            inputFocusOwner = null;
        }
    }

    public static void relinquishInputFocus(Component whom) {
        relinquishInputFocus(whom.gameObject);
    }
}
