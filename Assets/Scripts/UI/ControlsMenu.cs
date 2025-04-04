using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
            string deviceTypes;
            if (binding.groups == null)
            {
                deviceTypes = "";
            }
            else
            {
                deviceTypes = " (" +
                    String.Join(", ", binding.groups.Split(";",
                        StringSplitOptions.RemoveEmptyEntries
                    )) + ")";
            }
            if (binding.isPartOfComposite)
            {
                menuEntry.inputActionLabel.text =
                    $"{actionMap.name} {action.name} {binding.name}{deviceTypes}";
            }
            else
            {
                menuEntry.inputActionLabel.text =
                    $"{actionMap.name} {action.name}{deviceTypes}";
            }
        }

        private void SetControlLabel(string text)
        {
            menuEntry.rebindButtonLabel.text = text;
        }

        private void UpdateControlLabel()
        {
            binding = action.bindings[index];
            SetControlLabel(binding.ToDisplayString(
                InputBinding.DisplayStringOptions.DontOmitDevice
            ));
        }

        private void AfterRebind()
        {
            menu.CleanupRebindOperation();
            UpdateControlLabel();
        }

        private void Rebind()
        {
            menu.CleanupRebindOperation();
            menu.inputMan.DisableAllControls(); // so that UI controls can be rebound
            SetControlLabel("Listening...");
            menu.currentRebindingOperation =
                action.PerformInteractiveRebinding(index)
                    .OnComplete(_ => AfterRebind())
                    .OnCancel(_ => AfterRebind())
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

        public void Update()
        {
            if (menu.currentRebindingOperation == null)
            {
                UpdateControlLabel();
            }
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
    private List<ActionBindUI> actionBindUIs;

    override protected void Awake()
    {
        base.Awake();
        inputMan = InputModeManager.Instance;
        inputActions = inputMan.inputActions;
    }

    override protected void OnEnable()
    {
        base.OnEnable();
    }

    override protected void OnDisable()
    {
        base.OnDisable();
        CleanupRebindOperation();
        LookSensPrefs.SavePrefs();
        ControlRebindPrefs.SaveRebinds();
    }

    override protected bool CanDismiss()
    {
        return currentRebindingOperation == null;
    }

    void Start()
    {
        Populate();
        LookSensPrefs.LoadPrefs();
        mouseSensitivitySlider.value = LookSensPrefs.mouseSensitivity;
        mouseSensitivitySlider.onValueChanged.AddListener(value =>
            LookSensPrefs.mouseSensitivity = value);
        rightStickSensitivitySlider.value = LookSensPrefs.rightStickSensitivity;
        rightStickSensitivitySlider.onValueChanged.AddListener(value =>
            LookSensPrefs.rightStickSensitivity = value);
        backButton.onClick.AddListener(() => Return());
    }

    private void CleanupRebindOperation()
    {
        inputMan.SwitchToUIControls();
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
        actionBindUIs = new();
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
                        actionBindUIs.Add(abui);
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
        for (int i = 0; i < actionBindUIs.Count; i++)
        {
            ControlsMenuEntry menuEntryPrev = null, menuEntryCurr, menuEntryNext = null;
            if (i > 0) menuEntryPrev = actionBindUIs[i - 1].menuEntry;
            menuEntryCurr = actionBindUIs[i].menuEntry;
            if (i < actionBindUIs.Count - 1) menuEntryNext = actionBindUIs[i + 1].menuEntry;
            if (menuEntryPrev == null)
            {
                ControlsMenuEntry.ConnectNavigation(rightStickSensitivitySlider, menuEntryCurr);
            }
            else
            {
                ControlsMenuEntry.ConnectNavigation(menuEntryPrev, menuEntryCurr);
            }
            if (menuEntryNext == null)
            {
                ControlsMenuEntry.ConnectNavigation(menuEntryCurr, backButton);
            }
            else
            {
                ControlsMenuEntry.ConnectNavigation(menuEntryCurr, menuEntryNext);
            }

        }
        EventSystem.current?.SetSelectedGameObject(mouseSensitivitySlider.gameObject);
    }

    void Update()
    {
        foreach (var abui in actionBindUIs)
        {
            abui.Update();
        }
    }
}
