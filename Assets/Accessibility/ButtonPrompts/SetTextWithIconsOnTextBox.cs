// Credit - Christina Creates Games (Youtube tutorial for setting button prompts)
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Needle.Console;

[RequireComponent(typeof(TMP_Text))]
public class SetTextWithIconsOnTextBox : MonoBehaviour
{
    [TextArea(2, 3)]
    [SerializeField] private string message = "Press BUTTONPROMPT to interact.";

    [Header("Setup for sprites")]
    [SerializeField] private ListOfTMProSpriteAssets listOfTMProSpriteAssets;
    [SerializeField] private DeviceType deviceType;

    private InputSystem_Actions inputActions;
    private TMP_Text textbox;

    private void Awake()
    {
        inputActions = InputModeManager.Instance.inputActions;
        textbox = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        SetText();
    }

    [ContextMenu("Set Text")]
    private void SetText()
    {
        if ((int)deviceType >= listOfTMProSpriteAssets.SpriteAssets.Count)
        {
            D.Log($"Missing Sprite Asset for {deviceType}");
            return;
        }

        // Get the correct input binding
        InputBinding binding = GetBindingForDevice(deviceType);
        if (binding == default)
        {
            D.Log($"No valid binding found for {deviceType}");
            return;
        }

        // Replace BUTTONPROMPT with actual button sprite
        textbox.text = CompleteTextWithButtonPromptSprite.ReadAndReplaceBinding(
            message,
            binding,
            listOfTMProSpriteAssets.SpriteAssets[(int)deviceType]
        );
    }

    private InputBinding GetBindingForDevice(DeviceType device)
    {
        var bindings = inputActions.Player.Interact.bindings;
        foreach (var binding in bindings)
        {
            if (device == DeviceType.Gamepad && binding.path.Contains("Gamepad"))
                return binding;
            if (device == DeviceType.Keyboard && binding.path.Contains("Keyboard"))
                return binding;
        }
        return default; // Return default if no valid binding is found
    }

    private enum DeviceType
    {
        Gamepad = 0,
        Keyboard = 1
    }
}
