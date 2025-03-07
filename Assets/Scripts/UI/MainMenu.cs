using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle highContrastToggle;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider rightStickSensitivitySlider;
    [SerializeField] private Button bindJumpButton;
    [SerializeField] private Button bindJumpResetButton;
    [SerializeField] private Button bindJumpClearButton;
    [SerializeField] private Button bindInteractButton;
    [SerializeField] private Button bindInteractResetButton;
    [SerializeField] private Button bindInteractClearButton;
    [SerializeField] private Button bindMenuButton;
    [SerializeField] private Button bindMenuResetButton;
    [SerializeField] private Button bindMenuClearButton;
    [SerializeField] private Button bindSprintButton;
    [SerializeField] private Button bindSprintResetButton;
    [SerializeField] private Button bindSprintClearButton;
    [SerializeField] private Button bindCrouchButton;
    [SerializeField] private Button bindCrouchResetButton;
    [SerializeField] private Button bindCrouchClearButton;
    [SerializeField] private Button bindShipRiseButton;
    [SerializeField] private Button bindShipRiseResetButton;
    [SerializeField] private Button bindShipRiseClearButton;
    [SerializeField] private Button bindShipDropButton;
    [SerializeField] private Button bindShipDropResetButton;
    [SerializeField] private Button bindShipDropClearButton;
    [SerializeField] private Button bindSuperJumpButton;
    [SerializeField] private Button bindSuperJumpResetButton;
    [SerializeField] private Button bindSuperJumpClearButton;
    [SerializeField] private Button backToGameButton;
    [SerializeField] private Button backToOSButton;

    private class ActionBindUI
    {
        // TODO: Setup a way for one ActionBindUI to control multiple actions in parallel
        // (e.g. both Player.Interact and Flying.Interact)
        public MainMenu mainMenu;
        public InputAction action;
        public Button bindButton;
        public Button resetButton;
        public Button clearButton;

        private TextMeshProUGUI textObject;

        public void Setup()
        {
            bindButton.onClick.AddListener(Bind);
            resetButton.onClick.AddListener(ResetBind);
            clearButton.onClick.AddListener(ClearBind);
            textObject = bindButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            UpdateText();
        }

        private void Bind()
        {
            mainMenu.CleanupRebindOperation();
            SetText("Listening...");
            mainMenu.currentRebindingOperation =
                action.PerformInteractiveRebinding()
                    .OnComplete(_ => UpdateText())
                    .OnCancel(_ => UpdateText())
                    .Start();
        }

        private void ResetBind()
        {
            mainMenu.CleanupRebindOperation();
            action.RemoveAllBindingOverrides();
            UpdateText();
        }

        private void ClearBind()
        {
            // TODO
        }

        private void SetText(string what)
        {
            textObject.text = what;
        }

        private void UpdateText()
        {
            textObject.text = action.GetBindingDisplayString(0);
            // TODO: reflect all bindings, not just the first
        }
    }

    private InputModeManager inputMan;
    private InputSystem_Actions inputActions;
    private InputActionRebindingExtensions.RebindingOperation currentRebindingOperation = null;
    private InputModeManager.InputMode priorInputMode;

    private void CleanupRebindOperation()
    {
        if (
            currentRebindingOperation != null &&
            !currentRebindingOperation.completed &&
            !currentRebindingOperation.canceled
        )
        {
            currentRebindingOperation.Cancel();
        }
        currentRebindingOperation?.Dispose();
        currentRebindingOperation = null;
    }

    private ActionBindUI SetupActionBindUI(
        InputAction action, Button bindButton, Button resetButton, Button clearButton
    )
    {
        var abui = new ActionBindUI();
        abui.mainMenu = this;
        abui.action = action;
        abui.bindButton = bindButton;
        abui.resetButton = resetButton;
        abui.clearButton = clearButton;
        abui.Setup();
        return abui;
    }

    private void Awake()
    {
        inputMan = InputModeManager.Instance;
        inputActions = inputMan.inputActions;
    }

    private void OnEnable()
    {
        // TODO: certain UI input actions should navigate or close menu
        // w/o requiring use of on-screen buttons
    }

    private void Start()
    {
        transform.SetParent(SceneCore.canvas.transform, false);
        transform.SetLocalPositionAndRotation(3*Vector3.back, Quaternion.identity);
        priorInputMode = inputMan.inputMode;
        inputMan.SwitchToUIControls();
        // TODO: implement functionality for everything except buttons:
        //      fullScreenToggle
        //      volumeSlider
        //      languageDropdown
        //      highContrastToggle
        //      mouseSensitivitySlider
        //      rightStickSensitivitySlider
        SetupActionBindUI(
            inputActions.Player.Jump,
            bindJumpButton, bindJumpResetButton, bindJumpClearButton
        );
        SetupActionBindUI(
            inputActions.Player.Interact,
            bindInteractButton, bindInteractResetButton, bindInteractClearButton
        );
        SetupActionBindUI(
            inputActions.Player.Menu,
            bindMenuButton, bindMenuResetButton, bindMenuClearButton
        );
        SetupActionBindUI(
            inputActions.Player.Sprint,
            bindSprintButton, bindSprintResetButton, bindSprintClearButton
        );
        SetupActionBindUI(
            inputActions.Player.Crouch,
            bindCrouchButton, bindCrouchResetButton, bindCrouchClearButton
        );
        SetupActionBindUI(
            inputActions.Flying.Ascend,
            bindShipRiseButton, bindShipRiseResetButton, bindShipRiseClearButton
        );
        SetupActionBindUI(
            inputActions.Flying.Descend,
            bindShipDropButton, bindShipDropResetButton, bindShipDropClearButton
        );
        SetupActionBindUI(
            inputActions.Player.SuperJump,
            bindSuperJumpButton, bindSuperJumpResetButton, bindSuperJumpClearButton
        );
        backToGameButton.onClick.AddListener(() => Destroy(gameObject));
        // TODO: implement backToOSButton listener
    }

    private void OnDisable()
    {
        CleanupRebindOperation();
        switch (priorInputMode)
        {
            case InputModeManager.InputMode.Player:
                inputMan.SwitchToPlayerControls();
                break;
            case InputModeManager.InputMode.Flying:
                inputMan.SwitchToShipControls();
                break;
        }
    }
}
