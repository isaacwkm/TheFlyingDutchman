// Credit - Christina Creates Games (Youtube tutorial for setting button prompts)
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CompleteTextWithButtonPromptSprite
{
    public static string ReadAndReplaceBinding(string textToDisplay, InputBinding actionNeeded, TMP_SpriteAsset spriteAsset)
    {
        string stringButtonName = actionNeeded.ToString();
        stringButtonName = RenameInput(stringButtonName);

        textToDisplay = textToDisplay.Replace("BUTTONPROMPT", $"<sprite=\"{spriteAsset.name}\" name \"{stringButtonName}\">");

        return textToDisplay;
    }

    private static string RenameInput(string stringButtonName){
        stringButtonName = stringButtonName.Replace("Interact", String.Empty);

        stringButtonName = stringButtonName.Replace("<Keyboard>/", "Keyboard_");
        stringButtonName = stringButtonName.Replace("<Gamepad>/", "Gamepad_");

        return stringButtonName;
    }
}
