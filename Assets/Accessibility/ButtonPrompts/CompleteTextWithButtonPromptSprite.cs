// Credit - Christina Creates Games (Youtube tutorial for setting button prompts)
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CompleteTextWithButtonPromptSprite
{
    public static string ReadAndReplaceBinding(string textToDisplay, InputBinding actionNeeded, TMP_SpriteAsset spriteAsset)
    {
        string stringButtonName = GetReadableBindingName(actionNeeded);
        
        if (string.IsNullOrEmpty(stringButtonName))
            return textToDisplay; // Return unchanged if binding is invalid
        
        textToDisplay = textToDisplay.Replace("BUTTONPROMPT", 
        $"<sprite=\"{spriteAsset.name}\" name=\"{stringButtonName}\">");

        return textToDisplay;
    }

    private static string GetReadableBindingName(InputBinding binding)
    {
        string path = binding.effectivePath ?? binding.path; // Use effective binding if available
        
        if (string.IsNullOrEmpty(path))
            return string.Empty; // Invalid binding

        path = path.Replace("<Keyboard>/", "Keyboard_")
                   .Replace("<Gamepad>/", "Gamepad_")
                   .Replace("<Mouse>/", "Mouse_");

        return path;
    }
}

