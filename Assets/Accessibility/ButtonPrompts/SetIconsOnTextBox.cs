using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TMP_Text))]
public class SetIconsOnTextBox : MonoBehaviour
{
    private string tmproMessage = "Current tooltip text is set here.";

    // References to TMP_SpriteAssets for gamepad and keyboard
    [SerializeField] private TMP_SpriteAsset gamepadSpriteAsset;
    [SerializeField] private TMP_SpriteAsset keyboardSpriteAsset;

    private TMP_Text textbox;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        textbox = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        inputActions = InputModeManager.Instance.inputActions;
        SetText("space");
    }

    [ContextMenu("Set Text")]
    public void SetText(string key)
    {
        // Dynamically select the sprite asset based on the device
        TMP_SpriteAsset selectedSpriteAsset = SelectSpriteAssetBasedOnDevice();

        // Ensure a sprite asset is set
        if (selectedSpriteAsset == null)
        {
            Debug.LogError("No sprite asset selected.");
            return;
        }

        // Dynamically set the sprite asset for the TMP_Text
        textbox.spriteAsset = selectedSpriteAsset;

        // Replace BUTTONPROMPT with the actual sprite (e.g., "Gamepad_buttonWest")
        string formattedText = tmproMessage.Replace("BUTTONPROMPT", $"<sprite name=\"{GetSpriteNameForDevice(key)}\">");

        // Set the final formatted text
        textbox.text = formattedText;
    }

    // Dynamically select which sprite asset to use based on the device
    private TMP_SpriteAsset SelectSpriteAssetBasedOnDevice()
    {
        return keyboardSpriteAsset;

        // // Choose the sprite asset based on the device being used
        // if (Gamepad.current != null) // If a gamepad is being used
        // {
        //     return gamepadSpriteAsset;
        // }
        // else if (Keyboard.current != null) // If a keyboard is being used
        // {
        //     return keyboardSpriteAsset;
        // }

        // return null; // If no device is found, return null
    }

    // Get the sprite name based on the device
    private string GetSpriteNameForDevice(string actionNeeded)
    {
        return "Keyboard_" + actionNeeded;

        return "Keyboard_space";
        // if (Gamepad.current != null)
        // {
        //     return "Gamepad_buttonWest"; // Example for gamepad
        // }
        // else if (Keyboard.current != null)
        // {
        //     return "Keyboard_space"; // Example for keyboard
        // }

        // return string.Empty; // Default return if no device is detected
    }
}
