using UnityEngine;
using UnityEngine.InputSystem;

public class ControlRebindPrefs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadRebinds();
    }

    public static void SaveRebinds()
    {
        PlayerPrefs.SetString(
            "controlRebinds",
            InputModeManager.Instance.inputActions.SaveBindingOverridesAsJson()
        );
    }

    public static bool LoadRebinds()
    {
        if (PlayerPrefs.HasKey("controlRebinds"))
        {
            InputModeManager.Instance.inputActions.LoadBindingOverridesFromJson(
                PlayerPrefs.GetString("controlRebinds")
            );
            return true;
        }
        else
        {
            return false;
        }
    }
}
