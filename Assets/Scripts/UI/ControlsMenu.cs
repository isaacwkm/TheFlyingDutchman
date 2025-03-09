using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlsMenu : UIStack.Context
{
    private class ActionBindUI
    {
        public ControlsMenu menu;
        public ControlsMenuEntry menuEntry;
        public InputActionMap actionMap;
        public InputAction action;
        public InputBinding binding;

        private int index;

        public void Activate()
        {
            index = action.bindings.IndexOf(value => value.id == binding.id);
            menuEntry.rebindButton.onClick.AddListener(Rebind);
            menuEntry.clearButton.onClick.AddListener(Clear);
            menuEntry.resetButton.onClick.AddListener(Reset);
            SetBindingLabel();
            UpdateControlLabel();
        }

        private void SetBindingLabel()
        {
            if (binding.isPartOfComposite)
            {
                menuEntry.inputActionLabel.text =
                    $"{actionMap.name} {action.name} {binding.name}";
            }
            else
            {
                menuEntry.inputActionLabel.text =
                    $"{actionMap.name} {action.name}";
            }
        }

        private void SetControlLabel(string text)
        {
            menuEntry.rebindButtonLabel.text = text;
        }

        private void UpdateControlLabel()
        {
            SetControlLabel(binding.ToDisplayString());
        }

        private void Rebind()
        {
            menu.CleanupRebindOperation();
            SetControlLabel("Listening...");
            menu.currentRebindingOperation =
                action.PerformInteractiveRebinding(index)
                    .OnComplete(_ => UpdateControlLabel())
                    .OnCancel(_ => UpdateControlLabel())
                    .Start();
        }

        private void Clear()
        {
            action.ApplyBindingOverride(index, "");
            SetControlLabel("[unbound]");
        }

        private void Reset()
        {
            action.RemoveBindingOverride(index);
            UpdateControlLabel();
        }
    }

    [SerializeField] private RectTransform mainArea;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider rightStickSensitivitySlider;
    [SerializeField] private Button backButton;
    [SerializeField] private ControlsMenuEntry menuEntryPrefab;
    [SerializeField] private MainMenu mainMenuPrefab;

    private InputModeManager inputMan;
    private InputSystem_Actions inputActions;
    private InputActionRebindingExtensions.RebindingOperation currentRebindingOperation = null;

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

    private void OnDisable()
    {
        CleanupRebindOperation();
    }

    private void Start()
    {
        Populate();
        backButton.onClick.AddListener(() => Return());
    }

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

    private void Populate()
    {
        float y = 96.0f;
        foreach (var actionMap in inputActions.asset.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                foreach (var binding in action.bindings)
                {
                    if (!binding.isComposite)
                    {
                        var abui = new ActionBindUI();
                        abui.menu = this;
                        abui.menuEntry = Instantiate(menuEntryPrefab);
                        var rt = abui.menuEntry.GetComponent<RectTransform>();
                        rt.SetParent(mainArea, false);
                        rt.ForceUpdateRectTransforms();
                        rt.anchoredPosition += Vector2.down*y;
                        y += 48.0f;
                        rt.ForceUpdateRectTransforms();
                        abui.actionMap = actionMap;
                        abui.action = action;
                        abui.binding = binding;
                        abui.Activate();
                    }
                }
            }
        }
        mainArea.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            y + 48.0f
        );
        mainArea.ForceUpdateRectTransforms();
    }
}
