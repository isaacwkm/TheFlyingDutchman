// Attach this script to same gameObject with TMPro text component on it: e.g. your tooltip component.

using UnityEngine;
using TMPro;
using Needle.Console;
using Unity.VisualScripting;

[RequireComponent(typeof(TMP_Text))]
public class TMProSpriteAssetTextSetter : MonoBehaviour
{
    // References to TMP_SpriteAssets for gamepad and keyboard
    [SerializeField] private TMP_SpriteAsset gamepadSpriteAsset;
    [SerializeField] private TMP_SpriteAsset keyboardSpriteAsset;
    private TMP_Text textbox;

    private void Awake()
    {
        textbox = GetComponent<TMP_Text>();
    }

    public void SetText(string formattedText){
        UpdateSpriteAsset();
        textbox.text = formattedText;
    }
    private void UpdateSpriteAsset()
    {
        // Dynamically select the sprite asset based on the device
        TMP_SpriteAsset selectedSpriteAsset = SelectSpriteAssetBasedOnDevice();

        // Dynamically set the sprite asset for the TMP_Text
        textbox.spriteAsset = selectedSpriteAsset;
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
