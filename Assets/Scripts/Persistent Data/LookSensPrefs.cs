using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookSensPrefs : MonoBehaviour
{
    void Start()
    {
        LoadPrefs();
    }

    public static float mouseSensitivity;
    public static float rightStickSensitivity;

    public static void LoadPrefs()
    {
        mouseSensitivity = (
            PlayerPrefs.HasKey("mouseSensitivity") ?
            PlayerPrefs.GetFloat("mouseSensitivity") :
            0.5f
        );
        rightStickSensitivity = (
            PlayerPrefs.HasKey("rightStickSensitivity") ?
            PlayerPrefs.GetFloat("rightStickSensitivity") :
            0.5f
        );
    }

    public static void SavePrefs()
    {
        PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetFloat("rightStickSensitivity", rightStickSensitivity);
    }

    public static float GetMultiplier(InputAction.CallbackContext ctx)
    {
        string deviceClass = ctx.control.device.description.deviceClass;
        if (deviceClass == "Gamepad" || deviceClass == "Joystick")
        {
            return rightStickSensitivity*25.0f;
        }
        else if (deviceClass == "Mouse" || deviceClass == "Touchpad")
        {
            return mouseSensitivity*2.0f;
        }
        else
        {
            return 1.0f;
        }
    }
}
