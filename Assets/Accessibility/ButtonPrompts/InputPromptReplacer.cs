using UnityEngine;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;
using TMPro;
using Needle.Console;
using System;

// Apply component to a ButtonPromptZone (Game Object with a trigger collider)

public class InputPromptReplacer : MonoBehaviour
{
    [Tooltip("Set this so that the tooltip only shows up when the player is in the defined input mode. Leave as \"None\" to show always. WARNING: Not recommended for most use cases.")]
    public InputModeManager.InputMode explicitInputMode = InputModeManager.InputMode.None; // Leave as "None" to show always. WARNING: Not recommended.

    [TextArea(3, 5)]
    public string originalInputText = "Press BUTTONPROMPT.Jump to jump!"; // example, to be replaced for whatever your uses are
    public TooltipManager tooltipManager;
    private string inputTextConverted = "Converted inputText will result here. Manually modifying in this value in the inspector or in code has no effect.";

    private static readonly Regex buttonPromptRegex = new Regex(@"BUTTONPROMPT\.(\w+)", RegexOptions.Compiled);
    // effectivePathRegex parses UnityEngine InputBinding's .effectivePath property that returns control path strings that look like, for example, \"<Gamepad>/buttonSouth\".")
    private static readonly Regex effectivePathRegex = new Regex(@"^<(?<device>[^>]+)>\/(?<input>.+)$");
    private InputModeManager inputManager;
    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputManager = InputModeManager.Instance;
        inputActions = inputManager.inputActions;
    }

    private string DynamicConvert()
    {
        // The variable below, controlDeviceType, is an enum defined in InputModeManager. It contains all supported controller types.
        InputModeManager.ControlDeviceType controlDeviceType = inputManager.GetCurrentDeviceType();
        inputTextConverted = originalInputText;
        inputTextConverted = buttonPromptRegex.Replace(originalInputText, match =>
        {
            string actionName = match.Groups[1].Value; // ACTION name e.g., Jump, Sprint, etc. Automatically assumes actionMap context.

            return GetSpriteTag(actionName, controlDeviceType) ?? "UNBOUND";
        });
        return inputTextConverted;
    }

    public void ShowAtSecondaryTooltip() // Show at the center of the screen where the primary tooltip is. Automatically hides when a primary tooltip is active.
    {

    }

    public void HideAtSecondaryTooltip() // Hides manually
    {

    }

    public void ShowAtTopLeftTooltip() // Show at top left of the screen. Calling this method repeatedly appends a new tooltip below the last tip.
    {

    }

    public void HideAtTopLeftTooltip() // Remove an element from the list of tooltips in the top left.
    {

    }

    public void HideAllAtTopLeftTooltip() // Remove all elements from the top left screen tooltip.
    {

    }

    private string GetSpriteTag(string actionName, InputModeManager.ControlDeviceType deviceType)
    {
        InputBinding dynamicBinding = inputManager.GetBinding(actionName, deviceType); // Finds the binding using only a name and current device type

        (string device, string input) effectivePath = parseEffectivePath(dynamicBinding.effectivePath); // Grabs the effective path using Unity's API, then parses it into two separate strings, excluding all the weird symbols by using Regex.

        D.Log($"GetSpriteTag() - Device: {effectivePath.device} and Input: {effectivePath.input}", gameObject, "Able");

        string spriteTagName = effectivePath.device + "_" + effectivePath.input; // Sprite tag follow a naming convention outlined in this expression
        return spriteTagName;
    }

    private (string device, string input) parseEffectivePath(string effectivePath)
    {

        Match match = effectivePathRegex.Match(effectivePath);

        if (match.Success)
        {
            string device = match.Groups["device"].Value;
            string input = match.Groups["input"].Value;
            return (device, input);
        }
        else
        {
            D.LogError("Invalid input path in parseEffectivePath()", gameObject, "Any");
            return ("", "");
        }

    }
}
