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
    private void SetText(){
        if ((int)deviceType > listOfTMProSpriteAssets.SpriteAssets.Count - 1){
            D.Log($"Missing Sprite Asset for {deviceType}");
            return;
        }

        textbox.text = CompleteTextWithButtonPromptSprite.ReadAndReplaceBinding(
            message,
            inputActions.Player.Interact.bindings[(int)deviceType],
            listOfTMProSpriteAssets.SpriteAssets[(int)deviceType]
        );
    }

    private enum DeviceType{
        Keyboard = 0,
        Gamepad = 1
    }
}
