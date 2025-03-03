using UnityEngine;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;

public class InputPromptReplacer : MonoBehaviour
{
    public InputActionAsset inputActions; // Assign in Inspector

    [TextArea(3, 5)]
    public string inputText = "Press BUTTONPROMPT.Player.Jump to jump!";

    private static readonly Regex buttonPromptRegex = new Regex(@"BUTTONPROMPT\.(\w+)\.(\w+)", RegexOptions.Compiled);

    [ContextMenu("Replace Button Prompts")]
    public void ReplaceButtonPrompts()
    {
        inputText = buttonPromptRegex.Replace(inputText, match =>
        {
            string controlMode = match.Groups[1].Value; // e.g., Player, Vehicle, UI
            string actionName = match.Groups[2].Value;  // e.g., Jump, Accelerate

            return GetBindingString(controlMode, actionName) ?? "UNBOUND";
        });

        Debug.Log($"Updated Text: {inputText}");
    }

    private string GetBindingString(string controlMode, string actionName)
    {
        var actionMap = inputActions.FindActionMap(controlMode);
        if (actionMap == null) return null;

        var action = actionMap.FindAction(actionName);
        if (action == null) return null;

        return action.GetBindingDisplayString() ?? "???";
    }
}
