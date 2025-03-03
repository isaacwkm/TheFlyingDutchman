using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Needle.Console;

public class TopLeftTooltipManager : MonoBehaviour
{
    public GameObject messagePrefab; // A prefab with TextMeshProUGUI
    public Transform messageContainer; // Parent GameObject with Vertical Layout Group

    private readonly Queue<GameObject> messages = new(); // FIFO structure
    private InputModeManager.InputMode previousInputMode = InputModeManager.InputMode.None;
    private InputModeManager.InputMode currentInputMode = InputModeManager.InputMode.None;
    private List<GameObject> currentInputModeMessages = new List<GameObject>();

    void OnEnable()
    {
        InputModeManager.Instance.OnInputModeSwitch += HandleInputModeSwitch;
    }

    void OnDisable()
    {
        InputModeManager.Instance.OnInputModeSwitch -= HandleInputModeSwitch;
    }

    private GameObject AddMessage(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, messageContainer);
        newMessage.GetComponent<TMP_Text>().text = message;

        messages.Enqueue(newMessage);

        return newMessage;
    }

    public void RemoveMessage(GameObject messageObject)
    {
        if (messages.Contains(messageObject))
        {
            messages.Dequeue();
            Destroy(messageObject);
        }
    }

    public void RemoveAllMessages()
    {
        while (messages.Count > 0)
        {
            Destroy(messages.Dequeue());
        }
    }

    private void HandleInputModeSwitch()
    {
        previousInputMode = currentInputMode;
        InputModeManager.InputMode nextInputMode = InputModeManager.Instance.inputMode;
        currentInputMode = nextInputMode;

        switch (currentInputMode)
        {
            case InputModeManager.InputMode.Player:
                HideAllMessagesFromPreviousMode();
                return;
            case InputModeManager.InputMode.Flying:
                HandleShipModeMessages();
                return;
            case InputModeManager.InputMode.UI:
                HideAllMessagesFromPreviousMode();
                return;
            default:
                D.Log("TopLeftPrompts.cs: Did you add a new input mode?!", gameObject, "Any");
                return;
        }

    }

    private void HideAllMessagesFromPreviousMode()
    {
        for (int i = 0; i < currentInputModeMessages.Count; i++)
        {
            RemoveMessage(currentInputModeMessages[i]);
        }
        currentInputModeMessages.Clear();
    }

    public GameObject AddMessageWithConversion(InputPromptReplacer inputPromptReplacer) // Parameter demands an InputPromptReplacer script in order to enforce proper text conversion, as opposed to asking for a string.
    {
        return AddMessage(inputPromptReplacer.SetConvertedText_SlowPerformance());
    }

    public void HandleShipModeMessages()
    {
        HideAllMessagesFromPreviousMode();
        ShowAllShipControlsTopLeft();
    }

    private void ShowAllShipControlsTopLeft()
    {
        const string ascendMessageText = "Ascend: BUTTONPROMPT.Ascend";
        const string descendMessageText = "Descend: BUTTONPROMPT.Descend";
        const string exitMessageText = "Exit: BUTTONPROMPT.Jump_Off";

        // Use a helper method to create, disable, and set up components for each message
        SetupMessage(ascendMessageText, InputModeManager.InputMode.Flying);
        SetupMessage(descendMessageText, InputModeManager.InputMode.Flying);
        SetupMessage(exitMessageText, InputModeManager.InputMode.Flying);
    }

    private void SetupMessage(string messageText, InputModeManager.InputMode explicitInputMode)
    {
        // Instantiate message and fetch components
        GameObject message = AddMessage(messageText);
        currentInputModeMessages.Add(message); // add to list of current messages generated from current input mode
        MessageComponents messageComponents = GetMessageComponents(message);

        // Hide text initially
        messageComponents.TMProComponent.enabled = false;

        // Set up class variables within each component
        ConfigureInputPromptReplacer(messageComponents, explicitInputMode);

        // Perform slow text conversion for button prompts and set it to the original message's text
        messageComponents.TMProComponent.text = messageComponents.InputPromptReplacer.SetConvertedText_SlowPerformance();

        // Show text afterewards
        messageComponents.TMProComponent.enabled = true;
    }

    // Create a class to represent the components used in the next couple of methods below
    public class MessageComponents
    {
        public TextMeshProUGUI TMProComponent { get; set; }
        public InputPromptReplacer InputPromptReplacer { get; set; }
        public TMProSpriteAssetTextSetter SpriteAssetSetter { get; set; }

        public MessageComponents(TextMeshProUGUI tmproComponent, InputPromptReplacer inputPromptReplacer, TMProSpriteAssetTextSetter spriteAssetSetter)
        {
            TMProComponent = tmproComponent;
            InputPromptReplacer = inputPromptReplacer;
            SpriteAssetSetter = spriteAssetSetter;
        }
    }
    private void ConfigureInputPromptReplacer(MessageComponents components, InputModeManager.InputMode explicitInputMode)
    {
        string originalText = components.TMProComponent.text;
        components.InputPromptReplacer.SetOriginalInputText(originalText);
        components.InputPromptReplacer.TMProTextToReplace = components.SpriteAssetSetter;
        components.InputPromptReplacer.explicitInputMode = explicitInputMode;
    }

    private MessageComponents GetMessageComponents(GameObject message)
    {
        var tmproComponent = message.GetComponent<TextMeshProUGUI>();
        var inputPromptReplacer = message.GetComponent<InputPromptReplacer>();
        var spriteAssetSetter = message.GetComponent<TMProSpriteAssetTextSetter>();

        return new MessageComponents(tmproComponent, inputPromptReplacer, spriteAssetSetter);
    }

}
