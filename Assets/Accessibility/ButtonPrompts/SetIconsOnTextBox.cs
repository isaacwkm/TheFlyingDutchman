using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Needle.Console;

[RequireComponent(typeof(TMP_Text))]
public class SetIconsOnTextBox : MonoBehaviour
{
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
    }

    public void SetText(string formattedText)
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

        // Set the final formatted text
        textbox.text = formattedText;
    }

    // Dynamically select which sprite asset to use based on the device
    private TMP_SpriteAsset SelectSpriteAssetBasedOnDevice()
    {
        InputModeManager.ControlDeviceType deviceType = InputModeManager.Instance.GetCurrentDeviceType();

        switch (deviceType)
        {
            case InputModeManager.ControlDeviceType.Gamepad:
                return gamepadSpriteAsset;
            case InputModeManager.ControlDeviceType.Keyboard:
                return keyboardSpriteAsset;
            default:
                D.LogError("DeviceType doesn't exist in SelectSpriteAssetBasedOnDevice()!", gameObject, "Able");
                return null;
        }
    }

}
