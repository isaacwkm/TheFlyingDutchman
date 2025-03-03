using UnityEngine;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;
using TMPro;
using Needle.Console;
using System;

// Apply component to a ButtonPromptZone (Game Object with a trigger collider)
public class InputPromptReplacer : MonoBehaviour
{
    public InputModeManager inputManager;
    public InputSystem_Actions inputActions;
    private TMP_SpriteAsset currentSpriteAsset;

    [TextArea(3, 5)]
    public string inputText = "Press BUTTONPROMPT.Player.Jump to jump!";

    private static readonly Regex buttonPromptRegex = new Regex(@"BUTTONPROMPT\.(\w+)\.(\w+)", RegexOptions.Compiled);
    // effectivePathRegex parses UnityEngine InputBinding's .effectivePath property that returns control path strings that look like, for example, \"<Gamepad>/buttonSouth\".")
    private static readonly Regex effectivePathRegex = new Regex(@"^<(?<device>[^>]+)>\/(?<input>.+)$");

    void Awake()
    {
        inputManager = InputModeManager.Instance;
        inputActions = inputManager.inputActions;
    }


    [ContextMenu("Replace Button Prompts")]
    // public void ReplaceButtonPrompts()
    // {
    //     inputText = buttonPromptRegex.Replace(inputText, match =>
    //     {
    //         string controlMode = match.Groups[1].Value; // e.g., Player, Vehicle, UI
    //         string actionName = match.Groups[2].Value;  // e.g., Jump, Accelerate

    //         return GetBindingString(controlMode, actionName) ?? "UNBOUND";
    //     });

    //     Debug.Log($"Updated Text: {inputText}");
    // }

    private string GetSpriteTag(string actionName, ControlDeviceType deviceType)
    {
        InputBinding dynamicBinding = inputManager.GetBinding(actionName, deviceType);
        
        (string device, string input) effectivePath = parseEffectivePath(dynamicBinding.effectivePath); // Grabs the effective path using Unity's API, then parses it into two separate strings, excluding all the weird symbols by using Regex.
        
        D.Log($"GetSpriteTag() - Device: {effectivePath.device} and Input: {effectivePath.input}", gameObject, "Able");

        string spriteTagName = effectivePath.device + "_" + effectivePath.input; // Sprite tag follow a naming convention outlined in this expression
        return spriteTagName;
    }

    private (string device, string input) parseEffectivePath(string effectivePath){
        
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
